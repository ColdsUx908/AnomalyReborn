namespace Transoceanic.Graphics.Rendering;

public static partial class LightweightBloom
{
    internal static Asset<Effect> Shader;

    /// <summary>本机客户端辉光着色器是否已加载。</summary>
    public static bool IsAvailable => Shader?.IsLoaded == true;

    /// <summary>使用框架资产绘制图层；关闭或未加载时执行无滤波回退。其余契约与显式 Effect 入口相同。</summary>
    public static void Draw(SpriteBatch spriteBatch, bool enabled, in BloomSettings settings, Action drawLayer)
        => Draw(spriteBatch, enabled && IsAvailable ? Shader.Value : null, settings, drawLayer);

    /// <summary>使用框架资产和值类型图层；不为每次提交分配捕获委托。</summary>
    public static void Draw<TLayer>(SpriteBatch spriteBatch, bool enabled, in BloomSettings settings, TLayer layer)
        where TLayer : struct, IBloomLayer
        => Draw(spriteBatch, enabled && IsAvailable ? Shader.Value : null, settings, layer);
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
