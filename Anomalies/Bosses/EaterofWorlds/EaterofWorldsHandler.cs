using CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses;
using Terraria.GameContent.ItemDropRules;

namespace Anomalies.Bosses.EaterofWorlds;

public sealed class EaterofWorldsHandler
{
    public static IEaterofWorldsSegment TryGetSegment(NPC npc) => npc.type switch
    {
        NPCID.EaterofWorldsHead => EaterofWorldsHead.GetInstance(npc),
        NPCID.EaterofWorldsBody => EaterofWorldsBody.GetInstance(npc),
        NPCID.EaterofWorldsTail => EaterofWorldsTail.GetInstance(npc),
        _ => null
    };

    public static void DestroySegment(NPC npc)
    {
        npc.life = 0;
        npc.HitEffect(0, 9999, true);
        npc.checkDead();
    }

    public sealed class DefaultsChange : AnomalyGlobalItemBehavior
    {
        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            //因世界吞噬怪体节不再掉落物品，故在此处添加掉落物品
            //灾厄已经添加了相同机制，此处不再重复添加
            if (item.type == ItemID.EaterOfWorldsBossBag && !AnomalyModReferences.Calamity.IsLoaded)
            {
                itemLoot.Add(ItemDropRule.Common(ItemID.DemoniteOre, 1, 70, 90));
                itemLoot.Add(ItemDropRule.Common(ItemID.ShadowScale, 1, 20, 30));
            }
        }

        /// <summary>在行为内容加载时安装生成和掉落钩子，不提前修改 NPC 静态数组。</summary>
        public override void Load()
        {
            On_NPC.DropEoWLoot += On_NPC_DropEoWLoot;
            On_NPC.NewNPC += On_NPC_NewNPC;
        }

        /// <summary>在 tML 完成内容数组扩容后设置原版吞噬怪的残影表，避免加载后被重置。</summary>
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            NPCID.Sets.TrailingMode[NPCID.EaterofWorldsHead] = 1;
            NPCID.Sets.TrailingMode[NPCID.EaterofWorldsBody] = 1;
            NPCID.Sets.TrailingMode[NPCID.EaterofWorldsTail] = 1;
            NPCID.Sets.TrailCacheLength[NPCID.EaterofWorldsHead] = 16;
            NPCID.Sets.TrailCacheLength[NPCID.EaterofWorldsBody] = 16;
            NPCID.Sets.TrailCacheLength[NPCID.EaterofWorldsTail] = 16;
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
            //TODO: 在1.4.5tml中这段代码应该可以工作。现版本的生成源用的是NaturalSpawn，无法追踪

            if (!AnomalySharedData.Anomaly || !Main.getGoodWorld)
                return orig(source, X, Y, Type, Start, ai0, ai1, ai2, ai3, Target);

            if (source is not EntitySource_Parent parentSource
                || parentSource.Entity is not NPC npc
                || npc.type != NPCID.EaterofWorldsHead
                || Math.Abs(X - (int)npc.Center.X) <= 5 //引入容差，防止因坐标偏差导致的误判
                || Math.Abs(Y - (int)(npc.position.Y + npc.height)) <= 5
                || Type != NPCID.BigEater)
            {
                return orig(source, X, Y, Type, Start, ai0, ai1, ai2, ai3, Target);
            }

            return Main.maxNPCs;
        }
    }

    [ExtendsFromMod(CalamityModName)]
    public sealed class CalamitySupport : ILoadable
    {
        /// <summary>在 tML 内容加载时订阅运行期减伤兼容，不立即读取实体状态。</summary>
        /// <param name="mod">由 tML 提供的所属模组。</param>
        void ILoadable.Load(Mod mod)
        {
            // 这些是本模块自定义事件，不属于 tML 自动卸载的 On/IL 钩子。
            TOCleanup.Register(() =>
            {
                EaterofWorldsHead.OnRunningPreAI -= BreakEaterofWorldsDR;
                EaterofWorldsBody.OnRunningPreAI -= BreakEaterofWorldsDR;
                EaterofWorldsTail.OnRunningPreAI -= BreakEaterofWorldsDR;
            });
            EaterofWorldsHead.OnRunningPreAI += BreakEaterofWorldsDR;
            EaterofWorldsBody.OnRunningPreAI += BreakEaterofWorldsDR;
            EaterofWorldsTail.OnRunningPreAI += BreakEaterofWorldsDR;
        }

        // 自定义事件订阅由 TOCleanup 清理，不增加手动 On/IL 钩子卸载。
        void ILoadable.Unload() { }

        private static void BreakEaterofWorldsDR(AnomalyNPCBehavior behavior)
        {
            //将newAI[1]设置为DRIncreaseTime，以解除灾厄世界吞噬怪的出生DR机制
            behavior.NPC.CalamityNPC.newAI[1] = EaterOfWorldsAI.DRIncreaseTime;
        }
    }
}
