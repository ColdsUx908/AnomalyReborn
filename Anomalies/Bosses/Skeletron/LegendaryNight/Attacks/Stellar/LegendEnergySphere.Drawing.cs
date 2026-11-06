using Anomalies.Assets.Effects;

using static Anomalies.Bosses.Skeletron.LegendaryNight.LegendBlueStar;
using static Anomalies.Bosses.Skeletron.LegendaryNight.SkeletronLegendHead.Mechanic6Timeline;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendEnergySphere
{
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
    internal void DrawBody(SpriteBatch spriteBatch, Vector2? worldCenter = null, float? drawRadius = null, bool blue = false, float opacity = 1f)
    {
        float radius = drawRadius ?? SphereRadius;
        if (Main.dedServ || radius <= 0f)
            return;
        float photosphere = blue ? 1f : PrimaryPhotosphere;
        Vector2 center = (worldCenter ?? Projectile.Center) - Main.screenPosition;
        if (photosphere >= 1f)
        {
            DrawStellarPhotosphere(spriteBatch, center, radius, blue, opacity);
            return;
        }
        Texture2D bloom = ParticleHandler.GetTexture<BloomParticle>();
        spriteBatch.DrawFromCenter(bloom, center, null, (blue ? new Color(90, 140, 255, 0) : new Color(255, 190, 20, 0)) * opacity * (1f - photosphere) * 0.16f,
            scale: radius * 5f / bloom.Width);
        AnomalyEffects.MiscShaderContainer shader = AnomalyClientConfig.Instance.EnableShaders
            ? AnomalyEffects.Overlays.LegendStellarSphere : null;
        if (shader is null)
        {
            Texture2D body = AnomalyTextures.Extra.BasicCircleLarge;
            spriteBatch.DrawFromCenter(body, center, null, (blue ? new Color(55, 90, 170) : new Color(255, 105, 8)) * opacity * (1f - photosphere),
                scale: radius * 2f / body.Width);
            spriteBatch.DrawFromCenter(bloom, center, null, (blue ? new Color(170, 200, 255, 0) : new Color(255, 235, 120, 0)) * opacity * (1f - photosphere) * 0.7f,
                scale: radius * 2f / bloom.Width);
        }
        else
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            shader.Data.SetCustomParameter("uGlobalTime", (float)Main.timeForVisualEffects / 60f)
                .SetCustomParameter("uNebulaNoiseTexture", AnomalyTextures.Noise.Perlin2)
                .SetCustomParameter("uGoldColor", blue ? CompanionSurfaceColor : GoldColor)
                .SetCustomParameter("uStarlightColor", blue ? CompanionCoreColor : StarlightColor)
                .SetCustomParameter("uPhotosphere", photosphere)
                .SetCustomParameter("uBluePalette", blue ? 1f : 0f)
                .SetCustomParameter("uCoreFlash", blue ? 0f : CoreFlash)
                .Apply();
            Texture2D pixel = TOTextures.FillerTexture;
            spriteBatch.DrawFromCenter(pixel, center, null, Color.White * opacity * (1f - photosphere), scale: radius * 4f / pixel.Width);
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
                Vector2 offset = angle.ToRotationVector2() * (radius * distance * 0.92f);
                float twinkle = 0.6f + 0.4f * MathF.Sin(time * 2f + i * 1.7f);
                spriteBatch.DrawFromCenter(sparkle, center + offset, null,
                    (blue ? new Color(190, 215, 255, 0) : new Color(255, 235, 175, 0)) * opacity * (1f - photosphere) * twinkle, time * 0.15f,
                    (4f + 4f * twinkle) / sparkle.Width);
            }
        }
        if (photosphere > 0f)
            DrawStellarPhotosphere(spriteBatch, center, radius, blue, opacity * photosphere);
    }

    internal void DrawCore(SpriteBatch spriteBatch, Vector2? worldCenter = null, float? drawRadius = null, bool blue = false, float opacity = 1f)
    {
        float radius = drawRadius ?? SphereRadius;
        if (Main.dedServ || radius <= 0f)
            return;
        float photosphere = blue ? 1f : PrimaryPhotosphere;
        Vector2 center = (worldCenter ?? Projectile.Center) - Main.screenPosition;
        if (photosphere >= 1f)
            return;
        float flash = blue ? 0f : CoreFlash;
        AnomalyEffects.MiscShaderContainer shader = AnomalyClientConfig.Instance.EnableShaders
            ? AnomalyEffects.Overlays.LegendStellarSphere : null;
        if (shader is null)
        {
            Texture2D bloom = ParticleHandler.GetTexture<BloomParticle>();
            Texture2D sparkle = ParticleHandler.GetTexture<GenericSparkle>();
            float pulse = 1f + 0.08f * MathF.Sin((float)Main.timeForVisualEffects * 0.15f);
            float coreSize = radius * (1f + flash * 0.65f) * pulse;
            spriteBatch.DrawFromCenter(bloom, center, null, (blue ? new Color(150, 190, 255, 0) : new Color(255, 235, 160, 0)) * opacity * (1f - photosphere) * (0.8f + flash),
                scale: coreSize * 0.8f / bloom.Width);
            spriteBatch.DrawFromCenter(sparkle, center, null, (blue ? new Color(225, 235, 255, 0) : new Color(255, 250, 220, 0)) * opacity * (1f - photosphere) * (1f + flash),
                (float)Main.timeForVisualEffects * 0.005f, coreSize * 0.65f / sparkle.Width);
            return;
        }
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        shader.Data.SetCustomParameter("uGlobalTime", (float)Main.timeForVisualEffects / 60f)
            .SetCustomParameter("uNebulaNoiseTexture", AnomalyTextures.Noise.Perlin2)
            .SetCustomParameter("uGoldColor", blue ? CompanionSurfaceColor : GoldColor)
            .SetCustomParameter("uStarlightColor", blue ? CompanionCoreColor : StarlightColor)
            .SetCustomParameter("uPhotosphere", photosphere)
            .SetCustomParameter("uBluePalette", blue ? 1f : 0f)
            .SetCustomParameter("uCoreFlash", flash)
            .Apply();
        shader.Asset.Value.CurrentTechnique.Passes["Core"].Apply();
        Texture2D pixel = TOTextures.FillerTexture;
        spriteBatch.DrawFromCenter(pixel, center, null, Color.White * opacity, scale: radius * 4f / pixel.Width);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    }
    internal void DrawSystem(SpriteBatch spriteBatch, IReadOnlyList<LegendBlueStar> blueStars, IReadOnlyList<LegendLightOrb> lightOrbs)
    {
        LegendBlueStar blue = null;
        foreach (LegendBlueStar candidate in blueStars)
            if (candidate.PrimaryProjectile == Projectile)
            {
                blue = candidate;
                break;
            }
        blue?.DrawCompanionMatter(spriteBatch, behind: true);
        blue?.DrawBlueCompanion(spriteBatch, behind: true);
        DrawBody(spriteBatch);
        DrawLightOrbTrails(spriteBatch);
        foreach (LegendLightOrb orb in lightOrbs)
            if (orb.SphereProjectile == Projectile)
                orb.Draw(spriteBatch);
        DrawCore(spriteBatch);
        blue?.DrawCompanionMatter(spriteBatch, behind: false);
        blue?.DrawBlueCompanion(spriteBatch, behind: false);
    }
}