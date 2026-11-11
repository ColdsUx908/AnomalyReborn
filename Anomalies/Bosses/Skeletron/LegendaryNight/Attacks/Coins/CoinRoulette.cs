namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 锁定金币种类后生成审判的轮盘载体。
/// </summary>
/// <remarks>
/// <para>ai[0]：目标玩家索引 + 1；ai[1]：一阶段首次锁定组（0未锁定、1铜/金、2银/铂），负数表示机制五该轮起始音乐帧；ai[2]：金币种类，0铜、1银、2金、3铂金。</para>
/// <para>localAI：未使用；框架 Timer1..5：未使用。</para>
/// <para>ExtraAI：MasterIndex（Int32，主体索引 + 1）、LockTime（Int32，每轮锁定帧）；本地换币和锁定反馈不参与同步。</para>
/// </remarks>
public sealed class CoinRoulette : AnomalyModProjectile
{
    /// <summary>切换音效。</summary>
    public static readonly SoundStyle SwitchSound = new(LegendaryNightHandler.LegendaryNightPath + "Attacks/Coins/CoinRouletteSwitch") { PitchVariance = 0.5f };
    /// <summary>锁定音效。</summary>
    public static readonly SoundStyle LockSound = new(LegendaryNightHandler.LegendaryNightPath + "Attacks/Coins/CoinRouletteLock");

    private int _LastVisualCoinType = -1;
    private int _LastVisualLockFrame = -1;

    /// <summary>自转时刻。</summary>
    private const int SpinTime = 24;
    internal const int FirstLockFrame = 90;
    internal const int RoundDuration = FirstLockFrame + CoinJudgment.WarningDuration;
    internal const int RotationDuration = RoundDuration * 2;
    private const float CoinScale = 3f;
    /// <summary>玩家上方高度。</summary>
    public const float HeightAbovePlayer = 64f;
    private bool IsMechanic5 => Projectile.ai[1] < 0f;
    private int RoundStartMusicTime => -(int)Projectile.ai[1];
    public int MasterIndex;
    public int LockTime = FirstLockFrame;
    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    protected override void SendAnomalyExtraAI(BinaryWriter writer)
    {
        writer.Write(MasterIndex);
        writer.Write(LockTime);
    }

