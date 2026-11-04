using Anomalies.Assets.Effects;
using Terraria.Graphics.Shaders;
using static Anomalies.Bosses.Skeletron.LegendaryNight.SkeletronLegendHead.Mechanic6Data;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 机制6小光球。ai[0]：颜色（0金/1铂金）；ai[1]：出生音乐帧；ai[2]：出生方位。
/// ExtraAI：大球的owner+identity引用、出生半径、结算状态。按主人音乐时钟恢复漂移与吸入。
/// </summary>
public sealed class LegendLightOrb : AnomalyModProjectile
{
    private enum Resolution : byte
    {
        None,
        Intercepted,
        Absorbed
    }

    public const float Radius = 16f;
    private ProjectileNetReference _SphereReference;
    private Resolution _Resolution;
    private bool _ResolutionVisualsPlayed;
    private int _InterceptingPlayerIndex = -1;

    public Projectile SphereProjectile
    {
        get => _SphereReference.Resolve();
        set => _SphereReference.Set(value);
    }
    public float SpawnRadius { get; set; }
    public int TravelDuration { get; set; } = LightOrbMinimumLifetime;
    internal bool AvailableForInterception => _Resolution == Resolution.None && Age < TravelDuration;
    private LegendEnergySphere Sphere => SphereProjectile.ModProjectile as LegendEnergySphere;
    private int Age => Sphere?.Head is { } head ? head.Phase2MusicElapsedTime - (int)Projectile.ai[1] : -1;

    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;
    public Color OrbColor => (int)Projectile.ai[0] == 1
        ? LegendaryNightHandler.PlatinumColor : LegendaryNightHandler.StarColor;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = (int)(Radius * 2f);
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.netImportant = true;
        Projectile.hide = true;
        Projectile.timeLeft = LightOrbMinimumLifetime + 120;
    }

    // AI 独立检测拦截，不进入原版伤害/免疫碰撞路径。
    public override bool? CanDamage() => false;

    internal bool OverlapsPlayer(Rectangle targetHitbox) => Projectile.scale > 0f
        && new Circle(Projectile.Center, Radius * Projectile.scale).Collides(targetHitbox);

    public override void AI()
    {
        if (_Resolution != Resolution.None)
            return;
        LegendEnergySphere sphere = Sphere;
        if (sphere is null || sphere.Head is not { Mechanic6Active: true } head
            || head.CurrentBaseBehavior == SkeletronLegendHead.BaseBehavior.Despawn || head.Phase2Arena is null
            || head.Phase2MusicElapsedTime >= LightOrbEndMusicTime)
        {
            // 客户端等待主人/引用包，由服务器确认退场；不将取消误报为吸入。
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }
        int age = Age;
        Projectile.timeLeft = 120;
        Projectile.Center = sphere.Projectile.Center + Projectile.ai[2].ToRotationVector2()
            * LightOrbDistance(age, SpawnRadius, sphere.SphereRadius, TravelDuration);
        Projectile.rotation = Projectile.ai[2];
        Projectile.scale = LightOrbAppearance(age) * (1f - LightOrbAbsorption(age, TravelDuration));
        Projectile.Opacity = LightOrbAppearance(age);
        if (TONetworkEnvironment.NotClient)
        {
            // 不检查 immune、hurtCooldowns 或普通 CanHit；同一颗球只归属于首个接触玩家。
            foreach (Player player in Main.ActivePlayers)
            {
                if (player.dead || player.ghost || !OverlapsPlayer(player.Hitbox))
                    continue;
                sphere.InterceptLightOrb(player.whoAmI, (int)Projectile.ai[0]);
                _InterceptingPlayerIndex = player.whoAmI;
                Resolve(Resolution.Intercepted);
                return;
            }
            if (age >= TravelDuration)
            {
                _Resolution = Resolution.Absorbed;
                sphere.OnLightOrbAbsorbed(this);
                Resolve(Resolution.Absorbed);
                return;
            }
        }
        if (!Main.dedServ)
        {
            Lighting.AddLight(Projectile.Center, OrbColor.ToVector3() * (0.6f * Projectile.scale));
            SpawnTrail();
        }

        void SpawnTrail()
        {
            const int TrailInterval = 1;
            const float TrailSpacing = 8f;
            if (age <= 0 || age % TrailInterval != 0 || Projectile.scale <= 0f)
                return;

            // 按同步轨迹重建上一采样点，不使用收包瞬移距离；快速吸入时补足星芒间隔。
            Vector2 previous = sphere.Projectile.Center + Projectile.ai[2].ToRotationVector2()
                * LightOrbDistance(Math.Max(0, age - TrailInterval), SpawnRadius, sphere.SphereRadius, TravelDuration);
            Vector2 displacement = Projectile.Center - previous;
            if (displacement.LengthSquared() <= 0.0001f)
                return;
            int count = Math.Clamp((int)MathF.Ceiling(displacement.Length() / TrailSpacing), 1, 6);
            Vector2 backward = -displacement.SafeNormalize(Vector2.Zero);
            Color color = OrbColor * Projectile.Opacity;
            for (int i = 0; i < count; i++)
            {
                Vector2 position = Vector2.Lerp(previous, Projectile.Center, (i + Main.rand.NextFloat()) / count)
                    + backward * (Radius * Projectile.scale + 4f)
                    + Main.rand.NextVector2Circular(3f, 3f) * Projectile.scale;
                Vector2 velocity = backward.RotatedBy(Main.rand.NextFloat(-0.22f, 0.22f)) * Main.rand.NextFloat(1.5f, 3.5f);
                sphere.SpawnLightOrbTrailParticle(new GenericSparkle(position, velocity, color, color * 0.85f,
                    Main.rand.NextFloat(0.3f, 0.45f) * Projectile.scale, Main.rand.Next(24, 37),
                    rotationSpeed: 0.035f, bloomScale: 1.2f));
            }
        }
    }

    public override bool ShouldUpdatePosition() => false;

    private void Resolve(Resolution resolution)
    {
        _Resolution = resolution;
        PlayResolutionVisuals();
        // 删除包不携带 ExtraAI：先发送最终状态，再删除，客户端才能区分拦截、吸入与取消。
        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.SyncProjectile, number: Projectile.whoAmI);
        Projectile.Kill();
    }

    private void PlayResolutionVisuals()
    {
        if (Main.dedServ || _ResolutionVisualsPlayed || _Resolution == Resolution.None)
            return;
        _ResolutionVisualsPlayed = true;
        if (_Resolution == Resolution.Absorbed)
        {
            Sphere?.FlashCore();
            return;
        }
        if (_InterceptingPlayerIndex < 0 || _InterceptingPlayerIndex >= Main.maxPlayers)
            return;
        Player target = Main.player[_InterceptingPlayerIndex];
        const int ParticleCount = 40;
        for (int i = 0; i < ParticleCount; i++)
        {
            // 限制混白比例并关闭白色粒子核，保留金色/铂金色，避免过曝发白。
            Color color = Color.Lerp(OrbColor, Color.White, Main.rand.NextFloat(0.05f, 0.25f));
            ParticleHandler.SpawnParticle(new LegendaryNightAbsorptionParticle(target, Projectile.Center,
                Main.rand.NextPolarVector2(2f, 6f), color, Main.rand.NextFloat(0.35f, 0.65f)));
        }
    }

    protected override void SendAnomalyExtraAI(BinaryWriter writer)
    {
        base.SendAnomalyExtraAI(writer);
        _SphereReference.Write(writer);
        writer.Write(SpawnRadius);
        writer.Write((byte)_Resolution);
        writer.Write(TravelDuration);
        writer.Write((short)_InterceptingPlayerIndex);
    }

    protected override void ReceiveAnomalyExtraAI(BinaryReader reader)
    {
        base.ReceiveAnomalyExtraAI(reader);
        _SphereReference = ProjectileNetReference.Read(reader);
        SpawnRadius = reader.ReadSingle();
        _Resolution = (Resolution)reader.ReadByte();
        TravelDuration = reader.ReadInt32();
        _InterceptingPlayerIndex = reader.ReadInt16();
        PlayResolutionVisuals();
    }

    public override bool PreDraw(ref Color lightColor) => false;

    internal void Draw(SpriteBatch spriteBatch)
    {
        int age = Age;
        if (Main.dedServ || _Resolution != Resolution.None || age < 0 || age >= TravelDuration)
            return;

        Vector2 position = Projectile.Center - Main.screenPosition;
        Vector2 drawScale = LightOrbDrawScale(age, TravelDuration);
        AnomalyEffects.MiscShaderContainer shader = AnomalyClientConfig.Instance.EnableShaders
            ? AnomalyEffects.Primitives.LegendLightOrb : null;
        if (shader is null)
        {
            Texture2D circle = AnomalyTextures.Extra.BasicCircle;
            if (circle is null)
                return;
            Vector2 origin = circle.Size() * 0.5f;
            Vector2 scale = new Vector2(Radius * 2f) / circle.Size() * drawScale;
            spriteBatch.Draw(circle, position, null, OrbColor * Projectile.Opacity,
                Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
            return;
        }

        // 正方形局部UV直接表示世界等长坐标，无需屏幕长宽比修正。
        Texture2D pixel = TOTextures.FillerTexture;
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        MiscShaderData data = shader.Data;
        data.SetCustomParameter("uOrbColor", OrbColor.ToVector3())
            .SetCustomParameter("uGlobalTime", (float)Main.timeForVisualEffects / 60f + Projectile.ai[1] / 60f + Projectile.ai[2])
            .SetCustomParameter("uNebulaNoiseTexture", AnomalyTextures.Noise.Perlin2);
        data.UseOpacity(Projectile.Opacity);
        data.Apply();
        spriteBatch.Draw(pixel, position, null, Color.White, Projectile.rotation, pixel.Size() * 0.5f,
            new Vector2(Radius * 4f) / pixel.Size() * drawScale, SpriteEffects.None, 0f);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    }
}
