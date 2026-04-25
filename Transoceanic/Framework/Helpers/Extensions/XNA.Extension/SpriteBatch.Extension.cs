// Designed by ColdsUx

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(SpriteBatch spriteBatch)
    {
        /// <summary>
        /// 结束当前 SpriteBatch 的绘制批次，并使用指定的混合状态重新开始绘制。
        /// </summary>
        /// <param name="blendState">要应用的新混合状态。</param>
        public void ChangeBlendState(BlendState blendState)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, blendState, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// 以纹理中心为绘制原点绘制纹理。
        /// </summary>
        /// <param name="texture">要绘制的纹理。</param>
        /// <param name="center">纹理中心点在世界空间中的坐标。</param>
        /// <param name="sourceRectangle">要绘制的纹理源矩形区域，为 <see langword="null"/> 时绘制整个纹理。</param>
        /// <param name="color">绘制时的颜色调制。</param>
        /// <param name="rotation">纹理的旋转角度（弧度）。</param>
        /// <param name="scale">统一的缩放比例。</param>
        /// <param name="effects">应用的精灵翻转效果。</param>
        /// <param name="layerDepth">绘制的图层深度。</param>
        public void DrawFromCenter(Texture2D texture, Vector2 center, Rectangle? sourceRectangle, Color color, float rotation = 0f, float scale = 1f, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f) =>
            spriteBatch.Draw(texture, center, sourceRectangle, color, rotation, (sourceRectangle?.Size() ?? texture.Size()) / 2f, scale, effects, layerDepth);

        /// <summary>
        /// 以纹理中心为绘制原点绘制纹理，支持非均匀缩放。
        /// </summary>
        /// <param name="texture">要绘制的纹理。</param>
        /// <param name="center">纹理中心点在世界空间中的坐标。</param>
        /// <param name="sourceRectangle">要绘制的纹理源矩形区域，为 <see langword="null"/> 时绘制整个纹理。</param>
        /// <param name="color">绘制时的颜色调制。</param>
        /// <param name="rotation">纹理的旋转角度（弧度）。</param>
        /// <param name="scale">二维缩放向量，为 <see langword="null"/> 时使用 (1, 1)。</param>
        /// <param name="effects">应用的精灵翻转效果。</param>
        /// <param name="layerDepth">绘制的图层深度。</param>
        public void DrawFromCenter_VectorScale(Texture2D texture, Vector2 center, Rectangle? sourceRectangle, Color color, float rotation = 0f, Vector2? scale = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f) =>
            spriteBatch.Draw(texture, center, sourceRectangle, color, rotation, (sourceRectangle?.Size() ?? texture.Size()) / 2f, scale ?? new Vector2(1f), effects, layerDepth);
    }
}