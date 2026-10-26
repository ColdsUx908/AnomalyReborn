using Anomalies.Assets.Effects;

namespace Anomalies.Visuals;

public static class AnomalyDrawUtils
{
    /// <summary>逆变换屏幕四角，取得当前可见世界范围；padding 为世界坐标下的扩展距离。</summary>
    public static FloatRectangle GetVisibleWorldBounds(float padding = 0f)
    {
        Matrix inverse = Matrix.Invert(Main.GameViewMatrix.TransformationMatrix);
        Vector2 a = Vector2.Transform(Vector2.Zero, inverse);
        Vector2 b = Vector2.Transform(new Vector2(Main.screenWidth, 0f), inverse);
        Vector2 c = Vector2.Transform(new Vector2(0f, Main.screenHeight), inverse);
        Vector2 d = Vector2.Transform(Main.ScreenSize.ToVector2(), inverse);
        Vector2 min = Vector2.Min(Vector2.Min(a, b), Vector2.Min(c, d)) + Main.screenPosition - new Vector2(padding);
        Vector2 max = Vector2.Max(Vector2.Max(a, b), Vector2.Max(c, d)) + Main.screenPosition + new Vector2(padding);
        return new FloatRectangle(min, max - min);
    }

    /// <summary>
    /// 绘制世界坐标下的发光圆环。内部管理 Begin/End，调用时 SpriteBatch 必须处于未开始状态。
    /// color 决定光晕颜色，亮芯向白色提亮；透明度由 opacity 控制。
    /// </summary>
    public static void DrawGlowingCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color,
        float opacity = 1f, float lineWidth = 3f, float glowWidth = 14f)
    {
        if (radius <= 0f || opacity <= 0f || lineWidth <= 0f || glowWidth <= 0f)
            return;
        if (AnomalyClientConfig.Instance.EnableShaders && AnomalyEffects.Primitives.GlowCircle is { } shader)
        {
            Matrix view = Main.GameViewMatrix.TransformationMatrix;
            Vector2 screenCenter = Vector2.Transform(center - Main.screenPosition, view);
            float extent = radius + glowWidth * 6f;
            Vector2 screenExtent = new(extent * (Math.Abs(view.M11) + Math.Abs(view.M21)),
                extent * (Math.Abs(view.M12) + Math.Abs(view.M22)));
            int left = Math.Max(0, (int)MathF.Floor(screenCenter.X - screenExtent.X));
            int top = Math.Max(0, (int)MathF.Floor(screenCenter.Y - screenExtent.Y));
            int right = Math.Min(Main.screenWidth, (int)MathF.Ceiling(screenCenter.X + screenExtent.X));
            int bottom = Math.Min(Main.screenHeight, (int)MathF.Ceiling(screenCenter.Y + screenExtent.Y));
            if (right <= left || bottom <= top)
                return;

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
            shader.Data
                .SetCustomParameter("uScreenPosition", Main.screenPosition)
                .SetCustomParameter("uInverseViewMatrix", Matrix.Invert(view))
                .SetCustomParameter("uCenter", center)
                .SetCustomParameter("uRadius", radius)
                .SetCustomParameter("uLineWidth", lineWidth)
                .SetCustomParameter("uGlowWidth", glowWidth)
                .UseColor(color)
                .UseSecondaryColor(Color.Lerp(color, Color.White, 0.75f))
                .UseOpacity(opacity)
                .Apply();
            spriteBatch.Draw(TOTextures.FillerTexture, new Rectangle(left, top, right - left, bottom - top), Color.White);
            spriteBatch.End();
            return;
        }

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        const int Segments = 360;
        for (int i = 0; i < Segments; i++)
            DrawFallbackGlowLine(spriteBatch, center + (MathHelper.TwoPi * i / Segments).ToRotationVector2() * radius,
                center + (MathHelper.TwoPi * (i + 1) / Segments).ToRotationVector2() * radius, color, opacity, lineWidth, glowWidth);
        spriteBatch.End();
    }

    /// <summary>绘制世界坐标下的一条发光线段，调用要求与 DrawGlowingCircle 相同。</summary>
    public static void DrawGlowingLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color,
        float opacity = 1f, float lineWidth = 3f, float glowWidth = 14f)
    {
        Span<(Vector2 Start, Vector2 End)> lines = stackalloc (Vector2, Vector2)[1];
        lines[0] = (start, end);
        DrawGlowingLines(spriteBatch, lines, color, opacity, lineWidth, glowWidth);
    }

    /// <summary>一次 Begin/End 批量绘制同色线段，所有端点均为世界坐标。</summary>
    public static void DrawGlowingLines(SpriteBatch spriteBatch, ReadOnlySpan<(Vector2 Start, Vector2 End)> lines, Color color,
        float opacity = 1f, float lineWidth = 3f, float glowWidth = 14f)
    {
        if (lines.IsEmpty || opacity <= 0f || lineWidth <= 0f || glowWidth <= 0f)
            return;
        AnomalyEffects.MiscShaderContainer shader = AnomalyClientConfig.Instance.EnableShaders ? AnomalyEffects.Primitives.GlowLine : null;
        spriteBatch.Begin(shader is null ? SpriteSortMode.Deferred : SpriteSortMode.Immediate,
            BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        foreach ((Vector2 start, Vector2 end) in lines)
        {
            Vector2 edge = end - start;
            float length = edge.Length();
            if (length <= 0f)
                continue;
            if (shader is null)
            {
                DrawFallbackGlowLine(spriteBatch, start, end, color, opacity, lineWidth, glowWidth);
                continue;
            }
            float padding = glowWidth * 6f;
            shader.Data
                .SetCustomParameter("uLength", length)
                .SetCustomParameter("uPadding", padding)
                .SetCustomParameter("uLineWidth", lineWidth)
                .SetCustomParameter("uGlowWidth", glowWidth)
                .UseColor(color)
                .UseSecondaryColor(Color.Lerp(color, Color.White, 0.75f))
                .UseOpacity(opacity)
                .Apply();
            Texture2D texture = TOTextures.FillerTexture;
            spriteBatch.DrawFromCenter_VectorScale(texture, (start + end) * 0.5f - Main.screenPosition, null, Color.White,
                edge.ToRotation(), new Vector2((length + padding * 2f) / texture.Width, padding * 2f / texture.Height));
        }
        spriteBatch.End();
    }

    private static void DrawFallbackGlowLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end,
        Color color, float opacity, float lineWidth, float glowWidth)
    {
        Vector2 edge = end - start;
        Texture2D texture = TextureAssets.MagicPixel.Value;
        Color coreColor = Color.Lerp(color, Color.White, 0.75f);
        for (int layer = 0; layer < 3; layer++)
        {
            float width = layer switch { 0 => glowWidth * 2f, 1 => glowWidth, _ => lineWidth };
            float strength = layer switch { 0 => 0.08f, 1 => 0.18f, _ => 0.8f };
            Color layerColor = (layer == 2 ? coreColor : color) with { A = 0 };
            spriteBatch.DrawFromCenter_VectorScale(texture, (start + end) * 0.5f - Main.screenPosition,
                null, layerColor * (opacity * strength), edge.ToRotation(), new Vector2(edge.Length() / texture.Width, width / texture.Height));
        }
    }

    /// <summary>
    /// 从世界坐标起点沿 rotation 方向叠层绘制预警线，宽度由外缘向 1 像素亮芯收窄。
    /// </summary>
    public static void DrawLayeredWarning(SpriteBatch spriteBatch, Vector2 position, float rotation,
        float length, float width, Color outerColor, Color innerColor, float opacity = 1f,
        float startFadeLength = 0f, float coreWidth = 1f)
    {
        if (length <= 0f || width <= 0f || opacity <= 0f)
            return;

        Texture2D texture = AnomalyTextures.Extra.LaserTelegraphBeam;
        Vector2 origin = new(0f, texture.Height * 0.5f);
        Vector2 screenPosition = position - Main.screenPosition;

        const int Layers = 30;
        for (int i = 0; i < Layers; i++)
        {
            float strength = (float)i / (Layers - 1);
            float layerWidth = MathHelper.Lerp(width, Math.Min(coreWidth, width), strength);
            Color color = Color.Lerp(outerColor, innerColor, strength * strength)
                * (MathHelper.Lerp(0.02f, 0.09f, strength) * opacity);
            Vector2 scale = new(length / texture.Width, layerWidth / texture.Height);
            if (startFadeLength <= 0f)
            {
                spriteBatch.Draw(texture, screenPosition, null, color, rotation, origin, scale, SpriteEffects.None, 0f);
                continue;
            }

            // Keep the original beam UVs, fading only its first section into the emitter.
            int fadePixels = Math.Clamp((int)(Math.Min(startFadeLength, length * 0.5f) / length * texture.Width), 1, texture.Width);
            Vector2 direction = rotation.ToRotationVector2();
            const int FadeSteps = 16;
            for (int step = 0; step <= FadeSteps; step++)
            {
                int left = step == FadeSteps ? fadePixels : fadePixels * step / FadeSteps;
                int right = step == FadeSteps ? texture.Width : fadePixels * (step + 1) / FadeSteps;
                if (right <= left)
                    continue;
                float fade = step == FadeSteps ? 1f
                    : MathHelper.SmoothStep(0f, 1f, (left + right) * 0.5f / fadePixels);
                Rectangle source = new(left, 0, right - left, texture.Height);
                Vector2 start = screenPosition + direction * (left * scale.X);
                spriteBatch.Draw(texture, start, source, color * fade, rotation, origin, scale, SpriteEffects.None, 0f);
            }
        }
    }
}
