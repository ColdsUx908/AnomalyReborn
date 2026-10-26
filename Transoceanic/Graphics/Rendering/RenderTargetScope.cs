namespace Transoceanic.Graphics.Rendering;

/// <summary>
///     Manages the scope of a render target to be rendered to, swapping out the
///     currently-used targets of a device on creation and replacing it with the
///     given target.  Switches back to the old targets upon disposal.
/// </summary>
public readonly struct RenderTargetScope : IDisposable
{
    private readonly GraphicsDevice _GraphicsDevice;
    private readonly RenderTargetBinding[] _Previous;

    /// <summary>
    ///     Creates a new scope, saving the current device targets and starts
    ///     rendering to the new one instead.
    /// </summary>
    /// <param name="target">The target to render to.</param>
    /// <param name="preserveContents">
    ///     Whether to ensure swapped targets preserve their contents.
    /// </param>
    /// <param name="clearColor">
    ///     If not null, clears the target to the given color.
    /// </param>
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
    ///     Sets the device to use the targets that were in use before this
    ///     scope was instantiated.
    /// </summary>
    public void Dispose()
    {
        _GraphicsDevice.SetRenderTargets(_Previous);
    }
}

/// <summary>
///     Extensions supporting <see cref="RenderTargetScope"/>.
/// </summary>
public static class RenderTargetScopeExtensions
{
    /// <summary>
    ///     Creates a new scope, saving the current device targets and starts
    ///     rendering to the new one instead.
    /// </summary>
    /// <param name="target">The target to render to.</param>
    /// <param name="preserveContents">
    ///     Whether to ensure swapped targets preserve their contents.
    /// </param>
    /// <param name="clearColor">
    ///     If not null, clears the target to the given color.
    /// </param>
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

    /// <summary>
    ///     Creates a new scope, saving the current device targets and starts
    ///     rendering to the new one instead.
    /// </summary>
    /// <param name="target">The target to render to.</param>
    /// <param name="preserveContents">
    ///     Whether to ensure swapped targets preserve their contents.
    /// </param>
    /// <param name="clearColor">
    ///     If not null, clears the target to the given color.
    /// </param>
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

