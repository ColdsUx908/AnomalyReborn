namespace Anomalies.Common;

// Behavior 禁用的高频 Can 回调集中于此；每种实体的同名回调只保留一个 GlobalType/ModPlayer 实现。
// 新机制在现有回调中合并条件，不另建全局类型重复声明，也不添加无业务逻辑的默认重写。
public sealed partial class AnomalyPlayer
{
    // 同时覆盖荆棘、冲撞等经由 Player.ApplyDamageToNPC 结算的特殊攻击。
    public override bool CanHitNPC(NPC target) => AnomalyDepthDrawSystem.IsVisible(target.Anomaly.Z);
}

public sealed partial class AnomalyGlobalNPC
{
    public override bool? CanBeHitByItem(NPC npc, Player player, Item item) => AnomalyDepthDrawSystem.IsVisible(Z) ? null : false;

    public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile) => AnomalyDepthDrawSystem.IsVisible(Z) ? null : false;

    public override bool CanBeHitByNPC(NPC npc, NPC attacker) => Z == 0f;
}
