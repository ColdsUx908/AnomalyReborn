namespace Transoceanic.DataStructures.Rendering;

/// <summary>
/// 一个结构体，用于保存 <see cref="SpriteBatch"/> 的当前绘制状态，以便在更改状态后能够恢复原始状态。
/// </summary>
public struct SpriteBatchSnapshot
{
    public SpriteSortMode sortMode;
    public BlendState blendState;
    public SamplerState samplerState;
    public DepthStencilState depthStencilState;
    public RasterizerState rasterizerState;
    public Effect customEffect;
    public Matrix transformMatrix;

    public SpriteBatchSnapshot(SpriteBatch spriteBatch)
    {
        SpriteBatch_Publicizer publicizer = new(spriteBatch);

        sortMode = publicizer.sortMode;
        blendState = publicizer.blendState;
        samplerState = publicizer.samplerState;
        depthStencilState = publicizer.depthStencilState;
        rasterizerState = publicizer.rasterizerState;
        customEffect = publicizer.customEffect;
        transformMatrix = publicizer.transformMatrix;
    }
}

