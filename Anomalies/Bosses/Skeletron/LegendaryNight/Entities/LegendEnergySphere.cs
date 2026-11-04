using Anomalies.Assets.Effects;
using ReLogic.Utilities;
using static Anomalies.Bosses.Skeletron.LegendaryNight.SkeletronLegendHead.Mechanic6Data;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>ai[0]：主人索引+1；光球蓄力完成后持续保持。</summary>
public sealed class LegendEnergySphere : AnomalyModProjectile
{
    // 光球与其激光共用的基础色，着色器不再另写一份色值。
    public static Vector3 GoldColor => new(1f, 0.82f, 0.06f);
    public static Vector3 StarlightColor => new(1f, 0.98f, 0.8f);

    private const string AssetPath = LegendaryNightHandler.LegendaryNightEntityPath + "EnergySphere/";
    private static readonly SoundStyle _ChargeSound = new(AssetPath + "TeslaSphereLoop") { IsLooped = true, Volume = 0.45f };
    private SlotId _ChargeSoundSlot;
    private bool _SoundStarted;
    private double _CoreFlashStart = double.NegativeInfinity;
    private LightOrbTrailSet _LightOrbTrails;

    private sealed class LightOrbTrailSet : ParticleSet
    {
        public override int ParticleLifetime => 0;

        public LightOrbTrailSet() : base(0, 0f) => ParticleLimit = 1024;

        public override Particle SpawnParticle() =>
            throw new InvalidOperationException("Light orb trails accept explicit submissions only.");
    }
    internal struct LightOrbBonuses
    {
        public int DamageStacks;
        public int RegenerationStacks;
        public bool ExpectsPlatinum;

        public bool ApplyCollision(int color, bool noRequiredColorRemains)
        {
            if ((color == 1) != ExpectsPlatinum && !noRequiredColorRemains)
            {
                DamageStacks--;
                RegenerationStacks--;
                return false;
            }
            if (color == 1)
                RegenerationStacks++;
            else
                DamageStacks++;
            ExpectsPlatinum = !ExpectsPlatinum;
            return true;
        }
    }

    private readonly LightOrbBonuses[] _PlayerBonuses = new LightOrbBonuses[Main.maxPlayers];
    private int _LightOrbWavesSpawned;

    /// <summary>小球抵达球心时仅由服务器/单人端触发一次；不含拦截或退场取消。</summary>
    public event Action<LegendLightOrb> LightOrbAbsorbed;

    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;
    private NPC Master => NPC.GetNPCFromIndex((int)Projectile.ai[0] - 1);
    internal SkeletronLegendHead Head => LegendaryNightHandler.IsLegendaryNightHead((int)Projectile.ai[0], out SkeletronLegendHead head, oneBased: true) ? head : null;
    internal float SphereRadius => Head is { } head ? SkeletronLegendHead.Mechanic6Data.Radius(head.Phase2MusicElapsedTime) : 0f;
    private float CoreFlash => LightOrbCoreFlash((float)(Main.timeForVisualEffects - _CoreFlashStart));

    internal void FlashCore()
    {
        if (!Main.dedServ)
            _CoreFlashStart = Main.timeForVisualEffects;
    }

    internal void OnLightOrbAbsorbed(LegendLightOrb orb)
    {
        if (!TONetworkEnvironment.NotClient)
            return;
        const int StarCount = 50;
        for (int i = 0; i < StarCount; i++)
        {
            Projectile.NewProjectileAction<FallingStar>(SourceAI, Projectile.Center,
                Main.rand.NextPolarVector2(4f, 6f), SkeletronLegendHead.FallingStarDamage, 0f,
                action: p =>
                {
                    p.ai[2] = Projectile.ai[0];
                    p.scale = 0.2f;
                    p.Resize(16, 16);
                    p.timeLeft = 600;
                    FallingStar star = p.GetModProjectile<FallingStar>();
                    star.TrailType = 1; // 既有的蓝白光尾。
                    star.AccelerationStartTime = 300;
                    p.netUpdate = true;
                });
        }
        LightOrbAbsorbed?.Invoke(orb);
    }

    internal LightOrbBonuses GetBonuses(int playerIndex) => _PlayerBonuses[playerIndex];

