using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace Transoceanic.Framework.Helpers;

/// <summary>
/// 提供与 NPC 相关的便捷工具方法。
/// </summary>
public static class TONPCUtils
{
    private static readonly Lazy<Action<NPC, int, float, float, float>> VanillaMultiplayerScaling = new(CreateVanillaMultiplayerScaling);

    private static Action<NPC, int, float, float, float> CreateVanillaMultiplayerScaling()
    {
        // TODO: 升级 tModLoader 时复核 ScaleStats_ApplyMultiplayerStats 的名称、签名及 IL；当前按 NPCLoader 缩放钩子的四个参数移除调用。
        using DynamicMethodDefinition method = new(PublicizerHandler.GetInstanceMethod(typeof(NPC), "ScaleStats_ApplyMultiplayerStats"));
        // 仅运行程序集中的原版逻辑，避免探测倍率时递归调用模组缩放钩子。
        Mono.Collections.Generic.Collection<Instruction> instructions = method.Definition.Body.Instructions;
        Instruction hook = instructions.Single(i => i.OpCode == OpCodes.Call
            && i.Operand is Mono.Cecil.MethodReference m && m.DeclaringType.FullName == typeof(NPCLoader).FullName
            && m.Name == nameof(NPCLoader.ApplyDifficultyAndPlayerScaling));
        int index = instructions.IndexOf(hook);
        hook.OpCode = OpCodes.Pop;
        hook.Operand = null;
        for (int i = 0; i < 3; i++)
            instructions.Insert(++index, Instruction.Create(OpCodes.Pop));
        return method.Generate().CreateDelegate<Action<NPC, int, float, float, float>>();
    }

    /// <summary>
    /// 从当前 tModLoader 程序集的原版逻辑获取单人专家血量倍率，不调用模组钩子。
    /// </summary>
    public static float GetVanillaExpertLifeMultiplier(int npcType)
    {
        // TODO: 升级 tModLoader 时复核专家血量缩放流程；当前倍率按 EnemyMaxLifeMultiplier 与单人原版多人缩放结果相乘计算。
        const int ProbeLife = 1000000;
        NPC probe = new() { type = npcType, lifeMax = ProbeLife };
        VanillaMultiplayerScaling.Value(probe, 1, 1f, 1f, 1f);
        return GameModeData.ExpertMode.EnemyMaxLifeMultiplier * probe.lifeMax / ProbeLife;
    }

    /// <summary>
    /// 在异象 Boss 的 ApplyDifficultyAndPlayerScaling 中撤销单人专家倍率、大师的 1.5 倍血量
    /// 和 bossAdjustment，保留多人 balance，并将血量四舍五入到十的倍数。
    /// </summary>
    public static void ApplyAnomalyBossDifficultyAndPlayerScaling(NPC npc, float bossAdjustment)
    {
        // TODO: 升级 tModLoader 时复核缩放钩子的执行时机、大师相对专家的 1.5 倍血量及 bossAdjustment 的应用方式，再调整抵消公式。
        float vanillaExpertLifeMultiplier = GetVanillaExpertLifeMultiplier(npc.type);
        npc.lifeMax = (int)MathF.Round(npc.lifeMax / (vanillaExpertLifeMultiplier * 1.5f) / bossAdjustment / 10f) * 10;
    }

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
