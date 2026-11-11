using Anomalies.Assets.Effects;
using Terraria.Graphics.Shaders;
using static Anomalies.Bosses.Skeletron.LegendaryNight.SkeletronLegendHead.Mechanic6Parameters;
using static Anomalies.Bosses.Skeletron.LegendaryNight.SkeletronLegendHead.Mechanic6Timeline;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 可由玩家拦截、否则汇入中心恒星的金色/铂金光球。
/// </summary>
/// <remarks>
/// <para>ai[0]：颜色，0金色、1铂金；ai[1]：出生音乐帧；ai[2]：出生方位弧度。</para>
/// <para>localAI：未使用；框架 Timer1..5：未使用。</para>
/// <para>ExtraAI：中心恒星 owner + identity、SpawnRadius（Single）、Resolution（Byte）、TravelDuration（Int32）、InterceptingPlayerIndex（Int16，-1无人）。</para>
/// <para>结算视觉标记仅客户端使用；年龄由中心恒星所属主体的音乐时钟计算。</para>
/// </remarks>
public sealed class LegendLightOrb : AnomalyModProjectile
{
    private enum Resolution : byte
    {
        None,
        Intercepted,
        Absorbed,
        Incorrect
    }

    /// <summary>半径，单位为世界像素。</summary>
    public const float Radius = 32f;
    private ProjectileNetReference _SphereReference;
    private Resolution _Resolution;
    private bool _ResolutionFeedbackApplied;
    private int _InterceptingPlayerIndex = -1;

    public Projectile SphereProjectile
    {
        get => _SphereReference.Resolve();
        set => _SphereReference.Set(value);
    }
    public float SpawnRadius { get; set; }
    public int TravelDuration { get; set; } = LightOrbMinimumLifetime;
    private int OrbColorIndex => (int)Projectile.ai[0];
    private int SpawnMusicTime => (int)Projectile.ai[1];
    private float SpawnAngle => Projectile.ai[2];

    // 使用同步出生时间推导保底，沿用原有网络载荷。
    private int EffectiveTravelDuration => Math.Min(TravelDuration,
        LightOrbAbsorptionCompleteMusicTime - SpawnMusicTime);
    internal bool AvailableForInterception => _Resolution == Resolution.None && Age < EffectiveTravelDuration;
    private LegendEnergySphere Sphere => SphereProjectile.ModProjectile as LegendEnergySphere;
    private int Age => Sphere?.Head is SkeletronLegendHead head ? head.Phase2MusicElapsedTime - SpawnMusicTime : -1;

    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;
    public Color OrbColor => OrbColorIndex == 1
        ? LegendaryNightHandler.PlatinumColor : LegendaryNightHandler.StarColor;

    public override void SetStaticDefaults() => ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[Type] = true;

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
        if (sphere is null || sphere.Head is not SkeletronLegendHead { StellarMechanicsActive: true } head
            || head.CurrentBehavior == SkeletronLegendHead.Behavior.Despawn || head.Phase2Arena is null
            || head.Phase2MusicElapsedTime >= LightOrbEndMusicTime)
        {
            // 客户端等待主人/引用包，由服务器确认退场；不将取消误报为吸入。
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }
        int age = Age;
        Projectile.timeLeft = 120;
        Projectile.Center = sphere.Projectile.Center + SpawnAngle.ToRotationVector2()
            * LightOrbDistance(age, SpawnRadius, sphere.SphereRadius, EffectiveTravelDuration, TravelDuration);
        Projectile.rotation = SpawnAngle;
        Projectile.scale = LightOrbAppearance(age) * (1f - LightOrbAbsorption(age, EffectiveTravelDuration));
        Projectile.Opacity = LightOrbAppearance(age);
        if (TONetworkEnvironment.NotClient && TryResolveContact())
            return;
        if (!Main.dedServ)
        {
            Lighting.AddLight(Projectile.Center, OrbColor.ToVector3() * (0.6f * Projectile.scale));
            SpawnTrail();
        }

        void SpawnTrail()
        {
            const int TrailInterval = 2;
            const float TrailSpacing = 8f;
            const float TrailHalfWidthRatio = 0.7f;
            if (age <= 0 || age % TrailInterval != 0 || Projectile.scale <= 0f)
                return;

            // 按同步轨迹重建上一采样点，不使用收包瞬移距离；快速吸入时补足星芒间隔。
            Vector2 previous = sphere.Projectile.Center + SpawnAngle.ToRotationVector2()
                * LightOrbDistance(Math.Max(0, age - TrailInterval), SpawnRadius, sphere.SphereRadius, EffectiveTravelDuration, TravelDuration);
            Vector2 displacement = Projectile.Center - previous;
            if (displacement.LengthSquared() <= 0.0001f)
                return;
            int count = Math.Clamp((int)MathF.Ceiling(displacement.Length() / TrailSpacing), 1, 6);
            Vector2 backward = -displacement.SafeNormalize(Vector2.Zero);
            Vector2 sideways = new(-backward.Y, backward.X);
            Color color = OrbColor * (Projectile.Opacity * 0.85f);
            for (int i = 0; i < count; i++)
            {
                Vector2 position = Vector2.Lerp(previous, Projectile.Center, (i + Main.rand.NextFloat()) / count)
                    + backward * ((Radius + 2f) * Projectile.scale)
                    + sideways * (Main.rand.NextFloat(-1f, 1f) * Radius * TrailHalfWidthRatio * Projectile.scale);
                // 光点基本停留在经过的世界位置；横向撒开且缩小星芒，避免连成球体的尖长附肢。
                Vector2 velocity = Projectile.velocity.ToCustomLength(-3f) + Main.rand.NextPolarVector2(0.5f);
                sphere.SpawnLightOrbTrailParticle(new GenericSparkle(position, velocity, color, color * 0.35f,
                    Main.rand.NextFloat(0.14f, 0.2f) * Projectile.scale, Main.rand.Next(24, 37),
                    rotationSpeed: 0.02f, bloomScale: 0.8f));
            }
        }

