namespace Transoceanic.Extensions.Terraria;

public static class TOCombatExtensions
{
    public static void HideFromBestiary(this ModNPC npc) => NPCID.Sets.NPCBestiaryDrawOffset.Add(npc.Type, new NPCID.Sets.NPCBestiaryDrawModifiers { Hide = true });
    public static Vector2 ClampMagnitude(this Vector2 vector, float min, float max) => vector.SafeNormalize(Vector2.UnitY) * Math.Clamp(vector.Length(), min, max);
    public static Vector2 RotateDirectionTowards(this Vector2 vector, float angle, float maxChange) => vector.ToRotation().AngleTowards(angle, maxChange).ToRotationVector2();
    public static bool IsTileSolid(this Tile tile) => tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType];
}