    internal void InterceptLightOrb(int playerIndex, int color)
    {
        if (!TONetworkEnvironment.NotClient)
            return;
        int requiredColor = _PlayerBonuses[playerIndex].ExpectsPlatinum ? 1 : 0;
        bool noRequiredColorRemains = _LightOrbWavesSpawned >= LightOrbWaveCount;
        if (noRequiredColorRemains)
        {
            foreach (Projectile projectile in Projectile.ActiveProjectiles)
            {
                if (projectile.ModProjectile is LegendLightOrb orb && orb.SphereProjectile == Projectile
                    && orb.AvailableForInterception && (int)projectile.ai[0] == requiredColor)
                {
                    noRequiredColorRemains = false;
                    break;
                }
            }
        }
        if (!_PlayerBonuses[playerIndex].ApplyCollision(color, noRequiredColorRemains))
            SpawnLasers(PunishmentLaserCount, punishment: true);
        Projectile.netUpdate = true;
    }

    internal void SpawnLasers(int count, bool punishment = false)
    {
        if (!TONetworkEnvironment.NotClient || Head is not { } head || head.Phase2Arena is not { } arena)
            return;
        int time = head.Phase2MusicElapsedTime;
        // 全轮共用偏移，保留束间夹角约束，同时消除固定扇区边界上的方向空缺。
        float angleOffset = Main.rand.NextFloat(MathHelper.TwoPi);
        for (int sector = 0; sector < count; sector++)
        {
            float angle = GetLaserAngle(sector, count, Main.rand.NextFloat()) + angleOffset;
            float turnAngle = GetLaserTurnAngle(Main.rand.NextFloat());
            Projectile.NewProjectileAction<LegendEnergyLaser>(SourceAI, arena.Projectile.Center,
                Vector2.Zero, SkeletronLegendHead.FallingStarDamage, 0f, action: p =>
                {
                    p.ai[0] = Projectile.ai[0];
                    p.ai[1] = angle;
                    p.ai[2] = time;
                    LegendEnergyLaser laser = p.GetModProjectile<LegendEnergyLaser>();
                    laser.TurnAngle = turnAngle;
                    laser.Punishment = punishment;
                    p.netUpdate = true;
                });
        }
    }

