using Transoceanic.Runtime.Publicizing.Wrappers;
namespace Transoceanic.Graphics.Rendering;

/// <summary>
/// 一个结构体，用于保存 <see cref="SpriteBatch"/> 的当前绘制状态，以便在更改状态后能够恢复原始状态。
/// </summary>
public struct SpriteBatchSnapshot
{
    /// <summary>
    /// SpriteBatch.Begin 使用的精灵排序模式。
    /// </summary>
    public SpriteSortMode SortMode;
    /// <summary>
    /// SpriteBatch.Begin 使用的混合状态引用。
    /// </summary>
    public BlendState BlendState;
    /// <summary>
    /// SpriteBatch.Begin 使用的采样器状态引用。
    /// </summary>
    public SamplerState SamplerState;
    /// <summary>
    /// SpriteBatch.Begin 使用的深度与模板测试状态引用。
    /// </summary>
    public DepthStencilState DepthStencilState;
    /// <summary>
    /// SpriteBatch.Begin 使用的光栅化状态引用。
    /// </summary>
    public RasterizerState RasterizerState;
    /// <summary>
    /// SpriteBatch.Begin 使用的自定义 Effect 引用，可为 null。
    /// </summary>
    public Effect CustomEffect;
    /// <summary>
    /// SpriteBatch.Begin 使用的坐标变换矩阵。
    /// </summary>
    public Matrix TransformMatrix;

    /// <summary>
    /// 捕获指定 <see cref="SpriteBatch"/> 当前的绘制状态。
    /// 快照可在临时修改批次状态前创建，并在绘制完成后用于恢复原设置。
    /// </summary>
    public SpriteBatchSnapshot(SpriteBatch spriteBatch)
    {
        SpriteBatch_Publicizer publicizer = new(spriteBatch);

        SortMode = publicizer.sortMode;
        BlendState = publicizer.blendState;
        SamplerState = publicizer.samplerState;
        DepthStencilState = publicizer.depthStencilState;
        RasterizerState = publicizer.rasterizerState;
        CustomEffect = publicizer.customEffect;
        TransformMatrix = publicizer.transformMatrix;
    }
}

