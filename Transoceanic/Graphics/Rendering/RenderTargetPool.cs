namespace Transoceanic.Graphics.Rendering;

/// <summary>
/// 描述渲染目标的创建参数。
/// </summary>
/// <param name="Format">目标颜色缓冲区格式。</param>
/// <param name="Depth">深度/模板缓冲区格式，None 表示不创建。</param>
/// <param name="MultiSampleCount">多重采样数，负值按零保存；实际支持范围由图形设备决定。</param>
/// <param name="Usage">目标切换时的内容保留策略；复用的池目标不会自动清屏。</param>
/// <param name="GenerateMipmaps">是否创建 mipmap 层级。</param>
public readonly record struct RenderTargetDescriptor(
    SurfaceFormat Format,
    DepthFormat Depth,
    int MultiSampleCount,
    RenderTargetUsage Usage,
    bool GenerateMipmaps
)
{
    /// <summary>
    /// 默认创建参数。
    /// </summary>
    public static RenderTargetDescriptor Default { get; } = new(
        SurfaceFormat.Color,
        DepthFormat.None,
        0,
        RenderTargetUsage.DiscardContents,
        false
    );

    /// <summary>
    /// 与 <see cref="Default"/> 相同，但使用 <see cref="RenderTargetUsage.PreserveContents"/> 保留目标内容。
    /// </summary>
    public static RenderTargetDescriptor DefaultPreserveContents { get; } = new(
        SurfaceFormat.Color,
        DepthFormat.None,
        0,
        RenderTargetUsage.PreserveContents,
        false
    );

    /// <summary>
    /// 非负的请求多重采样数；负的构造参数被限制为零。
    /// </summary>
    public int MultiSampleCount { get; } = Math.Max(0, MultiSampleCount);

    /// <summary>
    /// 根据当前描述符创建二维渲染目标。
    /// </summary>
    /// <param name="device">用于创建目标的图形设备。</param>
    /// <param name="width">目标宽度，单位为像素，必须大于零。</param>
    /// <param name="height">目标高度，单位为像素，必须大于零。</param>
    public RenderTarget2D Create(GraphicsDevice device, int width, int height)
    {
        return new RenderTarget2D(device, width, height, GenerateMipmaps, Format, Depth, MultiSampleCount, Usage);
    }

    /// <summary>
    /// 从已有渲染目标提取创建参数。
    /// </summary>
    public static RenderTargetDescriptor From(RenderTarget2D target)
    {
        return new RenderTargetDescriptor(
            target.Format,
            target.DepthStencilFormat,
            target.MultiSampleCount,
            target.RenderTargetUsage,
            target.LevelCount > 1
        );
    }
}

/// <summary>
/// 池中渲染目标的租约，使用完毕后须归还。
/// </summary>
/// <param name="pool">出租目标的池。</param>
/// <param name="target">租用的渲染目标。</param>
public sealed class RenderTargetLease(
    RenderTarget2D target,
    RenderTargetPool pool
) : IDisposable
{
    /// <summary>
    ///     当前租用的目标；归还或释放池后不得再读取或绘制此目标。
    /// </summary>
    public RenderTarget2D Target { get; } = target;

    private bool _Returned;

    /// <summary>
    /// 校验租约所属池并标记首次归还；重复归还返回 false，池不匹配抛出 ArgumentException。
    /// </summary>
    internal bool TryReturn(RenderTargetPool owner)
    {
        if (!ReferenceEquals(pool, owner))
            throw new ArgumentException("The lease belongs to another render target pool.", nameof(owner));
        if (_Returned)
            return false;
        _Returned = true;
        return true;
    }

    /// <summary>
    /// 将租用的目标归还所属池；重复调用无效。
    /// </summary>
    public void Dispose()
    {
        pool.Return(this);
    }
}

/// <summary>
/// 提供可复用渲染目标实例的资源池。
/// </summary>
/// <remarks>通过租用和归还复用目标，减少频繁创建和销毁图形资源的开销。<br /> 本类不保证线程安全，除非另有说明，所有方法及其实现均须在图形主线程调用。</remarks>
public abstract class RenderTargetPool : IDisposable
{
    // Store as field of derived shared type so the JIT can de-virtualize calls
    // to it when the Shared property gets inlined.
    private static SharedRenderTargetPool _Shared = new();

    /// <summary>
    /// 获取共享的 <see cref="RenderTargetPool"/> 实例。
    /// </summary>
    /// <remarks>
    /// 按设备、像素尺寸和描述符精确匹配，可能复用包含旧图像的目标；使用前按需清屏。
    /// 缓存最多保留每键 4 个、总计 128 个目标，估算缓存显存不超过 128 MiB。
    /// 租用和每帧绘制开始时检查空闲过期项；卸载时释放并替换共享池。
    /// 预算不限制正在租用的目标，不包含驱动分配对齐等额外开销。
    /// </remarks>
    public static RenderTargetPool Shared => _Shared;

