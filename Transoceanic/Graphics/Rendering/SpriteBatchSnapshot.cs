using Transoceanic.Runtime.Publicizing.Wrappers;
namespace Transoceanic.Graphics.Rendering;

/// <summary>
/// 一个结构体，用于保存 <see cref="SpriteBatch"/> 的当前绘制状态，以便在更改状态后能够恢复原始状态。
/// </summary>
public struct SpriteBatchSnapshot
{
    public SpriteSortMode SortMode;
    public BlendState BlendState;
    public SamplerState SamplerState;
    public DepthStencilState DepthStencilState;
    public RasterizerState RasterizerState;
    public Effect CustomEffect;
    public Matrix TransformMatrix;

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

