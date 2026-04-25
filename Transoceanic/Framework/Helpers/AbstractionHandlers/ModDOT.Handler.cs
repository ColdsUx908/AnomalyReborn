// Designed by ColdsUx

namespace Transoceanic.Framework.Helpers.AbstractionHandlers;

/// <summary>
/// 自定义持续伤害（DOT）系统的全局处理器。
/// <para>负责收集所有 <see cref="ModDOT"/> 实例以及外部注册的 DOT 逻辑，并在玩家与 NPC 更新时应用伤害。</para>
/// </summary>
public sealed class ModDOTHandler : IContentLoader
{
    internal static readonly List<ModDOT> ModDOTSet = [];
    internal static readonly Dictionary<int, (Func<Player, bool> hasBuffPlayer, Func<NPC, bool> hasBuffNPC, Func<Player, float> damagePlayer, Func<NPC, float>, Func<NPC, int> damageValue)> ExternalDOTSet = [];

    /// <summary>
    /// 根据 Buff 类型 ID 注册一个外部 DOT 逻辑。
    /// </summary>
    /// <param name="type">buff 类型 ID。</param>
    /// <param name="hasBuffPlayer">判断玩家是否拥有此 buff 的方法。</param>
    /// <param name="hasBuffNPC">判断 NPC 是否拥有此 buff 的方法。</param>
    /// <param name="damagePlayer">计算对玩家造成的 DOT 伤害的方法。</param>
    /// <param name="damageNPC">计算对 NPC 造成的 DOT 伤害的方法。</param>
    /// <param name="damageValue">计算 NPC 受到的伤害值的方法。</param>
    /// <param name="cover">若已存在相同 ID 的注册，是否覆盖。</param>
    public static void RegisterDOT(int type, Func<Player, bool> hasBuffPlayer, Func<NPC, bool> hasBuffNPC, Func<Player, float> damagePlayer = null, Func<NPC, float> damageNPC = null, Func<NPC, int> damageValue = null, bool cover = true)
    {
        hasBuffPlayer ??= _ => false;
        hasBuffNPC ??= _ => false;
        damagePlayer ??= _ => 0f;
        damageNPC ??= _ => 0f;
        damageValue ??= _ => 0;
        if (cover || !ExternalDOTSet.ContainsKey(type))
            ExternalDOTSet[type] = (hasBuffPlayer, hasBuffNPC, damagePlayer, damageNPC, damageValue);
    }

    /// <summary>
    /// 通过 <see cref="ModBuff"/> 类型泛型注册一个外部 DOT 逻辑。
    /// </summary>
    /// <typeparam name="T">继承自 <see cref="ModBuff"/> 的类型。</typeparam>
    /// <inheritdoc cref="RegisterDOT(int, Func{Player, bool}, Func{NPC, bool}, Func{Player, float}, Func{NPC, float}, Func{NPC, int}, bool)"/>
    public static void RegisterDOT<T>(Func<Player, bool> hasBuffPlayer, Func<NPC, bool> hasBuffNPC, Func<Player, float> damagePlayer = null, Func<NPC, float> damageNPC = null, Func<NPC, int> damageValue = null, bool cover = true) where T : ModBuff =>
        RegisterDOT(ModContent.BuffType<T>(), hasBuffPlayer, hasBuffNPC, damagePlayer, damageNPC, damageValue, cover);

    /// <summary>
    /// 内容设置后阶段：收集所有继承自 <see cref="ModDOT"/> 的类型实例。
    /// </summary>
    void IContentLoader.PostSetupContent() => ModDOTSet.AddRange(TOReflectionUtils.GetTypeInstancesDerivedFrom<ModDOT>());

    /// <summary>
    /// 模组卸载时清理所有缓存的 DOT 数据。
    /// </summary>
    void IContentLoader.OnModUnload()
    {
        ModDOTSet.Clear();
        ExternalDOTSet.Clear();
    }
}

/// <summary>
/// 玩家行为扩展：在玩家生命回复更新时，计算并应用所有注册的 DOT 伤害。
/// </summary>
public sealed class ModDOT_Player : TOPlayerBehavior
{
    /// <summary>
    /// 更新玩家的不良生命回复值，将 DOT 伤害累加并减少生命回复。
    /// </summary>
    public override void UpdateBadLifeRegen()
    {
        float totalDOT = 0f;
        foreach (ModDOT dot in ModDOTHandler.ModDOTSet)
        {
            if (dot.HasBuff(Player))
                totalDOT += dot.GetDamage(Player);
        }
        foreach ((Func<Player, bool> hasBuffPlayer, _, Func<Player, float> damagePlayer, _, _) in ModDOTHandler.ExternalDOTSet.Values)
        {
            if (hasBuffPlayer(Player))
                totalDOT += damagePlayer(Player);
        }

        Player.lifeRegen -= (int)totalDOT;
    }
}

/// <summary>
/// 全局 NPC 行为扩展：在 NPC 生命回复更新时，计算并应用所有注册的 DOT 伤害。
/// </summary>
public sealed class ModDOT_GlobalNPC : TOGlobalNPCBehavior
{
    /// <summary>
    /// 更新 NPC 的生命回复，叠加 DOT 伤害。
    /// </summary>
    /// <param name="npc">受影响的 NPC。</param>
    /// <param name="damage">引用的伤害值，用于传递最终伤害。</param>
    public override void UpdateLifeRegen(NPC npc, ref int damage)
    {
        float totalDOT = 0f;
        int finalDamageValue = 0;
        foreach (ModDOT dot in ModDOTHandler.ModDOTSet)
        {
            if (dot.HasBuff(npc))
                ApplyDOT(dot.GetDamage(npc), dot.GetDamageValue(npc));
        }
        foreach ((_, Func<NPC, bool> hasBuffNPC, _, Func<NPC, float> damageNPC, Func<NPC, int> damageValue) in ModDOTHandler.ExternalDOTSet.Values)
        {
            if (hasBuffNPC(npc))
                ApplyDOT(damageNPC(npc), damageValue(npc));
        }
        npc.ApplyDOT((int)totalDOT, finalDamageValue, ref damage);

        void ApplyDOT(float dot, int damageValue)
        {
            totalDOT += dot * 2f;
            finalDamageValue = Math.Max(finalDamageValue, damageValue);
        }
    }
}