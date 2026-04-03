namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(SpriteBatch spriteBatch)
    {
        public void ChangeBlendState(BlendState blendState)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, blendState, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// 绘制纹理，保证传入的坐标为纹理中心点。
        /// </summary>
        public void DrawFromCenter(Texture2D texture, Vector2 center, Rectangle? sourceRectangle, Color color, float rotation = 0f, float scale = 1f, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f) =>
            spriteBatch.Draw(texture, center, sourceRectangle, color, rotation, (sourceRectangle?.Size() ?? texture.Size()) / 2f, scale, effects, layerDepth);

        /// <summary>
        /// 绘制纹理，保证传入的坐标为纹理中心点。
        /// </summary>
        public void DrawFromCenter_VectorScale(Texture2D texture, Vector2 center, Rectangle? sourceRectangle, Color color, float rotation = 0f, Vector2? scale = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f) =>
            spriteBatch.Draw(texture, center, sourceRectangle, color, rotation, (sourceRectangle?.Size() ?? texture.Size()) / 2f, scale ?? new Vector2(1f), effects, layerDepth);
    }
}
