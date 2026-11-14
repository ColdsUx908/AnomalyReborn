namespace Transoceanic.Graphics.Rendering;

/// <summary>按最终屏幕像素配置的双尺度辉光；关闭宽光晕可减少一组滤波。</summary>
/// <param name="NearRadius">紧密光晕的高斯标准差，单位为最终画面像素。</param>
/// <param name="FarRadius">扩散光晕的高斯标准差。</param>
/// <param name="NearStrength">紧密光晕强度。</param>
/// <param name="FarStrength">扩散光晕强度。</param>
/// <param name="WideHalo">是否计算扩散光晕。</param>
/// <param name="AdditiveBody">是否以预乘加色合成清晰本体，否则使用 AlphaBlend。</param>
/// <param name="BodyPasses">清晰本体合成次数，0 表示只合成光晕，最大为 2。</param>
public readonly record struct BloomSettings(float NearRadius, float FarRadius, float NearStrength,
    float FarStrength, bool WideHalo = true, bool AdditiveBody = false, int BodyPasses = 1)
{
    /// <summary>近光晕 Screen 合成次数，限制为 1～3。</summary>
    public int NearPasses { get; init; } = 1;
    /// <summary>远光晕 Screen 合成次数，限制为 1～3。</summary>
    public int FarPasses { get; init; } = 1;

    /// <summary>对应两层 8px/26px 光晕的默认设置。</summary>
    public static BloomSettings Default => new(8f, 26f, 0.85f, 0.9f);
}

/// <summary>主动提交图层的轻量 Bloom。滤波核心只依赖 FNA，可在独立预览中复用。</summary>
public static partial class LightweightBloom
{
    /// <summary>预乘颜色的加色混合，同时累加覆盖度；适用于透明特效层的捕获。</summary>
    public static BlendState AdditiveBlend
    {
        get => field ??= new BlendState
        {
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One
        };
        private set
        {
            field?.Dispose();
            field = value;
        }
    }

    private static BlendState ScreenBlend
    {
        get => field ??= new BlendState
        {
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.InverseSourceColor,
            AlphaSourceBlend = Blend.Zero,
            AlphaDestinationBlend = Blend.One
        };
        set
        {
            field?.Dispose();
            field = value;
        }
    }

