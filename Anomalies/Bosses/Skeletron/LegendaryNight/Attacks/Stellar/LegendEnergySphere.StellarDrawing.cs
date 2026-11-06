using Anomalies.Assets.Effects;

using static Anomalies.Bosses.Skeletron.LegendaryNight.LegendBlueStar;
using static Anomalies.Bosses.Skeletron.LegendaryNight.SkeletronLegendHead.Mechanic6Timeline;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendEnergySphere
{
    internal static float PrimaryPhotosphereProgress(float companionAge) => MathHelper.SmoothStep(0f, 1f,
        MathHelper.Clamp((companionAge - 180f) / 600f, 0f, 1f));
    private float PrimaryPhotosphere => PrimaryPhotosphereProgress((Head?.Phase2MusicElapsedTime ?? 0) - CompanionStartMusicTime);

    private float OrangeStarProgress => SkeletronLegendHead.Mechanic7Parameters.TransformationProgress(Head?.Phase2MusicElapsedTime ?? 0);

    private void DrawStellarPhotosphere(SpriteBatch spriteBatch, Vector2 center, float radius, bool blue, float opacity)
    {
        AnomalyEffects.MiscShaderContainer shader = AnomalyClientConfig.Instance.EnableShaders
            ? AnomalyEffects.Overlays.LegendStellarPhotosphere : null;
        float dissipation = blue ? 0f : SkeletronLegendHead.Mechanic7Parameters.StarDissipation(Head?.Phase2MusicElapsedTime ?? 0);
        if (dissipation >= 1f)
            return;
        Texture2D bloom = ParticleHandler.GetTexture<BloomParticle>();
        spriteBatch.DrawFromCenter(bloom, center, null,
            (blue ? new Color(55, 155, 255, 0) : Color.Lerp(new Color(255, 196, 72, 0), new Color(255, 104, 12, 0), OrangeStarProgress)) * (opacity * 0.22f * (1f - MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(dissipation / 0.55f, 0f, 1f)))),
            scale: radius * 5f / bloom.Width);
        if (shader is null)
        {
            DrawPhotosphereFallback(spriteBatch, center, radius, blue, opacity);
            return;
        }
        float time = ((Head?.Phase2MusicElapsedTime ?? 0) - CompanionStartMusicTime) / 60f;
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        shader.Data.SetCustomParameter("uNebulaNoiseTexture", AnomalyTextures.Noise.Perlin2)
            .SetCustomParameter("uGlobalTime", time)
            .SetCustomParameter("uSpin", time * (blue ? 0.85f : 0.38f))
            .SetCustomParameter("uBluePalette", blue ? 1f : 0f)
            .SetCustomParameter("uOrangeProgress", blue ? 0f : OrangeStarProgress)
            .SetCustomParameter("uDissipation", MathHelper.Lerp(-0.1f, 1.1f, dissipation))
            .SetCustomParameter("uCoronaOpacity", MathHelper.Clamp(1f - dissipation * 2f, 0f, 1f))
            .SetCustomParameter("uCoreFlash", blue ? 0f : CoreFlash)
            .Apply();
        Texture2D pixel = TOTextures.FillerTexture;
        spriteBatch.DrawFromCenter(pixel, center, null, Color.White * opacity, scale: radius * 4f / pixel.Width);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    }
    private void DrawPhotosphereFallback(SpriteBatch spriteBatch, Vector2 center, float radius, bool blue, float opacity)
    {
        Texture2D circle = AnomalyTextures.Extra.BasicCircleLarge;
        float dissipation = blue ? 0f : SkeletronLegendHead.Mechanic7Parameters.StarDissipation(Head?.Phase2MusicElapsedTime ?? 0);
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
}