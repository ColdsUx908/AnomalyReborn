// Developed by ColdsUx

namespace Transoceanic.Framework.Helpers;

/// <summary>
/// 提供与绘图相关的工具方法，例如屏幕坐标计算、自定义尺寸物品绘制以及描边效果。
/// </summary>
public static class TODrawUtils
{
    /// <summary>
    /// 获取当前屏幕的尺寸（像素）。
    /// </summary>
    public static Vector2 ScreenSize => new(Main.screenWidth, Main.screenHeight);

    /// <summary>
    /// 获取当前屏幕的中心点坐标（世界坐标系）。
    /// </summary>
    public static Vector2 ScreenCenter => Main.screenPosition + ScreenSize / 2f;

    /// <summary>
    /// 获取当前屏幕中心点所对应的物块坐标（每格 16 像素）。
    /// </summary>
    public static Vector2 ScreenCenterTile => ScreenCenter / 16f;

    /// <summary>
    /// 在物品栏中绘制特定大小的物品贴图，不受物品栏自动缩放限制。
    /// <br/>通常在 <see cref="ModItem.PreDrawInInventory(SpriteBatch, Vector2, Rectangle, Color, Color, Vector2, float)"/> 中使用。
    /// </summary>
    /// <param name="spriteBatch">用于绘制贴图的 SpriteBatch。</param>
    /// <param name="position">绘制基准位置（物品栏槽位坐标）。</param>
    /// <param name="frame">贴图源矩形区域。</param>
    /// <param name="drawColor">绘制颜色。</param>
    /// <param name="origin">贴图旋转/缩放原点。</param>
    /// <param name="texture">要绘制的贴图。</param>
    /// <param name="wantedScale">期望的缩放比例，默认为 1f。</param>
    /// <param name="drawOffset">相对于基准位置的绘制偏移量（缩放前）。</param>
    public static void DrawInInventoryWithCustomSize(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Vector2 origin,
        Texture2D texture, float wantedScale = 1f, Vector2 drawOffset = default)
        => spriteBatch.Draw(texture, position + drawOffset * wantedScale, frame, drawColor, 0f, origin, wantedScale, SpriteEffects.None, 0);

    /// <summary>
    /// 以指定位置为绘制原点，绘制带有描边效果的贴图（描边通过在周围多方向重复绘制实现）。
    /// </summary>
    /// <param name="spriteBatch">用于绘制贴图的 SpriteBatch。</param>
    /// <param name="texture">要绘制的贴图。</param>
    /// <param name="position">贴图绘制原点（左上角或自定义原点对应的位置）。</param>
    /// <param name="sourceRectangle">贴图源矩形区域，可为 <see langword="null"/> 表示整个纹理。</param>
    /// <param name="color">绘制颜色（描边时 Alpha 会被强制置零以达到纯色描边效果）。</param>
    /// <param name="rotation">旋转角度（弧度）。</param>
    /// <param name="origin">旋转/缩放原点。</param>
    /// <param name="scale">缩放比例。</param>
    /// <param name="spriteEffects">翻转效果。</param>
    /// <param name="layerDepth">图层深度。</param>
    /// <param name="way">描边采样方向数量，数值越大描边越平滑，默认为 8。</param>
    /// <param name="borderWidth">描边宽度（像素）。若小于等于 0，则只绘制原始贴图。</param>
    public static void DrawBorderTexture(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation = 0f, Vector2 origin = default, float scale = 1f, SpriteEffects spriteEffects = SpriteEffects.None, float layerDepth = 0f,
        int way = 8, float borderWidth = 1f)
    {
        if (borderWidth > 0f)
        {
            color.A = 0;
            float singleRadian = MathHelper.TwoPi / way;
            for (int i = 0; i < way; i++)
            {
                float rotationOffset = singleRadian * i;
                PolarVector2 offset = new(borderWidth * TOMathUtils.PolarEquation.LameCurve(rotationOffset, 3f), rotation + rotationOffset);
                spriteBatch.Draw(texture, position + offset, sourceRectangle, color, rotation, origin, scale, spriteEffects, layerDepth);
            }
        }
    }