    /// <summary>
    /// 按指定设备、像素宽度 <paramref name="width"/>、高度 <paramref name="height"/> 和创建参数 <paramref name="descriptor"/> 租用渲染目标。
    /// </summary>
    /// <param name="device">用于创建或匹配目标的图形设备。</param>
    /// <param name="width">目标宽度，单位为像素，必须大于零。</param>
    /// <param name="height">目标高度，单位为像素，必须大于零。</param>
    /// <param name="descriptor">渲染目标的创建参数。</param>
    /// <returns>渲染目标租约；使用结束并解除目标绑定后释放租约，将目标归还池。</returns>
    /// <remarks>
    /// 调用方用 using 管理返回的租约，结束绘制并解除目标绑定后 Dispose，将目标归还同一个池。
    /// 不要直接 Dispose 租约的 Target；池负责底层资源释放。复用目标不保证内容已清空。
    /// 本方法在图形主线程调用；专用服务器应跳过图形资源操作。
    /// </remarks>
    public abstract RenderTargetLease Rent(
        GraphicsDevice device,
        int width,
        int height,
        RenderTargetDescriptor descriptor
    );

    /// <summary>
    /// 归还此前通过本池 <see cref="Rent"/> 获取的渲染目标租约。
    /// </summary>
    /// <param name="lease">此前通过本池 <see cref="Rent"/> 获取的租约。</param>
    /// <remarks>
    /// RenderTargetLease.Dispose 自动调用本方法。同一租约重复归还无效，错误池归还会抛出 ArgumentException。
    /// 归还后不得再使用 Target。共享池会缓存可复用目标，池已释放、目标已释放或容量已满时释放或丢弃目标。
    /// </remarks>
    /// <exception cref="ArgumentNullException">租约为 null。</exception>
    /// <exception cref="ArgumentException">租约属于其他池。</exception>
    public void Return(RenderTargetLease lease)
    {
        ArgumentNullException.ThrowIfNull(lease);
        if (lease.TryReturn(this))
            ReturnCore(lease.Target);
    }

    /// <summary>
    /// 处理已通过租约归还校验的渲染目标，由派生池缓存或释放目标。
    /// </summary>
    protected abstract void ReturnCore(RenderTarget2D target);

    /// <summary>
    /// 释放池及其拥有的渲染目标，包括仍在出租的目标。<br /> 仅应在拥有该池且确认目标已不再使用时，或在模组卸载等必须释放资源的阶段调用。
    /// </summary>
    public abstract void Dispose();

    private static List<RenderTargetLease> _LeasesToClear = [];
    private static List<RenderTargetLease> _LeasesBeingCleared = [];
    private static bool _ClearingLeases;

    /// <summary>
    /// 登记待归还租约，在下一次绘制开始时释放；适用于本帧绘制需要移交租约所有权的情况。
    /// </summary>
    /// <param name="lease">在下一次绘制开始时归还的租约。</param>
    /// <remarks>优先由调用方明确管理租约生命周期；仅在无法保持本帧目标所有权时使用延迟归还。</remarks>
    public static void ReturnNextFrame(RenderTargetLease lease)
    {
        ArgumentNullException.ThrowIfNull(lease);
        _LeasesToClear.Add(lease);
    }

    /// <summary>
    /// 归还通过 <see cref="ReturnNextFrame"/> 登记的全部待处理租约；单项失败不阻止其余归还，最后聚合抛出异常。
    /// </summary>
    internal static void ClearPendingLeases()
    {
        if (_ClearingLeases)
            return;
        _Shared.TrimAged();
        if (_LeasesToClear.Count == 0)
            return;
        (_LeasesToClear, _LeasesBeingCleared) = (_LeasesBeingCleared, _LeasesToClear);
        _ClearingLeases = true;
        List<Exception> failures = null;
        try
        {
            foreach (RenderTargetLease lease in _LeasesBeingCleared)
            {
                try
                {
                    lease.Dispose();
                }
                catch (Exception exception)
                {
                    (failures ??= []).Add(exception);
                }
            }
        }
        finally
        {
            _LeasesBeingCleared.Clear();
            _ClearingLeases = false;
        }
        if (failures is not null)
            throw new AggregateException("Deferred render target return failed.", failures);
    }

