// Developed by ColdsUx

using CalamityMod.Events;

namespace CalamityAnomalies.ModCompatibility.CalamityBridge;

internal static class BossRushEvent_Bridge
{
    public static bool BossRushActive
    {
        get => BossRushEvent.BossRushActive;
        set => BossRushEvent.BossRushActive = value;
    }
}
