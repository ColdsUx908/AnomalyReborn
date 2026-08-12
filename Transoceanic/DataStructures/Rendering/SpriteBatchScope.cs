namespace Transoceanic.DataStructures.Rendering;

/// <summary>
/// 用于临时改变 SpriteBatch 绘制状态的作用域对象。
/// 构造时应用新状态，释放时自动恢复原始状态。
/// </summary>
public sealed class SpriteBatchScope : IDisposable
{
    private readonly SpriteBatch _SpriteBatch;
    private readonly SpriteBatchSnapshot _Snapshot;
    private bool _Disposed;

    /// <summary>
    /// 初始化作用域，并应用指定的新状态。
    /// </summary>
    /// <param name="spriteBatch">要操作的 SpriteBatch 实例。</param>
    /// <param name="sortMode">新排序模式（null 表示不变）。</param>
    /// <param name="blendState">新混合状态（null 表示不变）。</param>
    /// <param name="samplerState">新采样器状态（null 表示不变）。</param>
    /// <param name="depthStencilState">新深度模板状态（null 表示不变）。</param>
    /// <param name="rasterizerState">新光栅化状态（null 表示不变）。</param>
    /// <param name="customEffect">新自定义效果（null 表示不变）。</param>
    /// <param name="transformMatrix">新变换矩阵（null 表示不变）。</param>
    public SpriteBatchScope(
        SpriteBatch spriteBatch,
        SpriteSortMode? sortMode = null,
        BlendState blendState = null,
        SamplerState samplerState = null,
        DepthStencilState depthStencilState = null,
        RasterizerState rasterizerState = null,
        Effect customEffect = null,
        Matrix? transformMatrix = null)
    {
        _SpriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
        _Snapshot = spriteBatch.ChangeState(
            sortMode, blendState, samplerState,
            depthStencilState, rasterizerState,
            customEffect, transformMatrix);
    }

    /// <summary>
    /// 恢复 SpriteBatch 到构造前的原始状态。
    /// </summary>
    public void Dispose()
    {
        if (_Disposed)
            return;

        _SpriteBatch.ResetState(_Snapshot);
        _Disposed = true;
    }
}