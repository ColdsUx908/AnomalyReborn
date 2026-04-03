using CalamityMod.CalPlayer;
using CalamityMod.Graphics;

namespace CalamityAnomalies.ModCompatibility.CalamityBridge;

internal static class EnhancedDarknessSystem_Bridge
{
    public static void AddLightSource(Vector2? center = null, Texture2D texture = null, float scale = 1f, float rotation = 0, Vector2? vectorScale = null, float opacity = 1) =>
        EnhancedDarknessSystem.lights.Add(new EnhancedDarknessSystem.LightSource(center, texture, scale, rotation, vectorScale, opacity));

    public static void ChangeDarknessIntensity(Player player, Func<float, float> intensityModifier)
    {
        if (intensityModifier is null)
            return;
        CalamityPlayer calamityPlayer = player.Calamity;
        calamityPlayer.darknessIntensity = Math.Clamp(intensityModifier(calamityPlayer.darknessIntensity), 0f, 1f);
    }
}
