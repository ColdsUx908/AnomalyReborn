using Terraria.Graphics;
using Terraria.Graphics.Light;
using Terraria.Graphics.Renderers;

namespace Anomalies.Bosses.Skeletron.LegendaryNight.Misc;

public sealed class LegendaryNightLightingSystem : ModSystem
{
    [ThreadStatic]
    private static int _EntityDrawDepth;

    public override void Load()
    {
        if (Main.dedServ)
            return;
        On_LightingEngine.GetColor += GetFullBrightColor;
        On_LegacyLighting.GetColor += GetLegacyFullBrightColor;
        On_Main.DrawNPC += DrawEntity_DrawNPC;
        On_Main.DrawProj += DrawEntity_DrawProj;
        On_Main.DrawItem += DrawEntity_DrawItem;
        On_Main.DrawDust += DrawEntity_DrawDust;
        On_Main.DrawGore += DrawEntity_DrawGore;
        On_LegacyPlayerRenderer.DrawPlayers += DrawEntity_DrawPlayers;
        On_LegacyPlayerRenderer.DrawPlayer += DrawEntity_DrawPlayer;
    }

    // 仅在实体绘制调用内提升照明，淡入系数与天空由蓝转橙共用。
    private static Vector3 GetFullBrightColor(On_LightingEngine.orig_GetColor orig, LightingEngine self, int x, int y) =>
        ApplyEntityLighting(orig(self, x, y));

    private static Vector3 GetLegacyFullBrightColor(On_LegacyLighting.orig_GetColor orig, LegacyLighting self, int x, int y) =>
        ApplyEntityLighting(orig(self, x, y));

    private static Vector3 ApplyEntityLighting(Vector3 originalLight)
    {
        if (_EntityDrawDepth <= 0)
            return originalLight;
        float intensity = LegendaryNightSky.Instance?.EntityLightingIntensity ?? 0f;
        // 已有的高亮通道保持不变，只将较暗通道逐渐补至全亮。
        return Vector3.Lerp(originalLight, Vector3.Max(originalLight, Vector3.One), intensity);
    }

    private readonly struct EntityLightingScope : IDisposable
    {
        public EntityLightingScope() => _EntityDrawDepth++;
        public void Dispose() => _EntityDrawDepth--;
    }

    private static void DrawEntity_DrawNPC(On_Main.orig_DrawNPC orig, Main self, int index, bool behindTiles)
    {
        using EntityLightingScope scope = new();
        orig(self, index, behindTiles);
    }

    private static void DrawEntity_DrawProj(On_Main.orig_DrawProj orig, Main self, int index)
    {
        using EntityLightingScope scope = new();
        orig(self, index);
    }

    private static void DrawEntity_DrawItem(On_Main.orig_DrawItem orig, Main self, Item item, int index)
    {
        using EntityLightingScope scope = new();
        orig(self, item, index);
    }

    private static void DrawEntity_DrawDust(On_Main.orig_DrawDust orig, Main self)
    {
        using EntityLightingScope scope = new();
        orig(self);
    }

    private static void DrawEntity_DrawGore(On_Main.orig_DrawGore orig, Main self)
    {
        using EntityLightingScope scope = new();
        orig(self);
    }

    private static void DrawEntity_DrawPlayers(On_LegacyPlayerRenderer.orig_DrawPlayers orig, LegacyPlayerRenderer self, Camera camera, IEnumerable<Player> players)
    {
        using EntityLightingScope scope = new();
        orig(self, camera, players);
    }

    private static void DrawEntity_DrawPlayer(On_LegacyPlayerRenderer.orig_DrawPlayer orig, LegacyPlayerRenderer self, Camera camera, Player player, Vector2 position, float rotation, Vector2 rotationOrigin, float shadow, float scale)
    {
        using EntityLightingScope scope = new();
        orig(self, camera, player, position, rotation, rotationOrigin, shadow, scale);
    }
}
