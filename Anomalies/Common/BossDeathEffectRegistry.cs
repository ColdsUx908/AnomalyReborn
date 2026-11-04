#if ANOMALY_MODE
namespace Anomalies.Common;

/// <summary>集中拦截被 Boss 锁血否决的死亡特效；条件由各 Boss 模块注册。</summary>
public sealed class BossDeathEffectRegistry : ModSystem
{
    private static readonly Dictionary<int, List<Func<NPC, bool>>> _Conditions = [];

    private static readonly List<Action> _PendingRegistrations = [];

    /// <summary>
    /// 在行为类 Load 中注册。条件必须无副作用，并包含 Behavior 的 ShouldProcess 判定。
    /// 返回 true 表示此次死亡将被否决；不要在条件中调用 CheckDead 或修改实体状态。
    /// 同一类型的条件按注册顺序以 OR 合并，不保存逐实体数据。
    /// </summary>
    public static void Register(int npcType, Func<NPC, bool> condition)
    {
        ArgumentNullException.ThrowIfNull(condition);
        if (!_Conditions.TryGetValue(npcType, out List<Func<NPC, bool>> conditions))
            _Conditions.Add(npcType, conditions = []);
        conditions.Add(condition);
    }

    /// <summary>在 Load 中按 ModNPC 类型注册，待内容加载完成后解析 NPC 类型 ID。</summary>
    public static void Register<TModNPC>(Func<TModNPC, bool> condition) where TModNPC : ModNPC
    {
        ArgumentNullException.ThrowIfNull(condition);
        _PendingRegistrations.Add(() => Register(ModContent.NPCType<TModNPC>(),
            npc => npc.ModNPC is TModNPC modNPC && condition(modNPC)));
    }

    public override void PostSetupContent()
    {
        foreach (Action register in _PendingRegistrations)
            register();
        _PendingRegistrations.Clear();
    }

    public override void Load() => On_NPC.HitEffect_HitInfo += SuppressLockedDeathEffect;

    // 钩子交由 tML 卸载；这里只清理注册委托，避免重新加载时残留模块引用。
    public override void Unload()
    {
        _Conditions.Clear();
        _PendingRegistrations.Clear();
    }

    private static void SuppressLockedDeathEffect(On_NPC.orig_HitEffect_HitInfo orig, NPC self, NPC.HitInfo hit)
    {
        // 原版先执行 HitEffect 再调用 CheckDead，锁血恢复生命时死亡 Gore 已经生成。
        // 仅跳过被否决的致死特效；实际锁血、转阶段及同步仍由原 CheckDead 处理。
        if (self.life <= 0 && _Conditions.TryGetValue(self.type, out List<Func<NPC, bool>> conditions))
        {
            foreach (Func<NPC, bool> condition in conditions)
            {
                if (condition(self))
                    return;
            }
        }

        orig(self, hit);
    }
}
#endif
