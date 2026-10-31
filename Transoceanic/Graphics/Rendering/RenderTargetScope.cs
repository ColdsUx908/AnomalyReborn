namespace Transoceanic.Graphics.Rendering;

/// <summary>
/// 管理渲染目标作用域：创建时保存设备原有绑定并切换到指定目标，释放时恢复原有绑定。
/// </summary>
public readonly struct RenderTargetScope : IDisposable
{
    private readonly GraphicsDevice _GraphicsDevice;
    private readonly RenderTargetBinding[] _Previous;

    /// <summary>
    /// 创建作用域，保存图形设备的当前目标绑定并切换到指定目标。
    /// </summary>
    /// <param name="target">要绘制到的渲染目标。</param>
    /// <param name="preserveContents">是否将切换前绑定的二维目标设为保留内容；其原保留策略不会自动恢复。</param>
    /// <param name="clearColor">非 null 时，将新目标清空为指定颜色。</param>
    public RenderTargetScope(
        RenderTarget2D target,
        bool preserveContents = true,
        Color? clearColor = null
    )
    {
        ArgumentNullException.ThrowIfNull(target);

        _GraphicsDevice = target.GraphicsDevice;
        _Previous = _GraphicsDevice.GetRenderTargets();

        if (preserveContents)
        {
            RenderTargetPreserver.PreserveBindings(_Previous);
        }

        _GraphicsDevice.SetRenderTarget(target);

        if (clearColor.HasValue)
        {
            _GraphicsDevice.Clear(clearColor.Value);
        }
    }

    /// <summary>
    /// 恢复创建此作用域前的渲染目标绑定。
    /// </summary>
    public void Dispose()
    {
        _GraphicsDevice.SetRenderTargets(_Previous);
    }
}

/// <summary>
/// 创建 <see cref="RenderTargetScope"/> 的扩展方法。
/// </summary>
public static class RenderTargetScopeExtensions
{
    /// <inheritdoc cref="RenderTargetScope.RenderTargetScope(RenderTarget2D, bool, Color?)"/>
    /// <returns>释放时恢复原绑定的作用域；不会归还目标租约。</returns>
    public static RenderTargetScope Scope(
        this RenderTarget2D target,
        bool preserveContents = true,
        Color? clearColor = null
    )
    {
        return new RenderTargetScope(
            target,
            preserveContents,
            clearColor
        );
    }

    /// <inheritdoc cref="RenderTargetScope.RenderTargetScope(RenderTarget2D, bool, Color?)"/>
    /// <remarks>target 为持有目标的租约；作用域只恢复绑定，不归还租约。</remarks>
    /// <returns>释放时恢复原绑定的作用域；不会归还目标租约。</returns>
    public static RenderTargetScope Scope(
        this RenderTargetLease target,
        bool preserveContents = true,
        Color? clearColor = null
    )
    {
        return new RenderTargetScope(
            target.Target,
            preserveContents,
            clearColor
        );
    }
}