    /// <summary>
    /// 归还延迟租约，并在 finally 中释放共享池、创建新的空共享池；在图形主线程调用。
    /// </summary>
    internal static void UnloadShared()
    {
        try
        {
            ClearPendingLeases();
        }
        finally
        {
            _Shared.Dispose();
            _Shared = new();
        }
    }
}

/// <summary>
/// 适用于任意 <see cref="RenderTargetPool"/> 实现的租用扩展方法。
/// </summary>
internal static class RenderTargetPoolExtensions
{
    // TODO: Vector2 scale extensions and individual float width/height
    //       extensions.

    /// <param name="pool">提供租约的渲染目标池。</param>
    extension(RenderTargetPool pool)
    {
        /// <summary>
        /// 使用默认创建参数租用指定像素尺寸的渲染目标。
        /// </summary>
        public RenderTargetLease Rent(
            GraphicsDevice device,
            int width,
            int height
        )
        {
            return pool.Rent(
                device,
                width,
                height,
                RenderTargetDescriptor.Default
            );
        }

        /// <summary>
        /// 使用默认创建参数，按 <paramref name="scale"/> 缩放 <paramref name="baseSize"/>，各维向上取整且至少为一像素后租用目标。
        /// </summary>
        /// <param name="device">用于创建或匹配目标的图形设备。</param>
        /// <param name="baseSize">未缩放的目标像素尺寸。</param>
        /// <param name="scale">目标缩放因子，必须大于零。</param>
        /// <returns>渲染目标租约；使用结束并解除目标绑定后释放租约，将目标归还池。</returns>
        public RenderTargetLease RentScaled(
            GraphicsDevice device,
            Point baseSize,
            float scale
        )
        {
            return pool.RentScaled(
                device,
                baseSize,
                scale,
                RenderTargetDescriptor.Default
            );
        }

        /// <summary>
        /// 按 <paramref name="scale"/> 缩放 <paramref name="baseSize"/>，各维向上取整且至少为一像素，使用 <paramref name="descriptor"/> 租用目标。
        /// </summary>
        /// <param name="device">用于创建或匹配目标的图形设备。</param>
        /// <param name="baseSize">未缩放的目标像素尺寸。</param>
        /// <param name="scale">目标缩放因子，必须大于零。</param>
        /// <param name="descriptor">渲染目标的创建参数。</param>
        /// <returns>渲染目标租约；使用结束并解除目标绑定后释放租约，将目标归还池。</returns>
        public RenderTargetLease RentScaled(
            GraphicsDevice device,
            Point baseSize,
            float scale,
            RenderTargetDescriptor descriptor
        )
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(scale, 0f);

            int width = Math.Max(1, (int)MathF.Ceiling(baseSize.X * scale));
            int height = Math.Max(1, (int)MathF.Ceiling(baseSize.Y * scale));
            return pool.Rent(device, width, height, descriptor);
        }
    }
}

/// <summary>
/// 按设备、尺寸和描述符缓存渲染目标的共享池，限制缓存容量并淘汰过期目标。
/// </summary>
internal sealed class SharedRenderTargetPool : RenderTargetPool
{
    private readonly record struct Key(
        GraphicsDevice Device,
        int Width,
        int Height,
        RenderTargetDescriptor Descriptor
    )
    {
        /// <summary>
        /// 从渲染目标提取设备、像素尺寸和创建描述符，生成缓存键。
        /// </summary>
        public static Key From(RenderTarget2D target)
        {
            return new Key(target.GraphicsDevice, target.Width, target.Height, RenderTargetDescriptor.From(target));
        }
    }

    private sealed class Entry
    {
        /// <summary>
        /// 此缓存键下可再次租出的渲染目标栈。
        /// </summary>
        public Stack<RenderTarget2D> Targets { get; } = [];

        /// <summary>
        /// 该缓存键最近租出或归还目标时的 UTC 时间，用于过期清理。
        /// </summary>
        public DateTime LastUsed { get; set; } = DateTime.UtcNow;
    }

    // TODO: We can tweak these later for performance?
    private const int MaxPerKey = 4;         // Max identical targets.
    private const int MaxTotalTargets = 128; // Max targets stored.
    internal const long MaxCachedBytes = 128L * 1024 * 1024;
    private static readonly TimeSpan _MaxIdleTime = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan _MinimumTrimTime = TimeSpan.FromSeconds(1);

    private readonly Dictionary<Key, Entry> _Cache = [];
    private readonly HashSet<RenderTarget2D> _LeasedTargets = [];
    private DateTime _LastTrimmed = DateTime.UtcNow;
    private int _TotalCached;
    internal long CachedBytes { get; private set; }
    private bool _Disposed;