    /// <summary>
    /// 以中心点为绘制原点，绘制带有描边效果的贴图（描边通过在周围多方向重复绘制实现）。
    /// </summary>
    /// <param name="spriteBatch">用于绘制贴图的 SpriteBatch。</param>
    /// <param name="texture">要绘制的贴图。</param>
    /// <param name="center">贴图中心点坐标。</param>
    /// <param name="sourceRectangle">贴图源矩形区域，可为 <see langword="null"/> 表示整个纹理。</param>
    /// <param name="color">绘制颜色（描边时 Alpha 会被强制置零以达到纯色描边效果）。</param>
    /// <param name="rotation">旋转角度（弧度）。</param>
    /// <param name="scale">缩放比例。</param>
    /// <param name="spriteEffects">翻转效果。</param>
    /// <param name="layerDepth">图层深度。</param>
    /// <param name="way">描边采样方向数量，数值越大描边越平滑，默认为 8。</param>
    /// <param name="borderWidth">描边宽度（像素）。若小于等于 0，则只绘制原始贴图。</param>
    public static void DrawBorderTextureFromCenter(SpriteBatch spriteBatch, Texture2D texture, Vector2 center, Rectangle? sourceRectangle, Color color, float rotation = 0f, float scale = 1f, SpriteEffects spriteEffects = SpriteEffects.None, float layerDepth = 0f,
        int way = 8, float borderWidth = 1f)
    {
        if (borderWidth > 0f)
        {
            color.A = 0;
            float singleRadian = MathHelper.TwoPi / way;
            for (int i = 0; i < way; i++)
            {
                float rotationOffset = singleRadian * i;
                PolarVector2 offset = new(borderWidth * TOMathUtils.PolarEquation.LameCurve(rotationOffset, 3f), rotation + rotationOffset);
                spriteBatch.DrawFromCenter(texture, center + offset, sourceRectangle, color, rotation, scale, spriteEffects, layerDepth);
            }
        }
    }

    /// <summary>
    /// 绘制带有描边效果的字符串。
    /// </summary>
    /// <param name="spriteBatch">用于绘制文字的 SpriteBatch。</param>
    /// <param name="font">要使用的动态字体。</param>
    /// <param name="text">要绘制的文本内容。</param>
    /// <param name="baseDrawPosition">文字绘制基准位置（通常为左上角）。</param>
    /// <param name="mainColor">文字主体颜色。</param>
    /// <param name="borderColor">文字描边颜色。</param>
    /// <param name="way">描边采样方向数量，数值越大描边越平滑，默认为 8。</param>
    /// <param name="borderWidth">描边宽度（像素）。若小于等于 0，则只绘制主体文字。</param>
    /// <param name="scale">文字缩放比例。</param>
    /// <param name="rotation">旋转角度（弧度）。</param>
    public static void DrawBorderString(SpriteBatch spriteBatch, DynamicSpriteFont font, string text, Vector2 baseDrawPosition, Color mainColor, Color borderColor, int way = 8, float borderWidth = 1f, float scale = 1f, float rotation = 0f)
    {
        if (borderWidth > 0f)
        {
            float singleRadian = MathHelper.TwoPi / way;
            for (int i = 0; i < way; i++)
            {
                float rotationOffset = singleRadian * i;
                PolarVector2 offset = new(borderWidth * TOMathUtils.PolarEquation.LameCurve(rotationOffset, 3f), rotation + rotationOffset);
                spriteBatch.DrawString(font, text, baseDrawPosition + offset, borderColor, rotation, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
        }
        spriteBatch.DrawString(font, text, baseDrawPosition, mainColor, rotation, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    /// <summary>
    /// 绘制带有描边效果的字符串（使用 StringBuilder 以提高性能）。
    /// </summary>
    /// <param name="spriteBatch">用于绘制文字的 SpriteBatch。</param>
    /// <param name="font">要使用的动态字体。</param>
    /// <param name="textBuilder">要绘制的文本内容构建器。</param>
    /// <param name="baseDrawPosition">文字绘制基准位置（通常为左上角）。</param>
    /// <param name="mainColor">文字主体颜色。</param>
    /// <param name="borderColor">文字描边颜色。</param>
    /// <param name="way">描边采样方向数量，数值越大描边越平滑，默认为 8。</param>
    /// <param name="borderWidth">描边宽度（像素）。若小于等于 0，则只绘制主体文字。</param>
    /// <param name="scale">文字缩放比例。</param>
    /// <param name="rotation">旋转角度（弧度）。</param>
    public static void DrawBorderString(SpriteBatch spriteBatch, DynamicSpriteFont font, StringBuilder textBuilder, Vector2 baseDrawPosition, Color mainColor, Color borderColor, int way = 8, float borderWidth = 1f, float scale = 1f, float rotation = 0f) =>
        DrawBorderString(spriteBatch, font, textBuilder.ToString(), baseDrawPosition, mainColor, borderColor, way, borderWidth, scale, rotation);
}