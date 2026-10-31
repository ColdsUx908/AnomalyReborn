using System.ComponentModel;
using CalamityMod;

namespace Anomalies.ModCompatibility;

public static partial class BridgeUtils
{
    public static event Action<NPC> OnApplyCalamityHealthBoost;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ApplyCalamityHealthBoost(NPC npc) => OnApplyCalamityHealthBoost?.Invoke(npc);

    [ExtendsFromMod(CalamityModName)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class CalamitySupport : ILoadable
    {
        /// <summary>在所属模组内容加载时订阅生命增幅；配置在实际调用时读取。</summary>
        /// <param name="mod">由 tML 提供的所属模组。</param>
        void ILoadable.Load(Mod mod)
        {
            TOCleanup.Register(() => OnApplyCalamityHealthBoost = null);
            OnApplyCalamityHealthBoost += npc => npc.lifeMax = (int)(npc.lifeMax * (1f + CalamityServerConfig.Instance.BossHealthBoost * 0.01f));
        }

        // 自有事件由 TOCleanup 清理，tML 要求实现此生命周期入口。
        void ILoadable.Unload() { }
    }
}
