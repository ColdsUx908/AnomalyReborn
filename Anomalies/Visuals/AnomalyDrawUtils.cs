namespace Anomalies.Visuals;

public static class AnomalyDrawUtils
{
    /// <summary>
    /// 从世界坐标起点沿 rotation 方向叠层绘制预警线，宽度由外缘向 1 像素亮芯收窄。
    /// </summary>
    public static void DrawLayeredWarning(SpriteBatch spriteBatch, Vector2 position, float rotation,
        float length, float width, Color outerColor, Color innerColor, float opacity = 1f)
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
            float layerWidth = MathHelper.Lerp(width, Math.Min(1f, width), strength);
            Color color = Color.Lerp(outerColor, innerColor, strength * strength)
                * (MathHelper.Lerp(0.02f, 0.09f, strength) * opacity);
            Vector2 scale = new(length / texture.Width, layerWidth / texture.Height);
            spriteBatch.Draw(texture, screenPosition, null, color, rotation, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
