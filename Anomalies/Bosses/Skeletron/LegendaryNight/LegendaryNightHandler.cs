namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>传说之夜共享资源路径、主题颜色和主体查询入口。</summary>
public static class LegendaryNightHandler
{
    /// <summary>传说之夜资源根路径。</summary>
    public const string LegendaryNightPath = AnomalySharedData.ModPath + "Bosses/Skeletron/LegendaryNight/";
    /// <summary>机制七元素弹幕标记。</summary>
    public const float Mechanic7ElementProjectileMarker = -7f;

    /// <summary>本地化分类路径。</summary>
    public const string LocalizationCategory = "Bosses.Skeletron.LegendaryNight";
    /// <summary>骨质颜色。</summary>
    public static readonly Color BoneColor = new(233, 225, 199);
    /// <summary>史莱姆颜色。</summary>
    public static readonly Color SlimeColor = new(105, 140, 255);
    /// <summary>星体颜色。</summary>
    public static readonly Color StarColor = new(255, 215, 0);
    /// <summary>铂金颜色。</summary>
    public static readonly Color PlatinumColor = new(242, 243, 236);

    /// <summary>
    /// 检查 NPC 是否为有效的传说之夜骷髅王头部，排除无效索引返回的哑元。
    /// </summary>
    public static bool IsLegendaryNightHead(NPC npc) => npc is { active: true, type: NPCID.SkeletronHead }
        && npc.whoAmI >= 0 && npc.whoAmI < Main.maxNPCs && npc.Anomaly?.CustomAIType == 1;

    /// <summary>
    /// 按索引检查传说之夜头部；默认使用 0 基索引，oneBased 为 true 时使用 1 基索引。不创建 Behavior。
    /// </summary>
    public static bool IsLegendaryNightHead(int index, bool oneBased = false) => (!oneBased || index > 0)
        && IsLegendaryNightHead(NPC.GetNPCFromIndex(oneBased ? index - 1 : index));

    /// <summary>
    /// 按索引检查并创建一次头部 Behavior，供调用方复用；失败时输出 null。
    /// </summary>
    public static bool TryGetLegendaryNightHead(int index, out SkeletronLegendHead head, bool oneBased = false)
    {
        head = null;
        if (!IsLegendaryNightHead(index, oneBased))
            return false;

        head = SkeletronLegendHead.GetInstance(Main.npc[oneBased ? index - 1 : index]);
        return true;
    }

    /// <summary>取得仍在运行的场景候选，排除退场及未接入当前行为的主体。</summary>
    internal static bool TryGetSceneCandidate(NPC npc, out SkeletronLegendHead head)
    {
        head = null;
        if (!IsLegendaryNightHead(npc))
            return false;
        SkeletronLegendHead candidate = SkeletronLegendHead.GetInstance(npc);
        if (!candidate.ShouldProcess || candidate.CurrentBehavior == SkeletronLegendHead.Behavior.Despawn)
            return false;
        head = candidate;
        return true;
    }

    /// <summary>天空、世界时间和音乐共用的主体选择：时间控制优先，其次二阶段音乐，同优先级按 NPC 槽位顺序。</summary>
    internal static bool TryGetSceneHead(out SkeletronLegendHead head)
    {
        head = null;
        int selectedPriority = -1;
        foreach (NPC npc in NPC.ActiveNPCs)
        {
            if (!TryGetSceneCandidate(npc, out SkeletronLegendHead candidate))
                continue;
            int priority = candidate.SuppressSkyClouds ? 2 : candidate.Phase2MusicActive ? 1 : 0;
            if (priority > selectedPriority)
            {
                head = candidate;
                selectedPriority = priority;
            }
            if (priority == 2)
                return true;
        }
        return head is not null;
    }
    public static void SpawnBlessingBurst(Vector2 position)
    {
        if (Main.dedServ)
            return;
        for (int i = 0; i < 180; i++)
        {
            Vector2 velocity = Main.rand.NextPolarVector2(3f, 20f);
            Color color = Color.Lerp(StarColor, BoneColor, Main.rand.NextFloat());
            ParticleHandler.SpawnParticle(new OrbParticle(position, velocity, Main.rand.Next(30, 70),
                Main.rand.NextFloat(0.5f, 1.5f), color, lifeEndRatio: 0.2f));
        }
    }
}