    protected override void ReceiveAnomalyExtraAI(BinaryReader reader)
    {
        MasterIndex = reader.ReadInt32();
        LockTime = reader.ReadInt32();
    }

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.netImportant = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = RotationDuration + 1;
    }

    public override bool? CanDamage() => false;
    public override bool ShouldUpdatePosition() => false;

    public override void AI()
    {
        if (!TryGetHead(out SkeletronLegendHead head))
        {
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }

        if (IsMechanic5)
        {
            int age = head.Phase2MusicElapsedTime - RoundStartMusicTime;
            if (head.Target is Player rouletteTarget)
                Projectile.Center = rouletteTarget.Center - Vector2.UnitY * HeightAbovePlayer;
            if (TONetworkEnvironment.NotClient)
            {
                if (age == LockTime && head.Phase2Arena is LegendaryNightPhase2Arena arena)
                {
                    int round = (RoundStartMusicTime - SkeletronLegendHead.Mechanic5Timeline.InterludeStartMusicTime)
                        / SkeletronLegendHead.Mechanic5Parameters.JudgmentInterval;
                    Projectile.ai[2] = head.SelectMechanic5JudgmentCoin(round);
                    Projectile.netUpdate = true;
                    SpawnJudgment(arena.Projectile.Center, -head.Phase2MusicElapsedTime);
                }
                else if (age > 0 && age < LockTime && age % SpinTime == 0)
                {
                    Projectile.ai[2] = ((int)Projectile.ai[2] + Main.rand.Next(1, 4)) % 4;
                    Projectile.netUpdate = true;
                }
            }
            UpdateVisualEffects(age, age);
            return;
        }

        if (!Player.TryGetPlayerFromIndex((int)Projectile.ai[0] - 1, out Player target) || !target.active || target.dead)
        {
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }

        int timer = head.Timer1;
        Projectile.Center = target.Center - Vector2.UnitY * HeightAbovePlayer;
        int roundTime = timer % RoundDuration;
        if (TONetworkEnvironment.NotClient)
        {
            if (timer > 0 && roundTime < LockTime && roundTime % SpinTime == 0)
            {
                int coinType = (int)Projectile.ai[2];
                if (timer >= RoundDuration && roundTime + SpinTime >= LockTime)
                {
                    // 铜和金的编号为偶数，银和铂的编号为奇数。
                    int requiredParity = 2 - (int)Projectile.ai[1];
                    Projectile.ai[2] = (coinType & 1) == requiredParity ? coinType ^ 2 : requiredParity + 2 * Main.rand.Next(2);
                }
                else
                    Projectile.ai[2] = (coinType + Main.rand.Next(1, 4)) % 4;
                Projectile.netUpdate = true;
            }

            if (roundTime == LockTime)
            {
                if (timer < RoundDuration)
                {
                    Projectile.ai[1] = ((int)Projectile.ai[2] & 1) + 1;
                    Projectile.netUpdate = true;
                }
                SpawnJudgment(Projectile.Center, timer);
            }
        }
        UpdateVisualEffects(timer, roundTime);
    }

    private bool TryGetHead(out SkeletronLegendHead head) =>
        LegendaryNightHandler.TryGetLegendaryNightHead(MasterIndex, out head, oneBased: true)
        && (IsMechanic5 ? head.Mechanic5Active : head.CurrentPhase == SkeletronLegendHead.Phase.Phase1
            && head.CurrentBehavior == SkeletronLegendHead.Behavior.Spin && head.CurrentSpell == SkeletronLegendHead.Spell.PlatinumCoin);

    private void SpawnJudgment(Vector2 center, int lockFrame)
    {
        Projectile.NewProjectileAction<CoinJudgment>(SourceAI, center, Vector2.Zero, SkeletronLegendHead.CoinJudgmentDamage, 0f, action: p =>
        {
            p.ai[0] = Projectile.ai[2];
            p.ai[1] = lockFrame;
            p.ai[2] = MasterIndex;
        });
    }

    private void UpdateVisualEffects(int timer, int roundTime)
    {
        if (Main.dedServ)
            return;

        int coinType = (int)Projectile.ai[2];
        // 检测同步后的币种，保证客户端也能播放服务端决定的切换效果。
        if (roundTime >= LockTime)
        {
            int lockFrame = timer - roundTime + LockTime;
            if (_LastVisualLockFrame != lockFrame)
            {
                _LastVisualLockFrame = lockFrame;
                ReleaseCoinParticles(coinType, 48, 7f);
                SoundEngine.PlaySound(LockSound, Projectile.Center);
            }
        }
        else if (_LastVisualCoinType >= 0 && _LastVisualCoinType != coinType)
        {
            ReleaseCoinParticles(coinType, 16, 4f);
            SoundEngine.PlaySound(SwitchSound, Projectile.Center);
        }
        _LastVisualCoinType = coinType;
    }
    public static Color GetCoinColor(int coinType) => coinType switch
    {
        0 => new Color(210, 120, 65),
        1 => new Color(190, 205, 220),
        2 => LegendaryNightHandler.StarColor,
        _ => LegendaryNightHandler.PlatinumColor
    };

    private void ReleaseCoinParticles(int coinType, int count, float maxSpeed)
    {
        Color color = GetCoinColor(coinType);
        Vector2 center = Projectile.Center;
        for (int i = 0; i < count; i++)
        {
            ParticleHandler.SpawnParticle(new OrbParticle(center + Main.rand.NextVector2Circular(8f, 8f),
                Main.rand.NextPolarVector2(1f, maxSpeed), Main.rand.Next(20, 36),
                Main.rand.NextFloat(0.25f, 0.55f), color, lifeEndRatio: 0.2f));
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (!TryGetHead(out SkeletronLegendHead head))
            return false;

        int timer = IsMechanic5 ? head.Phase2MusicElapsedTime - RoundStartMusicTime : head.Timer1;
        if (timer < 0 || IsMechanic5 && timer >= SkeletronLegendHead.Mechanic5Parameters.JudgmentInterval)
            return false;
        int coinType = (int)Projectile.ai[2];
        Vector2 center = Projectile.Center - Main.screenPosition;
        Color color = GetCoinColor(coinType);
        float glow = timer % RoundDuration < LockTime ? 0.8f + 0.2f * MathF.Cos(timer * 0.9f) : 0.8f;
        DrawCoin(center, coinType, timer % SpinTime * 8 / SpinTime, 1f, glow);

        string text = coinType switch
        {
            0 => "3",
            1 => "15",
            2 => "60",
            _ => "591"
        };
        const float TextScale = 0.9f;
        DynamicSpriteFont font = FontAssets.MouseText.Value;
        Vector2 position = center - font.MeasureString(text) * new Vector2(0.5f, 1f) * TextScale - Vector2.UnitY * 23f;
        TODrawUtils.DrawBorderString(Main.spriteBatch, font, text, position, (color with { A = 0 }) * 0.28f, 12, 4f, TextScale, 0f);
        TODrawUtils.DrawStringWithBorder(Main.spriteBatch, font, text, position, color, Color.Black,
            borderWidth: 2f, scale: TextScale);
        return false;
    }

    internal static void DrawCoin(Vector2 screenCenter, int coinType, int frameIndex, float opacity, float glow)
    {
        Texture2D texture = TextureAssets.Coin[coinType].Value;
        Rectangle frame = texture.Frame(1, 8, 0, frameIndex);
        frame.Height -= 2;
        Texture2D bloom = ParticleHandler.GetTexture<BloomParticle>();
        Color halo = GetCoinColor(coinType) with { A = 0 };
        Main.spriteBatch.DrawFromCenter(bloom, screenCenter, null, halo * (opacity * glow), scale: 85f / bloom.Width);
        Main.spriteBatch.DrawFromCenter(texture, screenCenter, frame, Color.White * opacity, scale: CoinScale);
    }
}