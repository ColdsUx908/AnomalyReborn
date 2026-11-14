using Terraria.Graphics.Shaders;

namespace Anomalies.Assets.Effects;

/// <summary>统一屏幕 Filter 的参数入口；内容方可在应用 shader 前更新受相机影响的参数。</summary>
public sealed class AnomalyScreenShaderData(Asset<Effect> effect, string passName) : ScreenShaderData(effect, passName)
{
    /// <summary>在原生 Apply 前执行的可选参数更新；不负责资产加载或 Filter 注册，统一卸载时清除。</summary>
    public Action<ScreenShaderData> BeforeApply { get; set; }

    public override void Apply()
    {
        BeforeApply?.Invoke(this);
        base.Apply();
    }
}
