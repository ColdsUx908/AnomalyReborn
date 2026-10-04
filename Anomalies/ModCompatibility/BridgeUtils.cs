using System.ComponentModel;
using CalamityMod;

namespace Anomalies.ModCompatibility;

public static class BridgeUtils
{
    public static event Action<NPC> OnApplyCalamityHealthBoost;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ApplyCalamityHealthBoost(NPC npc) => OnApplyCalamityHealthBoost?.Invoke(npc);

    [ExtendsFromMod(CalamityModName)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class CalamitySupport : IContentLoader
    {
        void IContentLoader.PostSetupContent()
        {
            OnApplyCalamityHealthBoost += npc => npc.lifeMax = (int)(npc.lifeMax * (1f + CalamityServerConfig.Instance.BossHealthBoost * 0.01f));
        }
    }
}
