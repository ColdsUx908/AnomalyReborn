namespace Transoceanic.Behaviors;

/// <summary>
/// 面向全部 NPC 的行为基类，由全局 NPC 调度器筛选并调用。
/// </summary>
public abstract partial class GlobalNPCBehavior : GlobalEntityBehavior<NPC>
{
    /// <inheritdoc cref="GlobalNPC.CanFallThroughPlatforms"/>
    public virtual bool? CanFallThroughPlatforms(NPC npc) => null;

    /// <inheritdoc cref="GlobalNPC.CanBeCaughtBy"/>
    public virtual bool? CanBeCaughtBy(NPC npc, Item item, Player player) => null;

    /// <inheritdoc cref="GlobalNPC.ModifyGlobalLoot"/>
    public virtual void ModifyGlobalLoot(GlobalLoot globalLoot) { }

    /// <inheritdoc cref="GlobalNPC.EditSpawnRate"/>
    public virtual void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) { }

    /// <inheritdoc cref="GlobalNPC.EditSpawnRange"/>
    public virtual void EditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY, ref int safeRangeX, ref int safeRangeY) { }

    /// <inheritdoc cref="GlobalNPC.EditSpawnPool"/>
    public virtual void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) { }

    /// <inheritdoc cref="GlobalNPC.SpawnNPC"/>
    public virtual void SpawnNPC(int npc, int tileX, int tileY) { }

    /// <inheritdoc cref="GlobalNPC.CanChat"/>
    public virtual bool? CanChat(NPC npc) => null;

    /// <inheritdoc cref="GlobalNPC.ModifyShop"/>
    public virtual void ModifyShop(NPCShop shop) { }

    /// <inheritdoc cref="GlobalNPC.SetupTravelShop"/>
    public virtual void SetupTravelShop(int[] shop, ref int nextSlot) { }

    /// <inheritdoc cref="GlobalNPC.CanGoToStatue"/>
    public virtual bool? CanGoToStatue(NPC npc, bool toKingStatue) => null;

    /// <inheritdoc cref="GlobalNPC.BuffTownNPC"/>
    public virtual void BuffTownNPC(ref float damageMult, ref int defense) { }
}
