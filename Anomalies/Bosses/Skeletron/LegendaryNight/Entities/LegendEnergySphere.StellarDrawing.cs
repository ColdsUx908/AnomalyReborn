using Anomalies.Assets.Effects;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendEnergySphere
{
    private float OrangeStarProgress => SkeletronLegendHead.Mechanic7Data.TransformationProgress(Head?.Phase2MusicElapsedTime ?? 0);

    private void DrawStellarPhotosphere(SpriteBatch spriteBatch, Vector2 center, float radius, bool blue, float opacity)
    {
        AnomalyEffects.MiscShaderContainer shader = AnomalyClientConfig.Instance.EnableShaders
            ? AnomalyEffects.Overlays.LegendStellarPhotosphere : null;
        float dissipation = blue ? 0f : SkeletronLegendHead.Mechanic7Data.StarDissipation(Head?.Phase2MusicElapsedTime ?? 0);
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
}