    protected override void SendAnomalyExtraAI(BinaryWriter writer)
    {
        base.SendAnomalyExtraAI(writer);
        writer.Write(_LightOrbWavesSpawned);
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            writer.Write(_PlayerBonuses[i].DamageStacks);
            writer.Write(_PlayerBonuses[i].RegenerationStacks);
            writer.Write(_PlayerBonuses[i].ExpectsPlatinum);
        }
    }

    protected override void ReceiveAnomalyExtraAI(BinaryReader reader)
    {
        base.ReceiveAnomalyExtraAI(reader);
        int wavesSpawned = reader.ReadInt32();
        LightOrbBonuses[] bonuses = new LightOrbBonuses[Main.maxPlayers];
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            bonuses[i].DamageStacks = reader.ReadInt32();
            bonuses[i].RegenerationStacks = reader.ReadInt32();
            bonuses[i].ExpectsPlatinum = reader.ReadBoolean();
        }
        bonuses.CopyTo(_PlayerBonuses, 0);
        _LightOrbWavesSpawned = wavesSpawned;
    }

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 16;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.netImportant = true;
        Projectile.hide = true;
        Projectile.timeLeft = 1200;
    }

    public override void AI()
    {
        SkeletronLegendHead head = Head;
        if (head is not { Mechanic6Active: true } || head.CurrentBaseBehavior == SkeletronLegendHead.BaseBehavior.Despawn)
        {
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }
        int time = head.Phase2MusicElapsedTime;
        Projectile.timeLeft = 100;
        Projectile.Center = head.Phase2Arena is { } arena ? arena.Projectile.Center
            : Master.Center + SkeletronLegendHead.Mechanic6Data.SphereOffset(time);
        Projectile.velocity = Vector2.Zero;
        if (TONetworkEnvironment.NotClient)
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active || (_PlayerBonuses[i].DamageStacks == 0
                    && _PlayerBonuses[i].RegenerationStacks == 0 && !_PlayerBonuses[i].ExpectsPlatinum))
                    continue;
                _PlayerBonuses[i] = default;
                Projectile.netUpdate = true;
            }
        }
        if (TONetworkEnvironment.NotClient && ShouldSpawnLightOrb(time) && head.Phase2Arena is { } orbArena)
        {
            SpawnLightOrbWave(orbArena);
        }
        if (TONetworkEnvironment.NotClient && ShouldSpawnLaser(time))
            SpawnLasers(LaserCount);
        if (Main.dedServ)
            return;
        _LightOrbTrails?.Update();
        Lighting.AddLight(Projectile.Center, LegendaryNightHandler.StarColor.ToVector3() * 1.5f);
        if (!_SoundStarted)
        {
            _SoundStarted = true;
            _ChargeSoundSlot = SoundEngine.PlaySound(_ChargeSound, Projectile.Center, sound => Projectile.active);
        }
        if (SoundEngine.TryGetActiveSound(_ChargeSoundSlot, out ActiveSound activeSound))
        {
            activeSound.Position = Projectile.Center;
            activeSound.Volume = 0.15f + 0.5f * SkeletronLegendHead.Mechanic6Data.ChargeProgress(time);
        }
        const int AbsorptionParticleCount = 10;
        for (int i = 0; i < AbsorptionParticleCount; i++)
        {
            // 起点覆盖整圈并位于球面之外，沿直线快速汇聚为星光能量。
            float angle = MathHelper.TwoPi * (i + Main.rand.NextFloat()) / AbsorptionParticleCount;
            float distance = SphereRadius + Main.rand.NextFloat(100f, 260f);
            Vector2 origin = Projectile.Center + angle.ToRotationVector2() * distance;
            ParticleHandler.SpawnParticle(new LegendaryNightStellarMatterParticle(Projectile, origin,
                Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(12, 21)));
        }

        void SpawnLightOrbWave(LegendaryNightPhase2Arena arena)
        {
            int wave = (time - LightOrbFirstMusicTime) / LightOrbInterval;
            if (wave < _LightOrbWavesSpawned)
                return;
            if (wave == 0)
                TOLocalizationUtils.ChatLocalizedText(AnomalySharedData.ModLocalizationPrefix
                    + LegendaryNightHandler.LocalizationCategory + ".InterceptStarPower", LegendaryNightHandler.StarColor);
            int[] colors = CreateLightOrbColors(wave);
            for (int i = colors.Length - 1; i > 0; i--)
            {
                int other = Main.rand.Next(i + 1);
                (colors[i], colors[other]) = (colors[other], colors[i]);
            }
            for (int sector = 0; sector < LightOrbWaveSize; sector++)
            {
                float angle = GetLightOrbAngle(sector, Main.rand.NextFloat());
                int color = colors[sector];
                int lifetime = Main.rand.Next(GetLightOrbLifetime(wave, 0f), GetLightOrbLifetime(wave, 1f) + 1);
                Projectile.NewProjectileAction<LegendLightOrb>(SourceAI,
                    arena.Projectile.Center + angle.ToRotationVector2() * arena.Radius, Vector2.Zero, 0, 0f,
                    action: p =>
                    {
                        p.ai[0] = color;
                        p.ai[1] = time;
                        p.ai[2] = angle;
                        LegendLightOrb orb = p.GetModProjectile<LegendLightOrb>();
                        orb.SphereProjectile = Projectile;
                        orb.SpawnRadius = arena.Radius;
                        orb.TravelDuration = lifetime;
                        p.scale = 0f;
                        p.Opacity = 0f;
                        p.netUpdate = true;
                    });
            }
            _LightOrbWavesSpawned = wave + 1;
            Projectile.netUpdate = true;
        }
    }

    public override bool CanHitPlayer(Player target) => false;
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => new Circle(Projectile.Center, SphereRadius).Collides(targetHitbox);

    public override void OnKill(int timeLeft)
    {
        _LightOrbTrails?.Clear();
        _LightOrbTrails = null;
        if (!Main.dedServ && _SoundStarted && SoundEngine.TryGetActiveSound(_ChargeSoundSlot, out ActiveSound sound))
            sound.Stop();
    }

    internal void SpawnLightOrbTrailParticle(GenericSparkle particle)
    {
        if (!Main.dedServ)
            (_LightOrbTrails ??= new LightOrbTrailSet()).TrySpawnParticle(particle);
    }

    internal void DrawLightOrbTrails(SpriteBatch spriteBatch)
    {
        if (Main.dedServ || _LightOrbTrails is null || _LightOrbTrails.Particles.Count == 0)
            return;
        // 全局粒子早于竞技场覆盖层绘制；尾迹在球面之后单独加亮，避免被雾幕和球面遮挡。
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        _LightOrbTrails.DrawSet(Vector2.Zero);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    }

    public override bool PreDraw(ref Color lightColor) => false;

    // 由竞技场在同一个绘制入口中按球面、小球、星核的顺序调用。
    internal void DrawBody(SpriteBatch spriteBatch)
    {
        if (Main.dedServ || SphereRadius <= 0f)
            return;
        Vector2 center = Projectile.Center - Main.screenPosition;
        Texture2D bloom = ParticleHandler.GetTexture<BloomParticle>();
        spriteBatch.DrawFromCenter(bloom, center, null, new Color(255, 190, 20, 0) * 0.16f,
            scale: SphereRadius * 5f / bloom.Width);
        AnomalyEffects.MiscShaderContainer shader = AnomalyClientConfig.Instance.EnableShaders
            ? AnomalyEffects.Overlays.LegendEnergySphere : null;
        if (shader is null)
        {
            Texture2D body = AnomalyTextures.Extra.BasicCircleLarge;
            spriteBatch.DrawFromCenter(body, center, null, new Color(255, 105, 8),
                scale: SphereRadius * 2f / body.Width);
            spriteBatch.DrawFromCenter(bloom, center, null, new Color(255, 235, 120, 0) * 0.7f,
                scale: SphereRadius * 2f / bloom.Width);
        }
        else
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            shader.Data.SetCustomParameter("uGlobalTime", (float)Main.timeForVisualEffects / 60f)
                .SetCustomParameter("uNebulaNoiseTexture", AnomalyTextures.Noise.Perlin2)
                .SetCustomParameter("uGoldColor", GoldColor)
                .SetCustomParameter("uStarlightColor", StarlightColor)
                .SetCustomParameter("uCoreFlash", CoreFlash)
                .Apply();
            Texture2D pixel = TOTextures.FillerTexture;
            spriteBatch.DrawFromCenter(pixel, center, null, Color.White, scale: SphereRadius * 4f / pixel.Width);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        // 关闭着色器时同样保留可见的流动星辰，不只显示纯光晕。
        if (shader is null)
        {
            Texture2D sparkle = ParticleHandler.GetTexture<GenericSparkle>();
            float time = (float)Main.timeForVisualEffects / 60f;
            for (int i = 0; i < 48; i++)
            {
                float distance = MathF.Sqrt((i + 0.5f) / 48f);
                float angle = i * 2.399963f + time * (0.2f + distance * 0.4f);
                Vector2 offset = angle.ToRotationVector2() * (SphereRadius * distance * 0.92f);
                float twinkle = 0.6f + 0.4f * MathF.Sin(time * 2f + i * 1.7f);
                spriteBatch.DrawFromCenter(sparkle, center + offset, null,
                    new Color(255, 235, 175, 0) * twinkle, time * 0.15f,
                    (4f + 4f * twinkle) / sparkle.Width);
            }
        }
    }

    internal void DrawCore(SpriteBatch spriteBatch)
    {
        if (Main.dedServ || SphereRadius <= 0f)
            return;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float flash = CoreFlash;
        AnomalyEffects.MiscShaderContainer shader = AnomalyClientConfig.Instance.EnableShaders
            ? AnomalyEffects.Overlays.LegendEnergySphere : null;
        if (shader is null)
        {
            Texture2D bloom = ParticleHandler.GetTexture<BloomParticle>();
            Texture2D sparkle = ParticleHandler.GetTexture<GenericSparkle>();
            float pulse = 1f + 0.08f * MathF.Sin((float)Main.timeForVisualEffects * 0.15f);
            float coreSize = SphereRadius * (1f + flash * 0.65f) * pulse;
            spriteBatch.DrawFromCenter(bloom, center, null, new Color(255, 235, 160, 0) * (0.8f + flash),
                scale: coreSize * 0.8f / bloom.Width);
            spriteBatch.DrawFromCenter(sparkle, center, null, new Color(255, 250, 220, 0) * (1f + flash),
                (float)Main.timeForVisualEffects * 0.005f, coreSize * 0.65f / sparkle.Width);
            return;
        }
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        shader.Data.SetCustomParameter("uGlobalTime", (float)Main.timeForVisualEffects / 60f)
            .SetCustomParameter("uNebulaNoiseTexture", AnomalyTextures.Noise.Perlin2)
            .SetCustomParameter("uGoldColor", GoldColor)
            .SetCustomParameter("uStarlightColor", StarlightColor)
            .SetCustomParameter("uCoreFlash", flash)
            .Apply();
        shader.Asset.Value.CurrentTechnique.Passes["Core"].Apply();
        Texture2D pixel = TOTextures.FillerTexture;
        spriteBatch.DrawFromCenter(pixel, center, null, Color.White, scale: SphereRadius * 4f / pixel.Width);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    }
}