    /// <summary>
    /// 将源图层滤波到 caller-owned 的 scratch/result；结果不与场景混合。
    /// 两个目标须等尺寸且与 source 不同。调用前后 SpriteBatch 均未开始，返回时 result 为当前目标。
    /// 半径始终按 source 的像素尺寸解释，低分辨率滤波不会改变最终光晕大小。
    /// </summary>
    public static void Blur(SpriteBatch spriteBatch, Effect effect, Texture2D source,
        RenderTarget2D scratch, RenderTarget2D result, float radius)
    {
        ValidateBatch(spriteBatch);
        GraphicsDevice device = spriteBatch.GraphicsDevice;
        ValidateResource(device, effect, nameof(effect));
        ValidateResource(device, source, nameof(source));
        ValidateResource(device, scratch, nameof(scratch));
        ValidateResource(device, result, nameof(result));
        if (ReferenceEquals(source, scratch) || ReferenceEquals(source, result) || ReferenceEquals(scratch, result)
            || scratch.Width != result.Width || scratch.Height != result.Height)
            throw new ArgumentException("Bloom requires distinct, equally sized scratch/result targets.");
        float sigma = float.IsFinite(radius) ? Math.Max(radius, 0f) : 0f;
        // 先覆盖降采样的像素足迹，不能直接从原图稀疏抽取细星芒。
        effect.Parameters["uDownsampleStep"].SetValue(new Vector2(
            source.Width > result.Width ? 0.25f / result.Width : 0f,
            source.Height > result.Height ? 0.25f / result.Height : 0f));
        Filter(source, result, Vector2.Zero, "Downsample");

        // 每轮方差相加。首轮相邻采样不超过一个工作像素，之后只在已经平滑的
        // 图层上扩大核，避免大半径将十字星复制成横纵网格。复用原有两个目标。
        const float KernelVariance = 3.9512626f;
        double remainingVariance = (double)sigma * sigma;
        double accumulatedVariance = 0;
        double firstStep = Math.Min((double)source.Width / result.Width, (double)source.Height / result.Height);
        double firstVariance = KernelVariance * firstStep * firstStep;
        while (remainingVariance > 0.0001)
        {
            double passVariance = Math.Min(remainingVariance,
                Math.Max(firstVariance, accumulatedVariance * 3));
            float step = (float)Math.Sqrt(passVariance / KernelVariance);
            Filter(result, scratch, new Vector2(step / source.Width, 0f));
            Filter(scratch, result, new Vector2(0f, step / source.Height));
            accumulatedVariance += passVariance;
            remainingVariance = Math.Max(0f, remainingVariance - passVariance);
        }

        void Filter(Texture2D input, RenderTarget2D output, Vector2 step, string pass = "Pass0")
        {
            spriteBatch.GraphicsDevice.SetRenderTarget(output);
            spriteBatch.GraphicsDevice.Clear(Color.Transparent);
            effect.Parameters["uBlurStep"].SetValue(step);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone);
            effect.CurrentTechnique.Passes[pass].Apply();
            spriteBatch.Draw(input, new Rectangle(0, 0, output.Width, output.Height), Color.White);
            spriteBatch.End();
        }
    }

    /// <summary>
    /// 在当前目标上先 Screen 合成光晕、再合成清晰本体。纹理必须为预乘颜色。
    /// 目标不得与任一输入相同，调用前后 SpriteBatch 均未开始；不会切换目标。
    /// </summary>
    public static void Composite(SpriteBatch spriteBatch, Effect effect, Texture2D source,
        Texture2D near, Texture2D far, Rectangle destination, in BloomSettings settings)
    {
        ValidateBatch(spriteBatch);
        GraphicsDevice device = spriteBatch.GraphicsDevice;
        ValidateResource(device, effect, nameof(effect));
        ValidateResource(device, source, nameof(source));
        if (near is not null)
            ValidateResource(device, near, nameof(near));
        if (far is not null && settings.WideHalo)
            ValidateResource(device, far, nameof(far));
        foreach (RenderTargetBinding binding in device.GetRenderTargets())
            if (ReferenceEquals(binding.RenderTarget, source) || ReferenceEquals(binding.RenderTarget, near)
                || settings.WideHalo && ReferenceEquals(binding.RenderTarget, far))
                throw new ArgumentException("Bloom cannot composite a texture into itself.");
        if (destination.Width <= 0 || destination.Height <= 0)
            return;
        if (far is not null && settings.WideHalo)
            for (int i = 0; i < Math.Clamp(settings.FarPasses, 1, 3); i++)
                DrawHalo(far, settings.FarStrength);
        if (near is not null)
            for (int i = 0; i < Math.Clamp(settings.NearPasses, 1, 3); i++)
                DrawHalo(near, settings.NearStrength);
        spriteBatch.Begin(SpriteSortMode.Deferred, settings.AdditiveBody ? AdditiveBlend : BlendState.AlphaBlend,
            SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
        for (int i = 0; i < Math.Clamp(settings.BodyPasses, 0, 2); i++)
            spriteBatch.Draw(source, destination, Color.White);
        spriteBatch.End();

        void DrawHalo(Texture2D texture, float strength)
        {
            if (!float.IsFinite(strength) || strength <= 0f)
                return;
            effect.Parameters["uStrength"].SetValue(strength);
            spriteBatch.Begin(SpriteSortMode.Immediate, ScreenBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone);
            effect.CurrentTechnique.Passes["Halo"].Apply();
            spriteBatch.Draw(texture, destination, Color.White);
            spriteBatch.End();
        }
    }

    /// <summary>释放工具自己持有的混合状态；游戏卸载时自动调用，独立宿主退出时可调用。</summary>
    public static void ReleaseResources()
    {
        ScreenBlend = null;
        AdditiveBlend = null;
    }

    private static void ValidateResource(GraphicsDevice device, GraphicsResource resource, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(resource, parameterName);
        ObjectDisposedException.ThrowIf(resource.IsDisposed, resource);
        if (!ReferenceEquals(device, resource.GraphicsDevice))
            throw new ArgumentException("Bloom resources must belong to the SpriteBatch graphics device.", parameterName);
    }

    private static void ValidateBatch(SpriteBatch spriteBatch)
    {
        ArgumentNullException.ThrowIfNull(spriteBatch);
        if (new Transoceanic.Runtime.Publicizing.Wrappers.SpriteBatch_Publicizer(spriteBatch).beginCalled)
            throw new InvalidOperationException("Bloom requires an inactive SpriteBatch.");
    }
}
