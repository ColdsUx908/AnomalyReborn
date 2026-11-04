namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed class LegendaryNightHandler
{
    public const string LegendaryNightPath = AnomalySharedData.ModPath + "Bosses/Skeletron/LegendaryNight/";
    public const string LegendaryNightEntityPath = AnomalySharedData.ModPath + "Bosses/Skeletron/LegendaryNight/Entities/";
    public const string LocalizationCategory = "Bosses.Skeletron.LegendaryNight";
    public static readonly Color BoneColor = new(233, 225, 199);
    public static readonly Color SlimeColor = new(105, 140, 255);
    public static readonly Color StarColor = new(255, 215, 0);
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
    public static bool IsLegendaryNightHead(int index, out SkeletronLegendHead head, bool oneBased = false)
    {
        head = null;
        if (!IsLegendaryNightHead(index, oneBased))
            return false;

        head = SkeletronLegendHead.GetInstance(Main.npc[oneBased ? index - 1 : index]);
        return true;
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
