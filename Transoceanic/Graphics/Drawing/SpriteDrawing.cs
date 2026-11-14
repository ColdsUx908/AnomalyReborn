namespace Transoceanic.Graphics.Drawing;

/// <summary>按像素尺寸提交中心对齐的精灵；调用方持有纹理并负责 SpriteBatch 的 Begin/End 和混合状态。</summary>
public static class SpriteDrawing
{
    /// <summary>
    /// 按指定尺寸绘制完整纹理。先将尺寸换算为纹理缩放，再乘整体 scale；位置不受此 scale 影响。
    /// 非正尺寸或不超过 0.002 的不透明度不提交，透明度上限为 1。颜色必须符合调用方的混合约定。
    /// </summary>
    public static void DrawSized(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Vector2 size,
        Color color, float rotation = 0f, float opacity = 1f, float scale = 1f)
    {
        if (opacity <= 0.002f || size.X <= 0f || size.Y <= 0f)
            return;
        spriteBatch.Draw(texture, position, null, color * Math.Min(opacity, 1f), rotation,
            new Vector2(texture.Width, texture.Height) * 0.5f,
            new Vector2(size.X / texture.Width, size.Y / texture.Height) * scale, SpriteEffects.None, 0f);
    }

    /// <summary>
    /// 将强度拆为若干不超过 1 的精灵提交；适合加色混合，保留原有的绘制次数和顺序。
    /// intensity 必须为有限值且不大于 64；每次递减 1，不隐式修改纹理、颜色或混合状态。
    /// </summary>
    public static void DrawGlow(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Vector2 size,
        Color color, float rotation, float intensity, float scale = 1f)
    {
        const float MaximumIntensity = 64f;
        if (!float.IsFinite(intensity) || intensity > MaximumIntensity)
            throw new ArgumentOutOfRangeException(nameof(intensity));
        for (float remaining = intensity; remaining > 0.002f; remaining -= 1f)
            DrawSized(spriteBatch, texture, position, size, color, rotation, remaining, scale);
    }
}