    /// <inheritdoc/>
    public override RenderTargetLease Rent(GraphicsDevice device, int width, int height, RenderTargetDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(width, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(height, 0);
        ObjectDisposedException.ThrowIf(_Disposed, this);

        try
        {
            Key key = new(device, width, height, descriptor);
            if (!_Cache.TryGetValue(key, out Entry entry))
            {
                _Cache[key] = entry = new Entry();
            }
            else
            {
                entry.LastUsed = DateTime.UtcNow;
            }

            RenderTarget2D target;
            while (entry.Targets.Count > 0 && entry.Targets.Peek().IsDisposed)
            {
                entry.Targets.Pop();
                _TotalCached--;
                CachedBytes -= EstimateBytes(width, height, descriptor);
            }
            if (entry.Targets.Count > 0)
            {
                target = entry.Targets.Pop();
                _TotalCached--;
                CachedBytes -= EstimateBytes(width, height, descriptor);
            }
            else
            {
                target = descriptor.Create(device, width, height);
            }

            _LeasedTargets.Add(target);
            return new RenderTargetLease(target, this);
        }
        finally
        {
            TrimAged();
        }
    }

    /// <inheritdoc/>
    protected override void ReturnCore(RenderTarget2D target)
    {
        if (!_LeasedTargets.Remove(target))
            throw new InvalidOperationException("This target was not rented from this pool.");
        if (_Disposed || target.IsDisposed)
        {
            if (!target.IsDisposed)
                target.Dispose();
            return;
        }

        Key key = Key.From(target);
        if (!_Cache.TryGetValue(key, out Entry entry))
        {
            _Cache[key] = entry = new Entry();
        }
        else
        {
            // Mark it as used on return because that's realistically where the
            // countdown should begin.
            entry.LastUsed = DateTime.UtcNow;
        }

        if (entry.Targets.Count < MaxPerKey)
        {
            long bytes = EstimateBytes(target.Width, target.Height, key.Descriptor);
            if (_TotalCached >= MaxTotalTargets || bytes > MaxCachedBytes - CachedBytes)
            {
                target.Dispose();
                return;
            }

            entry.Targets.Push(target);
            _TotalCached++;
            CachedBytes += bytes;
        }
        else
        {
            target.Dispose();
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 释放全部缓存和仍出租的目标；重复调用无效，尚存租约随后仍可归还。
    /// </remarks>
    public override void Dispose()
    {
        if (_Disposed)
        {
            return;
        }

        Trim();
        foreach (RenderTarget2D target in _LeasedTargets)
            target.Dispose();
        _Disposed = true;
    }

    private void Trim()
    {
        foreach (Entry entry in _Cache.Values)
        {
            while (entry.Targets.Count > 0)
            {
                RenderTarget2D target = entry.Targets.Pop();
                CachedBytes -= EstimateBytes(target.Width, target.Height, RenderTargetDescriptor.From(target));
                target.Dispose();
                _TotalCached--;
            }
        }

        _Cache.Clear();
    }

    internal void TrimAged()
    {
        DateTime now = DateTime.UtcNow;

        if (now - _LastTrimmed < _MinimumTrimTime)
        {
            return;
        }

        _LastTrimmed = now;

        foreach ((Key key, Entry entry) in _Cache)
        {
            if (now - entry.LastUsed <= _MaxIdleTime)
            {
                continue;
            }

            while (entry.Targets.Count > 0)
            {
                RenderTarget2D target = entry.Targets.Pop();
                CachedBytes -= EstimateBytes(target.Width, target.Height, key.Descriptor);
                target.Dispose();
                _TotalCached--;
            }

            _Cache.Remove(key);
        }
    }

    // Conservative payload estimate: unknown color formats use 16 bytes per pixel.
    // Includes mip levels, multisample color/depth and the resolved color surface.
    internal static long EstimateBytes(int width, int height, RenderTargetDescriptor descriptor)
    {
        int colorBytes = descriptor.Format == SurfaceFormat.Color ? 4 : 16;
        int depthBytes = descriptor.Depth == DepthFormat.None ? 0 : 4;
        long pixels = 0;
        int mipWidth = width;
        int mipHeight = height;
        do
        {
            pixels += (long)mipWidth * mipHeight;
            if (!descriptor.GenerateMipmaps || (mipWidth == 1 && mipHeight == 1))
                break;
            mipWidth = Math.Max(1, mipWidth / 2);
            mipHeight = Math.Max(1, mipHeight / 2);
        } while (true);
        int samples = Math.Max(1, descriptor.MultiSampleCount);
        return pixels * colorBytes + (long)width * height *
            (depthBytes * (long)samples + (samples > 1 ? colorBytes * (long)samples : 0));
    }
}

