using Anomalies.Assets.Effects;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendEnergySphere
{
    internal const int CompanionStartMusicTime = 138 * 60 + 45;
    internal const int CompanionImpactMusicTime = 153 * 60;
    internal const int CompanionExplosionDuration = 72;
    internal const float CompanionRadiusRatio = 0.55f;
    internal static float CompanionShockwaveRadiusRatio(int count) => count == 0 ? 0.9f : 1.2f;
    internal static int CompanionShockwaveDamage(int count) => count == 0 ? 100 : (int)(100d * Math.Pow(count, 1.5d));
    internal static float CompanionShockwaveProgress(int age)
    {
        float progress = MathHelper.Clamp(age / (float)(CompanionExplosionDuration - 1), 0f, 1f);
        return TOMathUtils.Interpolation.CubicEaseOut(progress);
    }
    private Vector2 CompanionImpactCenter => Head.Phase2Arena.Projectile.Center;
    private float CompanionImpactMaximumRadius => Head.Phase2Arena.Radius * CompanionShockwaveRadiusRatio(AbsorbedLightOrbCount);
    private bool _CompanionShockwaveHit;

    private void UpdateCompanionShockwaveDamage()
    {
        int age = Head.Phase2MusicElapsedTime - CompanionImpactMusicTime;
        if (age < 0 || age >= CompanionExplosionDuration || _CompanionShockwaveHit || Head.Phase2Arena is null)
            return;
        Player player = Main.LocalPlayer;
        if (!player.active || player.dead || player.ghost)
            return;
        // 扩张圆盘覆盖帧间扫过区域，每次爆发每位本机玩家只结算一次。
        float radius = CompanionImpactMaximumRadius * CompanionShockwaveProgress(age);
        if (new Circle(CompanionImpactCenter, radius).Collides(player.Hitbox))
        {
            _CompanionShockwaveHit = true;
            LegendaryNightDamage.HurtLocalPlayer(Projectile, CompanionShockwaveDamage(AbsorbedLightOrbCount), false);
        }
    }

    private const float CompanionInitialOrbitRadius = 1900f;
    private const float CompanionInitialAngularSpeed = 0.009f;
    private const float CompanionCameraDistance = 1400f;
    private const float CompanionCameraInclination = 0.24f;
    private const int CompanionDebrisCount = 1800;
    private const int CompanionDebrisDuration = 120;
    private static Vector3 CompanionSurfaceColor => new(0.36f, 0.56f, 1f);
    private static Vector3 CompanionCoreColor => new(0.85f, 0.92f, 1f);

    // 客户端视觉归属于大球；运动、爆炸均由既有同步音乐时钟恢复，不增加网络载荷。
    private LightOrbTrailSet _CompanionImpactParticles;
    private bool _CompanionImpactPlayed;

    private static readonly float[] _CompanionOrbitAngles = CreateCompanionOrbitAngles();

    internal static float CompanionOrbitRadius(float age)
    {
        const float Duration = CompanionImpactMusicTime - CompanionStartMusicTime;
        const float ContactRadius = SkeletronLegendHead.Mechanic6Data.SphereRadius * (1f + CompanionRadiusRatio);
        float progress = MathHelper.Clamp(age / Duration, 0f, 1f);
        // 前70%时间仍在场地外；径向衰减集中在最后约4秒，连续加速而非突然切换。
        return MathHelper.Lerp(CompanionInitialOrbitRadius, ContactRadius, MathF.Pow(progress, 5f));
    }

    internal static float CompanionAngularSpeed(float age)
    {
        float radius = CompanionOrbitRadius(age);
        return CompanionInitialAngularSpeed * CompanionInitialOrbitRadius * CompanionInitialOrbitRadius / (radius * radius);
    }

    private static float[] CreateCompanionOrbitAngles()
    {
        const int Duration = CompanionImpactMusicTime - CompanionStartMusicTime;
        float[] angles = new float[Duration + 1];
        double angle = -MathHelper.PiOver2;
        angles[0] = (float)angle;
        for (int i = 1; i <= Duration; i++)
        {
            // 固定音乐帧的中点积分表，各端独立恢复相同相位，不依赖AI帧推进。
            angle += CompanionAngularSpeed(i - 0.5f);
            angles[i] = (float)angle;
        }
        return angles;
    }

    internal static float CompanionOrbitAngle(float age)
    {
        float elapsed = MathHelper.Clamp(age, 0f, _CompanionOrbitAngles.Length - 1);
        int frame = (int)elapsed;
        return MathHelper.Lerp(_CompanionOrbitAngles[frame],
            _CompanionOrbitAngles[Math.Min(frame + 1, _CompanionOrbitAngles.Length - 1)], elapsed - frame);
    }

    internal static Vector3 CompanionOrbitPosition(float age)
    {
        float radius = CompanionOrbitRadius(age);
        float angle = CompanionOrbitAngle(age);
        return new Vector3(MathF.Cos(angle) * radius, 0f, MathF.Sin(angle) * radius);
    }

    internal static float PrimaryPhotosphereProgress(float companionAge) => MathHelper.SmoothStep(0f, 1f,
        MathHelper.Clamp((companionAge - 180f) / 600f, 0f, 1f));

    private float PrimaryPhotosphere => PrimaryPhotosphereProgress((Head?.Phase2MusicElapsedTime ?? 0) - CompanionStartMusicTime);
    internal static bool ProjectCompanion(Vector3 position, out Vector2 offset, out float scale, out float opacity)
    {
        const float NearPlane = 180f;
        const float FadeDepth = 900f;
        // 摄像机略向下俯视，轨道本身始终为 Y=0 的 xOz 圆周；近侧放大，远侧缩小。
        float depth = CompanionCameraDistance + position.Z * MathF.Cos(CompanionCameraInclination) + position.Y * MathF.Sin(CompanionCameraInclination);
        scale = CompanionCameraDistance / Math.Max(depth, NearPlane);
        offset = new Vector2(position.X, position.Y * MathF.Cos(CompanionCameraInclination) - position.Z * MathF.Sin(CompanionCameraInclination)) * scale;
        opacity = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((depth - NearPlane) / FadeDepth, 0f, 1f));
        return depth > NearPlane;
    }

    private void UpdateBlueCompanion()
    {
        PrepareCompanionMatter(Head.Phase2MusicElapsedTime);
        int age = Head.Phase2MusicElapsedTime - CompanionImpactMusicTime;
        if (Head.Phase2Arena is null || age < 0)
            return;
        if (!_CompanionImpactPlayed)
        {
            _CompanionImpactPlayed = true;
            // 晚加入可恢复冲击波，过期的声音和瞬时爆裂不重播。
            if (age < CompanionExplosionDuration)
            {
                FlashCore();
                if (age <= 6)
                {
                    float proximity = 1f - MathHelper.Clamp(Main.LocalPlayer.Distance(CompanionImpactCenter) / 3000f, 0f, 1f);
                    Main.LocalPlayer.SetScreenshake(14f * proximity);
                    SoundEngine.PlaySound(SoundID.Item14 with { Volume = 1.4f, Pitch = -0.65f }, CompanionImpactCenter);
                }
                _CompanionImpactParticles = new LightOrbTrailSet(CompanionDebrisCount);
                for (int i = 0; i < CompanionDebrisCount; i++)
                {
                    float angle = MathHelper.TwoPi * (i + Main.rand.NextFloat()) / CompanionDebrisCount;
                    Color color = Color.Lerp(new Color(75, 135, 255), new Color(255, 224, 165), Main.rand.NextFloat());
                    _CompanionImpactParticles.TrySpawnParticle(new GenericSparkle(
                        CompanionImpactCenter + Main.rand.NextPolarVector2(32f),
                        angle.ToRotationVector2() * Main.rand.NextFloat(3f, 25f), color, color * 0.35f,
                        Main.rand.NextFloat(0.12f, 0.42f), Main.rand.Next(60, CompanionDebrisDuration + 1),
                        rotationSpeed: 0.025f, bloomScale: 1.2f));
                }
            }
        }
        _CompanionImpactParticles?.Update();
        if (age >= CompanionDebrisDuration)
        {
            _CompanionImpactParticles?.Clear();
            _CompanionImpactParticles = null;
        }
    }

    private void DrawPhotosphereFallback(SpriteBatch spriteBatch, Vector2 center, float radius, bool blue, float opacity)
    {
        Texture2D circle = AnomalyTextures.Extra.BasicCircleLarge;
        float dissipation = blue ? 0f : SkeletronLegendHead.Mechanic7Data.StarDissipation(Head?.Phase2MusicElapsedTime ?? 0);
        Color cool = blue ? new Color(219, 217, 207) : Color.Lerp(new Color(225, 164, 58), new Color(205, 55, 5), OrangeStarProgress);
        Color hot = blue ? new Color(205, 230, 255) : Color.Lerp(new Color(255, 225, 142), new Color(255, 163, 32), OrangeStarProgress);
        spriteBatch.DrawFromCenter(circle, center, null, cool * opacity * (1f - MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(dissipation / 0.65f, 0f, 1f))), scale: radius * 2f / circle.Width);
        float spin = ((Head?.Phase2MusicElapsedTime ?? 0) - CompanionStartMusicTime) / 60f * (blue ? 0.85f : 0.38f);
        for (int latitude = 0; latitude < 14; latitude++)
        {
            float y = -0.94f + latitude * (1.88f / 13f);
            float ringRadius = MathF.Sqrt(1f - y * y);
            for (int longitude = 0; longitude < 40; longitude++)
            {
                float angle = longitude * MathHelper.TwoPi / 40f + spin;
                Vector3 normal = new(MathF.Cos(angle) * ringRadius, y, MathF.Sin(angle) * ringRadius);
                normal = new(normal.X, normal.Y * 0.94f - normal.Z * 0.342f, normal.Z * 0.94f + normal.Y * 0.342f);
                if (normal.Z <= 0f)
                    continue;
                float granule = 0.5f + 0.5f * MathF.Sin(latitude * 13.7f + longitude * 5.1f);
                Color color = Color.Lerp(cool, hot, 0.3f + granule * 0.6f) * ((0.4f + 0.6f * normal.Z) * opacity
                    * MathHelper.Clamp((granule + 1f - dissipation * 2f) * 5f, 0f, 1f));
                spriteBatch.DrawFromCenter(circle, center + new Vector2(normal.X, normal.Y) * (radius * 0.94f), null,
                    color, scale: radius * 0.16f * MathF.Sqrt(normal.Z) / circle.Width);
            }
        }
    }
    internal void DrawBlueCompanion(SpriteBatch spriteBatch, bool behind)
    {
        int time = Head?.Phase2MusicElapsedTime ?? -1;
        if (time < CompanionStartMusicTime || time >= CompanionImpactMusicTime)
            return;
        Vector3 position = CompanionOrbitPosition(time - CompanionStartMusicTime);
        if ((position.Z > 0f) != behind || !ProjectCompanion(position, out Vector2 offset, out float scale, out float opacity))
            return;
        float radius = SphereRadius * CompanionRadiusRatio * scale;
        Vector2 center = Projectile.Center + offset;
        DrawBody(spriteBatch, center, radius, blue: true, opacity: opacity);
        // 不叠加平面星核：蓝星使用完整不透明的旋转光球层。
    }

    internal void DrawCompanionImpact(SpriteBatch spriteBatch)
    {
        int age = (Head?.Phase2MusicElapsedTime ?? -1) - CompanionImpactMusicTime;
        if (Head?.Phase2Arena is null || age < 0 || age >= CompanionDebrisDuration)
            return;
        float maximumRadius = CompanionImpactMaximumRadius;
        if (age < CompanionExplosionDuration)
        {
            float progress = age / (float)(CompanionExplosionDuration - 1);
            float opacity = 1f - MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((progress - 0.82f) / 0.18f, 0f, 1f));
            AnomalyEffects.MiscShaderContainer shader = AnomalyClientConfig.Instance.EnableShaders
                ? AnomalyEffects.Overlays.LegendCompanionExplosion : null;
            if (shader is not null)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                shader.Data.SetCustomParameter("uGlobalTime", age / 60f)
                    .SetCustomParameter("uLifetimeRatio", progress)
                    .SetCustomParameter("uYellowPalette", 0f)
                    .SetCustomParameter("uExpansionRatio", CompanionShockwaveProgress(age))
                    .SetCustomParameter("uNoiseTexture", AnomalyTextures.Noise.Perlin2)
                    .SetCustomParameter("uTextureSize", new Vector2(maximumRadius * 2f))
                    .Apply();
                Texture2D pixel = TOTextures.FillerTexture;
                spriteBatch.DrawFromCenter(pixel, CompanionImpactCenter - Main.screenPosition, null,
                    Color.White * opacity, scale: maximumRadius * 2f / pixel.Width);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                // 回退亮环与伤害扩张使用同一进度，线宽包含在半径上限内。
                const int Segments = 240;
                float radius = Math.Max(0f, CompanionShockwaveProgress(age) * maximumRadius - 10f);
                Texture2D pixel = TOTextures.FillerTexture;
                for (int i = 0; i < Segments; i++)
                {
                    Vector2 start = CompanionImpactCenter + (MathHelper.TwoPi * i / Segments).ToRotationVector2() * radius;
                    Vector2 end = CompanionImpactCenter + (MathHelper.TwoPi * (i + 1) / Segments).ToRotationVector2() * radius;
                    Vector2 edge = end - start;
                    Vector2 center = (start + end) * 0.5f - Main.screenPosition;
                    spriteBatch.DrawFromCenter_VectorScale(pixel, center, null, new Color(70, 130, 255, 0) * (opacity * 0.85f),
                        edge.ToRotation(), new Vector2(edge.Length() + 1f, 20f) / pixel.Size());
                    spriteBatch.DrawFromCenter_VectorScale(pixel, center, null, new Color(195, 225, 255, 0) * opacity,
                        edge.ToRotation(), new Vector2(edge.Length() + 1f, 6f) / pixel.Size());
                }
            }
        }
        if (_CompanionImpactParticles is not null)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            _CompanionImpactParticles.DrawSet(Vector2.Zero);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
