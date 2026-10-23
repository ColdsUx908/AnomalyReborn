using Anomalies.Bosses.BrainofCthulhu;

namespace Anomalies.Visuals.BossBar;

public partial class BossHealthBar
{
    static partial void FilterBrainExtension(NPC npc, ref bool include)
    {
        if (npc.type == NPCID.Creeper && npc.TryGetCreeperBehavior(out CreeperAI ai) && ai.IsPhantom)
            include = false;
    }

    static partial void AdjustBrainLifeContribution(NPC npc, ref int life)
    {
        if (npc.type == NPCID.Creeper && npc.TryGetCreeperBehavior(out CreeperAI ai))
            life = ai.CombinedLifeContribution;
    }

    static partial void RegisterBrainLifeFunctions()
    {
        // 飞眼怪死亡仅扣除当前血量；最大总血量在幻影清除后仍保持战斗期间的峰值。
        LifeFunctions.Add(b =>
        {
            if (b.NPC.type != NPCID.BrainofCthulhu || !b.NPC.TryGetBrainBehavior(out _))
                return false;
            long life = b.NPC.life;
            long maxLife = b.NPC.lifeMax;
            foreach ((long identifier, NPC creeper) in b.CustomOneToMany)
            {
                if (creeper.Identifier != identifier || !creeper.active || creeper.lifeMax <= 0)
                    continue;
                maxLife += creeper.lifeMax;
                life += GetLifeContribution(creeper);
            }
            b.CombinedNPCLife = life;
            b.InitialMaxLife = Math.Max(b.InitialMaxLife, maxLife);
            b.CombinedNPCMaxLife = b.InitialMaxLife;
            return true;
        });

    }
}
