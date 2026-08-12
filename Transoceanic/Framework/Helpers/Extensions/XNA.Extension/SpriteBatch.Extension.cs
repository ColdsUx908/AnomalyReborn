// Developed by ColdsUx

using Transoceanic.DataStructures.Rendering;

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(SpriteBatch spriteBatch)
    {
        /// <summary>
        /// 改变当前 SpriteBatch 的绘制状态，并返回一个包含原始状态的 SpriteBatchSnapshot 结构体。
        /// </summary>
        /// <param name="sortMode">要应用的新排序模式（null 表示不变）。</param>
        /// <param name="blendState">要应用的新混合状态（null 表示不变）。</param>
        /// <param name="samplerState">要应用的新采样器状态（null 表示不变）。</param>
        /// <param name="depthStencilState">要应用的新深度模板状态（null 表示不变）。</param>
        /// <param name="rasterizerState">要应用的新光栅化状态（null 表示不变）。</param>
        /// <param name="customEffect">要应用的新自定义效果（null 表示不变）。</param>
        /// <param name="transformMatrix">要应用的新变换矩阵。</param>
        /// <returns>包含原始状态的 SpriteBatchSnapshot 结构体。</returns>
        public SpriteBatchSnapshot ChangeState(
            SpriteSortMode? sortMode = null,
            BlendState blendState = null,
            SamplerState samplerState = null,
            DepthStencilState depthStencilState = null,
            RasterizerState rasterizerState = null,
            Effect customEffect = null,
            Matrix? transformMatrix = null)
        {
            SpriteBatchSnapshot copy = new(spriteBatch);
            spriteBatch.End();
            spriteBatch.Begin(
                sortMode ?? copy.sortMode,
                blendState ?? copy.blendState,
                samplerState ?? copy.samplerState,
                depthStencilState ?? copy.depthStencilState,
                rasterizerState ?? copy.rasterizerState,
                customEffect ?? copy.customEffect,
                transformMatrix ?? copy.transformMatrix);
            return copy;
        }

        /// <summary>
        /// 根据提供的快照重置 SpriteBatch 的绘制状态。
        /// </summary>
        /// <param name="snapshot">要应用的快照。</param>
        public void ResetState(SpriteBatchSnapshot snapshot)
        {
            spriteBatch.End();
            spriteBatch.Begin(snapshot.sortMode, snapshot.blendState, snapshot.samplerState, snapshot.depthStencilState, snapshot.rasterizerState, snapshot.customEffect, snapshot.transformMatrix);
        }

        /// <summary>
        /// 创建一个新的 SpriteBatchScope 对象，用于在使用完毕后自动恢复 SpriteBatch 的绘制状态。
        /// </summary>
        /// <param name="sortMode">要应用的新排序模式（null 表示不变）。</param>
        /// <param name="blendState">要应用的新混合状态（null 表示不变）。</param>
        /// <param name="samplerState">要应用的新采样器状态（null 表示不变）。</param>
        /// <param name="depthStencilState">要应用的新深度模板状态（null 表示不变）。</param>
        /// <param name="rasterizerState">要应用的新光栅化状态（null 表示不变）。</param>
        /// <param name="customEffect">要应用的新自定义效果（null 表示不变）。</param>
        /// <param name="transformMatrix">要应用的新变换矩阵（null 表示不变）。</param>
        /// <returns>一个新的 SpriteBatchScope 对象。</returns>
        public SpriteBatchScope Scope(
            SpriteSortMode? sortMode = null,
            BlendState blendState = null,
            SamplerState samplerState = null,
            DepthStencilState depthStencilState = null,
            RasterizerState rasterizerState = null,
            Effect customEffect = null,
            Matrix? transformMatrix = null) =>
            new(spriteBatch, sortMode, blendState, samplerState, depthStencilState, rasterizerState, customEffect, transformMatrix);

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