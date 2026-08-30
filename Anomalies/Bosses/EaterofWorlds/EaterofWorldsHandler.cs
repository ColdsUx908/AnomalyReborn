using CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses;
using Terraria.GameContent.ItemDropRules;

namespace Anomalies.Bosses.EaterofWorlds;

public sealed class EaterofWorldsHandler
{
    public static void DestroySegment(NPC npc)
    {
        npc.life = 0;
        npc.HitEffect(0, 10.0);
        npc.checkDead();
    }

    public sealed class DeathEventChange : AnomalyGlobalItemBehavior, IContentLoader
    {
        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            //因世界吞噬怪体节不再掉落物品，故在此处添加掉落物品
            //灾厄已经添加了相同机制，此处不再重复添加
            if (item.type == ItemID.EaterOfWorldsBossBag && !ModReference.Calamity.IsLoaded)
            {
                itemLoot.Add(ItemDropRule.Common(ItemID.DemoniteOre, 1, 70, 90));
                itemLoot.Add(ItemDropRule.Common(ItemID.ShadowScale, 1, 20, 30));
            }
        }

        void IContentLoader.PostSetupContent()
        {
            On_NPC.DropEoWLoot += On_NPC_DropEoWLoot;
            On_NPC.NewNPC += On_NPC_NewNPC;
        }

        private static void On_NPC_DropEoWLoot(On_NPC.orig_DropEoWLoot orig, NPC self, bool fromCheckDead)
        {
            //修复世界吞噬怪死亡时不会掉落物品的问题

            if (!AnomalySharedData.Anomaly || !self.Anomaly.ShouldRunAnomalyAI)
            {
                orig(self, fromCheckDead);
                return;
            }

            self.boss = true;
            self.NPCLoot();
        }

        private int On_NPC_NewNPC(On_NPC.orig_NewNPC orig, IEntitySource source, int X, int Y, int Type, int Start, float ai0, float ai1, float ai2, float ai3, int Target)
        {
            //拦截FTW世界吞噬怪头部死亡时的吞噬者生成

            return orig(source, X, Y, Type, Start, ai0, ai1, ai2, ai3, Target);

            //TODO: 在1.4.5tml中这段代码应该可以工作。现版本的生成源用的是NaturalSpawn，无法追踪
            /*
            if (!AnomalySharedData.Anomaly || !Main.getGoodWorld)
                return orig(source, X, Y, Type, Start, ai0, ai1, ai2, ai3, Target);

            if (source is not EntitySource_Parent parentSource
                || parentSource.Entity is not NPC npc
                || npc.type != NPCID.EaterofWorldsHead
                || X != (int)npc.Center.X
                || Y != (int)(npc.position.Y + npc.height)
                || Type != NPCID.BigEater)
            {
                return orig(source, X, Y, Type, Start, ai0, ai1, ai2, ai3, Target);
            }

            return Main.maxNPCs;
            */
        }
    }

    [ExtendsFromMod(CalamityModName)]
    public sealed class CalamitySupport : IContentLoader
    {
        void IContentLoader.PostSetupContent()
        {
            EaterofWorldsHead.OnRunningPreAI += BreakEaterofWorldsDR;
            EaterofWorldsBody.OnRunningPreAI += BreakEaterofWorldsDR;
            EaterofWorldsTail.OnRunningPreAI += BreakEaterofWorldsDR;
        }

        private static void BreakEaterofWorldsDR(AnomalyNPCBehavior behavior)
        {
            //将newAI[1]设置为DRIncreaseTime，以解除灾厄世界吞噬怪的出生DR机制
            behavior.NPC.CalamityNPC.newAI[1] = EaterOfWorldsAI.DRIncreaseTime;
        }
    }
}