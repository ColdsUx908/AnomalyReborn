using Anomalies.Assets.Effects;

namespace Anomalies.SpecialContents.Contributor.MeguminExplosion;

internal static class ExplosionDrawing
{
    internal const int CircleBrightness = 5;

    internal static void Circle(Vector2 center, Vector2 size, float time, float opacity, float rotation, Color color)
    {
        if (opacity <= 0f) return;
        Texture2D noise = AnomalyTextures.Noise.Turbulence;
        if (AnomalyClientConfig.Instance.EnableShaders && AnomalyEffects.Primitives.MeguminCircle is not null)
        {
            // The shader emits premultiplied light with zero alpha, so AlphaBlend adds it without dimming the background.
            using (Main.spriteBatch.Scope(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap,
                DepthStencilState.None, RasterizerState.CullNone, transformMatrix: Main.GameViewMatrix.TransformationMatrix, effectIsNull: true))
            {
                AnomalyEffects.Primitives.MeguminCircle.Data.SetCustomParameter("uFlowTime", time)
                    .SetCustomParameter("uBrightness", (float)CircleBrightness).Apply();
                Main.spriteBatch.Draw(noise, center - Main.screenPosition, null, color * opacity, rotation,
                    noise.Size() * 0.5f, size / noise.Size(), SpriteEffects.None, 0f);
            }
        }
        else
        {
            using (Main.spriteBatch.Scope(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, transformMatrix: Main.GameViewMatrix.TransformationMatrix, effectIsNull: true))
            {
                // Repeat the additive contribution instead of clamping an 8-bit Color multiplied by five.
                for (int pass = 0; pass < CircleBrightness; pass++)
                {
                    Ring(center, size * 0.4f, color * opacity, 2f, rotation);
                    Ring(center, size * 0.32f, color * (opacity * 0.6f), 1.5f, rotation);
                    for (int i = 0; i < 6; i++)
                    {
                        float a = i * MathHelper.TwoPi / 6f + time;
                        float b = a + MathHelper.TwoPi / 3f;
                        Line(center + a.ToRotationVector2() * size * 0.3f,
                            center + b.ToRotationVector2() * size * 0.3f, 1.5f, color * opacity);
                    }
                }
            }
        }
    }

