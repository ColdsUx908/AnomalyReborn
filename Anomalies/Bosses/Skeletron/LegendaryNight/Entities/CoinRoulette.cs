namespace Anomalies.Bosses.Skeletron.LegendaryNight.Entities;

// ai[0]：目标玩家（1-based）；ai[1]：计时；ai[2]：铜/银/金/铂金币类型。
public sealed class CoinRoulette : AnomalyModProjectile
{
    public static readonly SoundStyle SwitchSound = new(LegendaryNightHandler.LegendaryNightEntityPath + "CoinRouletteSwitch") { PitchVariance = 0.5f };
    public static readonly SoundStyle LockSound = new(LegendaryNightHandler.LegendaryNightEntityPath + "CoinRouletteLock");

    private int _LastVisualCoinType = -1;
    private bool _LockEffectsPlayed;

    private const int SpinTime = 24;
    private const int PauseTime = 4;
    public const int CycleTime = SpinTime + PauseTime;
    public const int MarkDuration = CoinJudgment.WarningDuration;
    public const float HeightAbovePlayer = 64f;
    public int MasterIndex;
    public int LockTime = 120;
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
        Projectile.timeLeft = 600;
    }

    public override bool? CanDamage() => false;
    public override bool ShouldUpdatePosition() => false;

    public override void AI()
    {
        NPC master = NPC.GetNPCFromIndex(MasterIndex - 1);
        if (!master.active || master.type != NPCID.SkeletronHead || master.Anomaly.CustomAIType != 1
            || SkeletronLegendHead.GetInstance(master).CurrentAvatar != SkeletronLegendHead.Avatar.PlatinumCoin
            || !Player.TryGetPlayerFromIndex((int)Projectile.ai[0] - 1, out Player target) || !target.active || target.dead)
        {
            if (TOSharedData.NotClient)
                Projectile.Kill();
            return;
        }

        int timer = (int)Projectile.ai[1] + 1;
        Projectile.ai[1] = timer;
        if (timer <= LockTime)
            Projectile.Center = target.Center - Vector2.UnitY * HeightAbovePlayer;

        if (timer < LockTime && timer % CycleTime == SpinTime && TOSharedData.NotClient)
        {
            // 每圈结束时换为另一种币，然后短暂停顿。
            Projectile.ai[2] = ((int)Projectile.ai[2] + Main.rand.Next(1, 4)) % 4;
            Projectile.netUpdate = true;
        }

        if (timer == LockTime)
        {
            Projectile.netUpdate = true;
            if (TOSharedData.NotClient)
            {
                Projectile.NewProjectileAction<CoinJudgment>(SourceAI, Projectile.Center, Vector2.Zero, SkeletronLegendHead.CoinJudgmentDamage, 0f, action: p =>
                {
                    p.ai[0] = Projectile.ai[2];
                    p.ai[2] = MasterIndex;
                    p.netUpdate = true;
                });
            }
        }
        UpdateVisualEffects(timer);
        if (timer >= LockTime + MarkDuration)
            Projectile.Kill();
    }

    private void UpdateVisualEffects(int timer)
    {
        if (Main.dedServ)
            return;

        int coinType = (int)Projectile.ai[2];
        // 检测同步后的币种，保证客户端也能播放服务端决定的切换效果。
        if (timer >= LockTime)
        {
            if (!_LockEffectsPlayed)
            {
                _LockEffectsPlayed = true;
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
        int timer = (int)Projectile.ai[1];
        bool locked = timer >= LockTime;
        Texture2D texture = TextureAssets.Coin[(int)Projectile.ai[2]].Value;
        int cycleTimer = timer % CycleTime;
        int frameIndex = locked || cycleTimer >= SpinTime ? 0 : cycleTimer * 8 / SpinTime;
        Rectangle frame = texture.Frame(1, 8, 0, frameIndex);
        frame.Height -= 2;
        float opacity = locked ? MathHelper.Clamp((LockTime + MarkDuration - timer) / 15f, 0f, 1f) : 1f;
        Main.spriteBatch.DrawFromCenter(texture, Projectile.Center - Main.screenPosition,
            frame, Color.White * opacity, 0f, 2f);
        return false;
    }
}
