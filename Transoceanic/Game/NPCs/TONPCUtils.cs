using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace Transoceanic.Game.NPCs;

/// <summary>
/// 提供与 NPC 相关的便捷工具方法。
/// </summary>
public static class TONPCUtils
{
    private static readonly Lazy<Action<NPC, int, float, float, float>> _VanillaMultiplayerScaling = new(CreateVanillaMultiplayerScaling);

    /// <summary>检查宿主 NPC 缩放方法及 IL 结构并创建隔离委托；框架原生加载时执行，也可离线调用。</summary>
    /// <exception cref="InvalidOperationException">宿主签名、缩放钩子结构或动态方法生成不兼容；消息包含宿主版本和目标。</exception>
    public static void ValidateHostCompatibility() => _ = _VanillaMultiplayerScaling.Value;

    private static Action<NPC, int, float, float, float> CreateVanillaMultiplayerScaling()
    {
        const string TargetName = "ScaleStats_ApplyMultiplayerStats";
        try
        {
            MethodInfo source = typeof(NPC).GetMethod(TargetName, BindingFlags.Instance | BindingFlags.NonPublic,
                null, [typeof(int), typeof(float), typeof(float), typeof(float)], null);
            if (source is null || source.ReturnType != typeof(void) || source.IsGenericMethod)
                throw new MissingMethodException("Expected instance void NPC." + TargetName + "(int, float, float, float).");
            using DynamicMethodDefinition method = new(source);
            RemoveModScalingHook(method.Definition);
            return method.Generate().CreateDelegate<Action<NPC, int, float, float, float>>();
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Transoceanic NPC scaling is incompatible with host {typeof(NPC).Assembly.FullName}; target Terraria.NPC.{TargetName}(Int32, Single, Single, Single). Expected exactly one static void NPCLoader.ApplyDifficultyAndPlayerScaling(NPC, Int32, Single, Single) call. No fallback was applied.", exception);
        }
    }

    // Validate the entire expected call before mutating the copied body. Kept separate for malformed-IL regression tests.
    internal static void RemoveModScalingHook(Mono.Cecil.MethodDefinition method)
    {
        Mono.Collections.Generic.Collection<Instruction> instructions = method.Body.Instructions;
        Instruction[] hooks = instructions.Where(instruction => instruction.Operand is Mono.Cecil.MethodReference reference
            && reference.DeclaringType.FullName == typeof(NPCLoader).FullName
            && reference.Name == nameof(NPCLoader.ApplyDifficultyAndPlayerScaling)).ToArray();
        if (hooks.Length != 1 || hooks[0].OpCode != OpCodes.Call
            || hooks[0].Operand is not Mono.Cecil.MethodReference hook || hook.HasThis
            || hook.ReturnType.FullName != "System.Void"
            || !hook.Parameters.Select(parameter => parameter.ParameterType.FullName).SequenceEqual(
                [typeof(NPC).FullName, "System.Int32", "System.Single", "System.Single"]))
            throw new InvalidOperationException("NPC scaling IL must contain exactly one matching static void scaling hook with four arguments.");
        Instruction call = hooks[0];
        int index = instructions.IndexOf(call);
        call.OpCode = OpCodes.Pop;
        call.Operand = null;
        for (int i = 0; i < 3; i++)
            instructions.Insert(++index, Instruction.Create(OpCodes.Pop));
    }

    /// <summary>
    /// 从当前 tModLoader 程序集的原版逻辑获取单人专家血量倍率，不调用模组钩子。
    /// </summary>
    public static float GetVanillaExpertLifeMultiplier(int npcType)
    {
        // TODO: 升级 tModLoader 时复核专家血量缩放流程；当前倍率按 EnemyMaxLifeMultiplier 与单人原版多人缩放结果相乘计算。
        const int ProbeLife = 1000000;
        NPC probe = new() { type = npcType, lifeMax = ProbeLife };
        _VanillaMultiplayerScaling.Value(probe, 1, 1f, 1f, 1f);
        return GameModeData.ExpertMode.EnemyMaxLifeMultiplier * probe.lifeMax / ProbeLife;
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

internal sealed class NPCScalingCompatibility : ILoadable
{
    public void Load(Mod mod) => TONPCUtils.ValidateHostCompatibility();
    public void Unload() { }
}