    internal static void Laser(Vector2 focus, Vector2 top, Vector2 bottom, float age, float power, float opacity)
    {
        float time = age / 60f;
        if (opacity <= 0f) return;
        float width = 540f * (0.16f + power * 0.84f);
        float length = bottom.Y - top.Y;
        Texture2D noise = AnomalyTextures.Noise.Turbulence;
        bool useShader = AnomalyClientConfig.Instance.EnableShaders && AnomalyEffects.Primitives.MeguminLaser is not null;
        if (useShader)
        {
            using (Main.spriteBatch.Scope(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap,
                DepthStencilState.None, RasterizerState.CullNone, transformMatrix: Main.GameViewMatrix.TransformationMatrix, effectIsNull: true))
            {
                AnomalyEffects.Primitives.MeguminLaser.Data.UseImage1(AnomalyTextures.Noise._Perlin)
                    .SetCustomParameter("uFlowTime", time)
                    .SetCustomParameter("uBeamTop", top.Y / 2400f)
                    .SetCustomParameter("uBeamSpan", length / 2400f).Apply();
                Main.spriteBatch.Draw(noise, (top + bottom) * 0.5f - Main.screenPosition,
                    null, new Color(1f, 1f, 1f, opacity), 0f, noise.Size() * 0.5f,
                    new Vector2(width, length) / noise.Size(), SpriteEffects.None, 0f);
            }
        }

        // The same straight layered beam remains readable when shaders are disabled.
        using (Main.spriteBatch.Scope(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
            DepthStencilState.None, RasterizerState.CullNone, transformMatrix: Main.GameViewMatrix.TransformationMatrix, effectIsNull: true))
        {
            if (!useShader)
            {
                Texture2D bloom = ModContent.Request<Texture2D>("Anomalies/SpecialContents/Contributor/MeguminExplosion/MeguminBloom").Value;
                Main.spriteBatch.Draw(bloom, (top + bottom) * 0.5f - Main.screenPosition, null,
                    Color.OrangeRed * (opacity * 0.7f), 0f, bloom.Size() * 0.5f,
                    new Vector2(width, length * 1.15f) / bloom.Size(), SpriteEffects.None, 0f);
                Line(top, bottom, width * 0.38f, Color.OrangeRed * (opacity * 0.32f));
                Line(top, bottom, width * 0.21f, Color.Orange * (opacity * 0.55f));
                Line(top, bottom, width * 0.065f, Color.LightGoldenrodYellow * opacity);
                for (int side = -1; side <= 1; side += 2)
                    Line(top + Vector2.UnitX * (side * width * 0.17f), bottom + Vector2.UnitX * (side * width * 0.17f),
                        4f + 5f * power, Color.LightGoldenrodYellow * opacity);
            }

            // Braided golden filaments, sparks and long broken rays surround the hot core.
            for (int strand = 0; strand < 3; strand++)
            {
                Vector2 previous = top;
                for (int segment = 0; segment <= 120; segment++)
                {
                    float progress = segment / 120f;
                    float worldY = MathHelper.Lerp(top.Y, bottom.Y, progress);
                    float angle = worldY / 85f - time * 9f + strand * MathHelper.TwoPi / 3f;
                    float offset = MathF.Sin(angle) * width * (0.2f + strand * 0.035f);
                    Vector2 point = Vector2.Lerp(top, bottom, progress) + Vector2.UnitX * offset;
                    float strength = opacity * Math.Max(0.08f, 0.35f + 0.4f * MathF.Cos(angle));
                    if (segment > 0)
                        Line(previous, point, 1.2f + power * 1.5f, Color.Gold * strength);
                    if (segment % 3 == 0)
                        Line(point - Vector2.UnitY * 2f, point + Vector2.UnitY * 2f, 3f, Color.LightGoldenrodYellow * opacity);
                    previous = point;
                }
            }
            for (int i = 0; i < 48; i++)
            {
                // Fixed seeds avoid frame-to-frame jitter; every streak streams downward.
                float progress = (i * 0.618034f + time * (0.5f + i % 5 * 0.08f)) % 1f;
                float side = i % 2 == 0 ? -1f : 1f;
                float spread = width * (0.26f + (MathF.Sin(i * 12.37f) * 0.5f + 0.5f) * 0.28f);
                Vector2 point = Vector2.Lerp(top, bottom, progress) + Vector2.UnitX * (side * spread);
                float streakLength = 25f + i % 6 * 14f;
                float strength = opacity * MathF.Sin(progress * MathHelper.Pi);
                Color color = i % 3 == 0 ? Color.LightGoldenrodYellow : Color.Orange;
                Line(point - new Vector2(side * 5f, streakLength), point, 1.5f + i % 3, color * (strength * 0.8f));
                if (i % 6 == 0)
                {
                    Line(point - Vector2.UnitX * 9f, point + Vector2.UnitX * 9f, 1.2f, Color.Gold * strength);
                    Line(point - Vector2.UnitY * 15f, point + Vector2.UnitY * 15f, 1.5f, Color.LightGoldenrodYellow * strength);
                }
            }
            for (int i = 0; i < 4; i++)
            {
                float progress = (time * 1.7f + i * 0.25f) % 1f;
                Vector2 center = Vector2.Lerp(top, bottom, progress);
                float radius = width * (0.36f + 0.1f * MathF.Sin(progress * MathHelper.Pi));
                Ring(center, new Vector2(radius, radius * 0.16f), Color.Orange * (opacity * 0.45f), 2f + power);
            }
        }

        // Ritual seals travel with the mouse focus; they never cap or cover the continuous column.
        for (int i = 0; i < 3; i++)
        {
            float diameter = 460f + i * 180f;
            Circle(focus + Vector2.UnitY * ((i - 1) * 260f), new Vector2(diameter, diameter * 0.19f),
                time * (i % 2 == 0 ? 1f : -1f), opacity * (0.13f + 0.12f * power), 0f,
                i == 1 ? Color.OrangeRed : Color.Gold);
        }
        Glow(focus, new Vector2(300f, 140f) * (0.5f + power), Color.Gold * (opacity * 0.45f));
    }

