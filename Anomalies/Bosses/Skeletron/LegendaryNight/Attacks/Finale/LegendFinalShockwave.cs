namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 终幕陨星命中的冲击波，成功时展开无伤害的金纹光环。
/// </summary>
/// <remarks>
/// <para>ai[0]、ai[2]：保持0，使用独立寿命计时；ai[1]：最大扩张半径；damage：0为成功演出，1为失败秒杀。</para>
/// <para>localAI：未使用；框架 Timer1..5：未使用；继承共享冲击波协议，无额外载荷。</para>
/// <para>主体死亡后独立完成扩张和失败判定，使用原版绘制入口保持可见。</para>
/// </remarks>
public sealed class LegendFinalShockwave : LegendStellarShockwave
{
    internal const float FailureScreenshake = 18f;
    private const int FailureShellDuration = 300;
    private const int FailureDuration = 360;
    private const int SuccessDuration = 240;
    private const int StarBurstAge = 28;
    private const float OuterShaderRadiusScale = 1.95f;

    private bool _Hit;
    private bool _ImpactPlayed;
    private bool Success => Projectile.damage == 0;
    protected override bool YellowPalette => true;
    protected override string ExplosionPass => Success ? "CurtainCall" : "SupernovaRemnant";
    protected override float ShaderRadiusScale => OuterShaderRadiusScale;
    protected override int Lifetime => Success ? SuccessDuration : FailureDuration;
    // SetDefaults runs before damage distinguishes success from failure.
    protected override int InitialTimeLeft => FailureDuration;
    protected override float VisualOpacity => 1f;
    protected override Color OuterColor => Success ? new Color(255, 171, 24, 0) : base.OuterColor;
    protected override Color InnerColor => Success ? new Color(255, 225, 98, 0) : base.InnerColor;
    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;

    protected override float OuterExpansion(int age) => CoastingOuterExpansion(age);

    protected override Vector4 RemnantState(int age)
    {
        if (Success)
        {
            const int OuterFadeStart = 60;
            float outerFade = 1f - MathHelper.SmoothStep(0f, 1f, (age - OuterFadeStart) / (float)(SuccessDuration - 1 - OuterFadeStart));
            return new Vector4(0f, 1f, 0f, outerFade);
        }

        return NebulaRemnantState(age, FailureShellDuration);
    }

    protected override Vector3 InteriorState(int age)
    {
        if (Success)
        {
            float burst = MathHelper.SmoothStep(0f, 1f, (age - StarBurstAge) / 20f);
            return new Vector3(burst, burst, 1f - burst);
        }

        return NebulaInteriorState(age, Lifetime);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        base.PreDraw(ref lightColor);
        int age = Age;
        if (Success && age >= StarBurstAge && age < Lifetime)
        {
            DrawFireworkBurst(age - StarBurstAge, CompanionShockwaveProgress(StarBurstAge) * 0.69f, 140);
            DrawFireworkBurst(age - StarBurstAge - 13, 0.29f, 80);
        }
        return false;
    }