        bool TryResolveContact()
        {
            // 不检查 immune、hurtCooldowns 或普通 CanHit；同一颗球只归属于首个接触玩家。
            foreach (Player player in Main.ActivePlayers)
            {
                if (player.dead || player.ghost || !OverlapsPlayer(player.Hitbox))
                    continue;
                bool correct = sphere.InterceptLightOrb(player.whoAmI, OrbColorIndex);
                _InterceptingPlayerIndex = player.whoAmI;
                Resolve(correct ? Resolution.Intercepted : Resolution.Incorrect);
                return true;
            }
            if (age >= EffectiveTravelDuration)
            {
                sphere.RecordLightOrbAbsorption();
                Resolve(Resolution.Absorbed);
                return true;
            }

            return false;
        }
    }

    public override bool ShouldUpdatePosition() => false;

    private void Resolve(Resolution resolution)
    {
        _Resolution = resolution;
        ApplyResolutionFeedback();
        // 删除包不携带 ExtraAI：先发送最终状态，再删除，客户端才能区分拦截、吸入与取消。
        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.SyncProjectile, number: Projectile.whoAmI);
        Projectile.Kill();
    }

    private void ApplyResolutionFeedback()
    {
        if (Main.dedServ || _ResolutionFeedbackApplied || _Resolution == Resolution.None)
            return;
        _ResolutionFeedbackApplied = true;
        if (_Resolution == Resolution.Absorbed)
        {
            Sphere?.FlashCore();
            return;
        }
        if (_InterceptingPlayerIndex is < 0 or >= Main.maxPlayers)
            return;
        Player target = Main.player[_InterceptingPlayerIndex];

        bool incorrect = _Resolution == Resolution.Incorrect;
        PlayInterceptionEffects(target, incorrect);
        ApplyIncorrectInterceptionDamage(target, incorrect);
    }

    private void PlayInterceptionEffects(Player target, bool incorrect)
    {
        SoundEngine.PlaySound(SoundID.Item35);

        bool platinum = OrbColorIndex == 1;
        Color textColor = incorrect ? Color.Red : platinum ? LegendaryNightHandler.PlatinumColor : Color.Gold;
        string text = incorrect ? "Punished~" : platinum ? "Life Regen UP!" : "Damage UP!";
        CombatText.NewText(new Rectangle((int)target.Center.X, (int)target.Top.Y, 0, 0),
            textColor, text, dramatic: true);

        const int ParticleCount = 40;
        for (int i = 0; i < ParticleCount; i++)
        {
            // 限制混白比例并关闭白色粒子核，保留金色/铂金色，避免过曝发白。
            Color color = Color.Lerp(OrbColor, Color.White, Main.rand.NextFloat(0.05f, 0.25f));
            ParticleHandler.SpawnParticle(new LegendaryNightAbsorptionParticle(target, Projectile.Center,
                Main.rand.NextPolarVector2(2f, 6f), color, Main.rand.NextFloat(0.35f, 0.65f), incorrect));
            if (incorrect && SphereProjectile is Projectile { active: true } sphere)
                ParticleHandler.SpawnParticle(new LegendaryNightAbsorptionParticle(target, target.Center,
                    Main.rand.NextPolarVector2(2f, 6f),
                    i % 2 == 0 ? LegendaryNightHandler.StarColor : LegendaryNightHandler.PlatinumColor,
                    Main.rand.NextFloat(0.35f, 0.65f), sphereTarget: sphere));
        }

    }

    private void ApplyIncorrectInterceptionDamage(Player target, bool incorrect)
    {
        if (incorrect && target.whoAmI == Main.myPlayer)
            LegendaryNightDamage.HurtLocalPlayer(Projectile, SkeletronLegendHead.IncorrectLightOrbDamage, true, ignoreAllImmunity: true);
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
        ApplyResolutionFeedback();
    }

    public override bool PreDraw(ref Color lightColor) => false;

    internal void Draw(SpriteBatch spriteBatch)
    {
        int age = Age;
        if (Main.dedServ || _Resolution != Resolution.None || age < 0 || age >= EffectiveTravelDuration)
            return;

        Vector2 position = Projectile.Center - Main.screenPosition;
        Vector2 drawScale = LightOrbDrawScale(age, EffectiveTravelDuration);
        AnomalyEffects.MiscShaderContainer shader = AnomalyClientConfig.Instance.EnableShaders
            ? AnomalyEffects.Primitives.LegendStellarLightOrb : null;
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
            .SetCustomParameter("uSpriteWorldSize", new Vector2(Radius * 4f) * drawScale)
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