// Developed by ColdsUx

using CalamityMod.World;

namespace CalamityAnomalies.ModCompatibility.CalamityBridge;

/// <inheritdoc cref="CalamityWorld"/>
internal static class CalamityWorld_Bridge
{
    /// <inheritdoc cref="CalamityWorld.LegendaryMode"/>
    public static bool LegendaryMode => CalamityWorld.LegendaryMode;
}
