// Developed by ColdsUx

using System.ComponentModel;
using CalamityMod;
using static Anomalies.ModCompatibility.BridgeUtils;

namespace Anomalies.ModCompatibility;

public static class BridgeUtils
{
    public static event Action<NPC> OnApplyCalamityHealthBoost;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ApplyCalamityHealthBoost(NPC npc) => OnApplyCalamityHealthBoost?.Invoke(npc);
}

[ExtendsFromMod(CalamityModName)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class BridgeUtils_Calamity : IContentLoader
{
    void IContentLoader.PostSetupContent()
    {
        OnApplyCalamityHealthBoost += npc => npc.lifeMax = (int)(npc.lifeMax * (1f + CalamityServerConfig.Instance.BossHealthBoost * 0.01f));
    }
}