    private void DrawFireworkBurst(int age, float radiusRatio, int sparkCount)
    {
        if (age < 0 || age >= 156)
            return;

        float radius = Projectile.ai[1];
        float starRadius = radius * radiusRatio;
        Vector2 center = Projectile.Center - Main.screenPosition;
        Texture2D glow = ParticleHandler.GetTexture<BloomParticle>();
        Texture2D pixel = TOTextures.FillerTexture;
        SpriteBatch spriteBatch = Main.spriteBatch;

        for (int i = 0; i < sparkCount; i++)
        {
            float variation = i * 0.61803399f % 1f;
            float lifetime = MathHelper.Lerp(72f, 156f, variation);
            if (age >= lifetime)
                continue;

            float along = (i + 0.5f) * 10f / sparkCount;
            int edge = (int)along;
            Vector2 start = StarVertex(edge);
            Vector2 end = StarVertex(edge + 1);
            Vector2 outline = Vector2.Lerp(start, end, along - edge);
            Vector2 direction = Vector2.Normalize(outline);
            Vector2 origin = center + outline * starRadius;
            float scatter = (i * 0.41421356f % 1f - 0.5f) * 0.55f;
            Vector2 velocity = direction.RotatedBy(scatter) * (radius * MathHelper.Lerp(0.002f, 0.0075f, variation));
            float gravity = radius * MathHelper.Lerp(0.000055f, 0.000105f, variation);
            float progress = age / lifetime;
            float flicker = 0.78f + 0.22f * MathF.Sin(age * 0.73f + i * 2.39f);
            float fade = (1f - MathHelper.SmoothStep(0f, 1f, (progress - 0.25f) / 0.75f)) * flicker;
            Color hotGold = new(255, 221, 80, 0);
            Color ember = new(255, 87, 9, 0);
            Color spark = Color.Lerp(hotGold, ember, progress) * fade;
            Vector2 head = SparkPosition(age, origin, velocity, gravity);
            Vector2 bend = SparkPosition(Math.Max(0, age - 4), origin, velocity, gravity);
            Vector2 tail = SparkPosition(Math.Max(0, age - 10), origin, velocity, gravity);
            float width = MathHelper.Lerp(1.4f, 3.6f, variation) * (1f - progress * 0.65f);
            DrawTrail(bend, tail, width * 0.55f, spark * 0.48f);
            DrawTrail(head, bend, width, spark);
            spriteBatch.DrawFromCenter(glow, head, null, spark * 0.85f, scale: width * 6f / glow.Width);
            spriteBatch.DrawFromCenter(glow, head, null, new Color(255, 244, 173, 0) * fade, scale: width * 2.5f / glow.Width);
        }

        static Vector2 StarVertex(int index)
        {
            float angle = index * MathHelper.Pi / 5f - MathHelper.PiOver2;
            return angle.ToRotationVector2() * (index % 2 == 0 ? 0.96f : 0.48f);
        }

        static Vector2 SparkPosition(int time, Vector2 origin, Vector2 velocity, float gravity)
            => origin + velocity * (time / (1f + time / 30f)) + Vector2.UnitY * (0.5f * gravity * time * time);

        void DrawTrail(Vector2 start, Vector2 end, float width, Color color)
        {
            Vector2 segment = start - end;
            spriteBatch.DrawFromCenter_VectorScale(pixel, (start + end) * 0.5f, null, color,
                segment.ToRotation(), new Vector2(segment.Length(), width) / pixel.Size());
        }
    }

    protected override void UpdateExplosion(int age)
    {
        PlayImpactOnce();
        if (Success || _Hit || age >= CompanionExplosionDuration)
            return;
        Player player = Main.LocalPlayer;
        float radius = Projectile.ai[1] * CompanionShockwaveProgress(age);
        if (player.active && !player.dead && !player.ghost && new Circle(Projectile.Center, radius).Collides(player.Hitbox))
        {
            _Hit = true;
            player.KillMe(PlayerDeathReason.ByCustomReason(Language.GetText(
                "Mods.Anomalies." + LegendaryNightHandler.LocalizationCategory + ".TrialFailedDeath").ToNetworkText(player.name)), SkeletronLegendHead.TrialFailedDamage, 0);
        }

        void PlayImpactOnce()
        {
            if (!_ImpactPlayed)
            {
                _ImpactPlayed = true;
                if (!Success)
                {
                    const int ParticleCount = 320;
                    for (int i = 0; i < ParticleCount; i++)
                    {
                        Color color = i % 3 == 0 ? Color.Gold
                            : Color.Lerp(LegendaryNightHandler.BoneColor, Color.White, Main.rand.NextFloat());
                        ParticleHandler.SpawnParticle(new OrbParticle(Projectile.Center,
                            Main.rand.NextPolarVector2(5f, 35f), Main.rand.Next(40, 85),
                            Main.rand.NextFloat(0.5f, 1.6f), color, lifeEndRatio: 0.2f));
                    }
                }
                SoundEngine.PlaySound(Meteor.ExplosionSound with { Volume = Success ? 0.45f : 1f }, Projectile.Center);
                Main.LocalPlayer.SetScreenshake(Success ? 3f : FailureScreenshake);
            }
        }
    }
}
