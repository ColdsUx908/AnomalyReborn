namespace Transoceanic.Graphics.Rendering;

public static partial class LightweightBloom
{
    internal static Asset<Effect> Shader;

    /// <summary>本机客户端辉光着色器是否已加载。</summary>
    public static bool IsAvailable => Shader?.IsLoaded == true;

    /// <summary>
    /// 在当前绘制层合并处理一次回调提交的全部效果。无内容时不要调用。
    /// 调用前后 SpriteBatch 必须未开始；回调自行 Begin/End，按原视口像素及游戏矩阵绘制。
    /// 回调输出应为预乘 RGBA；建议用 AdditiveBlend 捕获发光内容。
    /// 禁用或 shader 不可用时直接执行回调，不捕获和滤波。此方法不注册任何绘制钩子。
    /// </summary>
    public static void Draw(SpriteBatch spriteBatch, bool enabled, in BloomSettings settings, Action drawLayer)
        => Draw(spriteBatch, enabled && IsAvailable ? Shader.Value : null, settings, drawLayer);

    /// <summary>
    /// 使用调用者持有的兼容 Effect 绘制图层，适用于独立宿主和自定义资源生命周期。
    /// effect 为 null 时直接绘制本体，其余约定与自动资产版本相同；不取得 Effect 的所有权。
    /// </summary>
    public static void Draw(SpriteBatch spriteBatch, Effect effect, in BloomSettings settings, Action drawLayer)
    {
        ArgumentNullException.ThrowIfNull(drawLayer);
        if (effect is null)
        {
            drawLayer();
            return;
        }
        GraphicsDevice device = spriteBatch.GraphicsDevice;
        Viewport viewport = device.Viewport;
        Rectangle scissor = device.ScissorRectangle;
        RenderTargetBinding[] previous = device.GetRenderTargets();
        RenderTargetUsage backbufferUsage = device.PresentationParameters.RenderTargetUsage;
        BlendState blend = device.BlendState;
        DepthStencilState depth = device.DepthStencilState;
        RasterizerState rasterizer = device.RasterizerState;
        SamplerState sampler = device.SamplerStates[0];
        Texture texture = device.Textures[0];
        int width = viewport.Width;
        int height = viewport.Height;
        using RenderTargetLease source = RenderTargetPool.Shared.Rent(device, width, height, RenderTargetDescriptor.Default);
        using RenderTargetLease nearScratch = RenderTargetPool.Shared.Rent(device, Math.Max(1, width / 2), Math.Max(1, height / 2), RenderTargetDescriptor.Default);
        using RenderTargetLease near = RenderTargetPool.Shared.Rent(device, nearScratch.Target.Width, nearScratch.Target.Height, RenderTargetDescriptor.Default);
        using RenderTargetLease farScratch = settings.WideHalo
            ? RenderTargetPool.Shared.Rent(device, Math.Max(1, width / 4), Math.Max(1, height / 4), RenderTargetDescriptor.Default) : null;
        using RenderTargetLease far = settings.WideHalo
            ? RenderTargetPool.Shared.Rent(device, farScratch.Target.Width, farScratch.Target.Height, RenderTargetDescriptor.Default) : null;
        try
        {
            // FNA may discard either an RT or a backbuffer when rebound, even without Clear.
            RenderTargetPreserver.PreserveBindings(previous);
            if (previous.Length == 0)
                device.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
            device.SetRenderTarget(source.Target);
            device.Clear(Color.Transparent);
            drawLayer();
            Blur(spriteBatch, effect, source.Target, nearScratch.Target, near.Target, settings.NearRadius);
            if (settings.WideHalo)
                Blur(spriteBatch, effect, source.Target, farScratch.Target, far.Target, settings.FarRadius);
            device.SetRenderTargets(previous);
            device.Viewport = viewport;
            device.ScissorRectangle = scissor;
            Composite(spriteBatch, effect, source.Target, near.Target, far?.Target,
                new Rectangle(0, 0, width, height), settings);
        }
        finally
        {
            device.SetRenderTargets(previous);
            device.Viewport = viewport;
            device.ScissorRectangle = scissor;
            device.PresentationParameters.RenderTargetUsage = backbufferUsage;
            device.BlendState = blend;
            device.DepthStencilState = depth;
            device.RasterizerState = rasterizer;
            device.SamplerStates[0] = sampler;
            device.Textures[0] = texture;
        }
    }
}

internal sealed class LightweightBloomAssets : ILoadable
{
    public void Load(Mod mod)
    {
        if (!Main.dedServ)
            LightweightBloom.Shader = mod.Assets.Request<Effect>("Assets/Effects_Compiled/LightweightBloom", AssetRequestMode.ImmediateLoad);
    }

    public void Unload()
    {
        LightweightBloom.Shader = null;
        LightweightBloom.ReleaseResources();
    }
}
