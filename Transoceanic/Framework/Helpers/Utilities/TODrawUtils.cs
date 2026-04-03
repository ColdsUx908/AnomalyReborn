using Transoceanic.DataStructures;

namespace Transoceanic.Framework.Helpers;

public static class TODrawUtils
{
    public static Vector2 ScreenSize => new(Main.screenWidth, Main.screenHeight);

    public static Vector2 ScreenCenter => Main.screenPosition + ScreenSize / 2f;

    public static Vector2 ScreenCenterTile => ScreenCenter / 16f;

    /// <summary>
    /// 在物品栏中绘制特定大小的物品贴图，不受物品栏自动缩放限制。
    /// <br/>在 <see cref="ModItem.PreDrawInInventory(SpriteBatch, Vector2, Rectangle, Color, Color, Vector2, float)"/> 中使用。
    /// </summary>
    public static void DrawInventoryCustomSize(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Vector2 origin,
        Texture2D texture, float wantedScale = 1f, Vector2 drawOffset = default)
        => spriteBatch.Draw(texture, position + drawOffset * wantedScale, frame, drawColor, 0f, origin, wantedScale, SpriteEffects.None, 0);

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

    public static void DrawBorderString(SpriteBatch spriteBatch, DynamicSpriteFont font, StringBuilder textBuilder, Vector2 baseDrawPosition, Color mainColor, Color borderColor, int way = 8, float borderWidth = 1f, float scale = 1f, float rotation = 0f) =>
        DrawBorderString(spriteBatch, font, textBuilder.ToString(), baseDrawPosition, mainColor, borderColor, way, borderWidth, scale, rotation);
}
