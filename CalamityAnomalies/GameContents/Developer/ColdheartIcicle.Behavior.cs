// Developed by ColdsUx

namespace CalamityAnomalies.GameContents.Developer;

public sealed class ColdheartIcicle_Player : CAPlayerBehavior
{
    public override bool ShouldProcess => false;

    public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
    {
        if (!mediumCoreDeath && ColdheartIcicle.IsLegendOwner(Player))
            yield return Item.CreateItem<ColdheartIcicle>();
    }
}

public sealed class ColdheartIcicle_GlobalNPC : CAGlobalNPCBehavior
{
    public override void OnKill(NPC npc)
    {
        foreach (Projectile p in Projectile.ActiveProjectiles)
        {
            if (p.ModProjectile is ColdheartIcicleDream dream && dream.Target == npc)
                dream.Timer2++;
        }
    }

    public override void ModifyGlobalLoot(GlobalLoot globalLoot)
    {
        //globalLoot.Add(ItemDropRule.ByCustomCondition(i => !i.IsInSimulation && i.npc.value > 0f && i.player.ZoneSnow && !Main.snowMoon, null, null, ModContent.ItemType<ColdheartIcicle>(), 400000));
        //globalLoot.Add(ItemDropRule.ByCustomCondition(i => !i.IsInSimulation && i.npc.FrostMoonEnemy && Main.snowMoon, null, null, ModContent.ItemType<ColdheartIcicle>(), 20000));
    }
}