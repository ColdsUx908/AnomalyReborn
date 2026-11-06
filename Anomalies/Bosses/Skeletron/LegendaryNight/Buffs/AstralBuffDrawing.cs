namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>星神祝福和压制图标的共享绘制入口。</summary>
internal static class AstralBuffDrawing
{
    public static void DrawGlow(SpriteBatch spriteBatch, ref BuffDrawParams drawParams, Color color)
    {
        if (Main.dedServ)
            return;

        Texture2D bloom = ParticleHandler.GetTexture<BloomParticle>();
        if (bloom is null || bloom.IsDisposed)
            return;

        Vector2 iconSize = drawParams.SourceRectangle.Size();
        Vector2 center = drawParams.Position + iconSize * 0.5f;
        float pulse = 0.9f + 0.1f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2f);
        Vector2 glowSize = iconSize + new Vector2(70f);
        Vector2 glowScale = glowSize / bloom.Size();
        Color glow = (color with { A = 0 }) * pulse;

        // 零 Alpha 的预乘颜色提供叠加光晕，无需切换 UI 的 SpriteBatch 或着色器。
        spriteBatch.DrawFromCenter_VectorScale(bloom, center, null, glow * 0.8f, 0f,
            glowScale, SpriteEffects.None, 0f);
        spriteBatch.DrawFromCenter_VectorScale(bloom, center, null, glow * 0.45f, 0f,
            glowScale * 0.65f, SpriteEffects.None, 0f);

        // 与 Radiance 的发光图标一样，避免非悬停状态将图标本身压暗。
        drawParams.DrawColor = Color.White;
    }
}