namespace Transoceanic.Framework.Helpers;

/// <summary>
/// 提供与 NPC 相关的便捷工具方法。
/// </summary>
public static class TONPCUtils
{
    /// <summary>
    /// 判断当前 NPC 是否处于单独击败机械双子魔眼（The Twins）中某一只的状态（即另一只已死亡）。
    /// </summary>
    /// <param name="npc">要判断的 NPC 实例，通常应为 <see cref="NPCID.Retinazer"/> 或 <see cref="NPCID.Spazmatism"/>。</param>
    /// <returns>若另一只双子魔眼已不存在于世界中，返回 <see langword="true"/>；否则返回 <see langword="false"/>。若传入的 NPC 并非双子魔眼，返回 <see langword="false"/>。</returns>
    public static bool IsDefeatingTwins(NPC npc) => npc.type switch
    {
        NPCID.Retinazer => !NPC.ActiveNPCs.Any(n => n.type == NPCID.Spazmatism),
        NPCID.Spazmatism => !NPC.ActiveNPCs.Any(n => n.type == NPCID.Retinazer),
        _ => false
    };

    /// <summary>
    /// 获取一个值，指示机械三王（毁灭者、双子魔眼、机械骷髅王）是否都已被击败。
    /// </summary>
    public static bool DownedMechBossAll => NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3;

    /// <summary>
    /// 以更不易获取到空索引的方式获取 Boss 头部纹理索引。
    /// <br/>此方法会优先返回 <see cref="NPCID.Sets.BossHeadTextures"/> 中的索引，如果该索引存在且 <see cref="NPC.GetBossHeadTextureIndex"/> 返回 -1，则使用前者；否则使用后者。
    /// </summary>
    /// <param name="npc">要获取头部纹理索引的 NPC 实例。</param>
    /// <returns>返回 Boss 头部纹理的索引。</returns>
    public static int GetBossHeadTextureIndexBetter(NPC npc)
    {
        int result = NPCID.Sets.BossHeadTextures[npc.type];
        int result2 = npc.GetBossHeadTextureIndex();

        if (result != -1 && result2 == -1)
            return result;
        return result2;
    }
}
