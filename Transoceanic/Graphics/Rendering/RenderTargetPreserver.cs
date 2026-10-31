using Transoceanic.Runtime.Publicizing.Wrappers;
namespace Transoceanic.Graphics.Rendering;

/// <summary>
/// 设置二维渲染目标的内容保留策略。<br /> 临时切换渲染目标时，通常优先使用 <see cref="RenderTargetScope"/>。
/// </summary>
public static class RenderTargetPreserver
{
    /// <summary>
    /// 将给定绑定中的二维渲染目标强制设为保留内容；不会在随后自动恢复原保留策略。
    /// </summary>
    /// <param name="bindings">待保留的绑定；只修改其中的二维渲染目标，其他目标跳过。调用须在图形主线程执行。</param>
    public static void PreserveBindings(RenderTargetBinding[] bindings)
    {
        foreach (RenderTargetBinding binding in bindings)
        {
            if (binding.RenderTarget is not RenderTarget2D randerTarget)
            {
                continue;
            }

            new RenderTarget2D_Publicizer(randerTarget).RenderTargetUsage = RenderTargetUsage.PreserveContents;
        }
    }

    /// <summary>
    /// 获取当前游戏图形设备的渲染目标绑定，并将其中二维目标设为保留内容。
    /// </summary>
    /// <returns>当前图形设备的渲染目标绑定快照；其中二维目标的 Usage 已设为 PreserveContents。</returns>
    public static RenderTargetBinding[] GetAndPreserveCurrentBindings()
    {
        RenderTargetBinding[] bindings = Main.instance.GraphicsDevice.GetRenderTargets();
        {
            PreserveBindings(bindings);
        }

        return bindings;
    }
}