    internal static void Inferno(Vector2 center, Vector2 size, int mode, float time, float opacity, float rotation = 0f)
    {
        if (opacity <= 0f) return;
        Texture2D noise = AnomalyTextures.Noise.Turbulence;
        if (AnomalyClientConfig.Instance.EnableShaders && AnomalyEffects.Primitives.MeguminInferno is not null)
        {
            using (Main.spriteBatch.Scope(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap,
                DepthStencilState.None, RasterizerState.CullNone, transformMatrix: Main.GameViewMatrix.TransformationMatrix, effectIsNull: true))
            {
                AnomalyEffects.Primitives.MeguminInferno.Data.UseImage1(AnomalyTextures.Noise._Perlin)
                    .SetCustomParameter("uFlowTime", time).SetCustomParameter("uMode", (float)mode).Apply();
                Main.spriteBatch.Draw(noise, center - Main.screenPosition, null, Color.White * opacity, rotation,
                    noise.Size() * 0.5f, size / noise.Size(), SpriteEffects.None, 0f);
            }
        }
        else
        {
            Texture2D bloom = ModContent.Request<Texture2D>("Anomalies/SpecialContents/Contributor/MeguminExplosion/MeguminBloom").Value;
            using (Main.spriteBatch.Scope(SpriteSortMode.Deferred, mode == 2 ? BlendState.AlphaBlend : BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                transformMatrix: Main.GameViewMatrix.TransformationMatrix, effectIsNull: true))
            {
                if (mode == 2)
                    Main.spriteBatch.Draw(bloom, center - Main.screenPosition, null, new Color(45, 30, 38) * opacity,
                        rotation, bloom.Size() * 0.5f, size / bloom.Size(), SpriteEffects.None, 0f);
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Color color = i switch { 0 => Color.OrangeRed, 1 => Color.Orange, _ => Color.LightGoldenrodYellow };
                        Vector2 layerSize = size * (1f - i * 0.22f);
                        Main.spriteBatch.Draw(bloom, center - Main.screenPosition, null, color * (opacity * 0.85f),
                            rotation, bloom.Size() * 0.5f, layerSize / bloom.Size(), SpriteEffects.None, 0f);
                    }
                }
            }
        }
    }

    internal static void Glow(Vector2 center, Vector2 size, Color color)
    {
        Texture2D circle = AnomalyTextures.Extra.BasicCircle;
        using (Main.spriteBatch.Scope(SpriteSortMode.Deferred, BlendState.Additive))
        {
            for (int i = 3; i > 0; i--)
                Main.spriteBatch.Draw(circle, center - Main.screenPosition, null, color * (0.13f / i), 0f,
                    circle.Size() * 0.5f, size * (i / 2f) / circle.Size(), SpriteEffects.None, 0f);
        }
    }

    // All pixel lines use one source texel; MagicPixel is a 1 x 1000 texture in Terraria.
    internal static void Line(Vector2 start, Vector2 end, float width, Color color)
    {
        Vector2 difference = end - start;
        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, start - Main.screenPosition, new Rectangle(0, 0, 1, 1),
            color, difference.ToRotation(), new Vector2(0f, 0.5f), new Vector2(difference.Length(), width), SpriteEffects.None, 0f);
    }

    internal static void Ring(Vector2 center, Vector2 radius, Color color, float width, float rotation = 0f)
    {
        Vector2 previous = center + new Vector2(radius.X, 0f).RotatedBy(rotation);
        const int segments = 96;
        for (int i = 1; i <= segments; i++)
        {
            Vector2 point = center + (MathHelper.TwoPi * i / segments).ToRotationVector2() * radius;
            point = center + (point - center).RotatedBy(rotation);
            Line(previous, point, width, color);
            previous = point;
        }
    }
}
