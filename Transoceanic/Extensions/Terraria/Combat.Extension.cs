namespace Transoceanic.Extensions.Terraria;

/// <summary>
/// 提供战斗逻辑使用的实体、向量与物块扩展。
/// </summary>
public static class TOCombatExtensions
{
    /// <summary>
    /// 将该 ModNPC 类型在图鉴绘制配置中设为隐藏，应在静态配置阶段调用。
    /// </summary>
    public static void HideFromBestiary(this ModNPC npc) => NPCID.Sets.NPCBestiaryDrawOffset.Add(npc.Type, new NPCID.Sets.NPCBestiaryDrawModifiers { Hide = true });
    /// <summary>
    /// 保留向量方向并限制长度在 min 与 max 之间；零向量以向下方向代替，边界无效时由 Math.Clamp 抛出异常。
    /// </summary>
    public static Vector2 ClampMagnitude(this Vector2 vector, float min, float max) => vector.SafeNormalize(Vector2.UnitY) * Math.Clamp(vector.Length(), min, max);
    /// <summary>
    /// 将方向向目标角度旋转至多 maxChange 弧度，返回单位方向向量，不保留原长度。
    /// </summary>
    public static Vector2 RotateDirectionTowards(this Vector2 vector, float angle, float maxChange) => vector.ToRotation().AngleTowards(angle, maxChange).ToRotationVector2();
    /// <summary>
    /// 判断物块是否存在、未被致动且属于实心非平台类型。
    /// </summary>
    public static bool IsTileSolid(this Tile tile) => tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType];
}
