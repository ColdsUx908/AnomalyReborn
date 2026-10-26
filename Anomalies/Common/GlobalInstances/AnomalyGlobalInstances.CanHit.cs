namespace Anomalies.Common;

// CanHit 系列统一入口。SingleBehavior 仅提供业务钩子，不在通用 Handler 重复注册。
// 深度限制拥有否决权；通过后再查询该方法实际存在且 ShouldProcess 的行为。
public sealed partial class AnomalyPlayer
{
    // 同时覆盖荆棘、冲撞等经由 Player.ApplyDamageToNPC 结算的特殊攻击。
    public override bool CanHitNPC(NPC target) => AnomalyDepthDrawSystem.IsVisible(target.Anomaly.Z);
}

public sealed partial class AnomalyGlobalNPC
{
    public override bool? CanBeHitByItem(NPC npc, Player player, Item item)
    {
        if (!AnomalyDepthDrawSystem.IsVisible(Z))
            return false;
        return npc.TryGetBehavior(out AnomalyNPCBehavior behavior) ? behavior.CanBeHitByItem(player, item) : null;
    }

    public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
    {
        if (!AnomalyDepthDrawSystem.IsVisible(Z))
            return false;
        return npc.TryGetBehavior(out AnomalyNPCBehavior behavior) ? behavior.CanBeHitByProjectile(projectile) : null;
    }

    public override bool CanBeHitByNPC(NPC npc, NPC attacker) =>
        Z == 0f && (!npc.TryGetBehavior(out AnomalyNPCBehavior behavior) || behavior.CanBeHitByNPC(attacker));

    public override bool? CanCollideWithPlayerMeleeAttack(NPC npc, Player player, Item item, Rectangle meleeAttackHitbox) =>
        npc.TryGetBehavior(out AnomalyNPCBehavior behavior) ? behavior.CanCollideWithPlayerMeleeAttack(player, item, meleeAttackHitbox) : null;

    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) =>
        !npc.TryGetBehavior(out AnomalyNPCBehavior behavior) || behavior.CanHitPlayer(target, ref cooldownSlot);

    public override bool CanHitNPC(NPC npc, NPC target) =>
        !npc.TryGetBehavior(out AnomalyNPCBehavior behavior) || behavior.CanHitNPC(target);
}

public sealed partial class AnomalyGlobalProjectile
{
    public override bool? CanHitNPC(Projectile projectile, NPC target) =>
        projectile.TryGetBehavior(out AnomalyProjectileBehavior behavior) ? behavior.CanHitNPC(target) : null;

    public override bool CanHitPvp(Projectile projectile, Player target) =>
        !projectile.TryGetBehavior(out AnomalyProjectileBehavior behavior) || behavior.CanHitPvp(target);

    public override bool CanHitPlayer(Projectile projectile, Player target) =>
        !projectile.TryGetBehavior(out AnomalyProjectileBehavior behavior) || behavior.CanHitPlayer(target);
}

public sealed partial class AnomalyGlobalItem
{
    public override bool? CanHitNPC(Item item, Player player, NPC target) =>
        item.TryGetBehavior(out AnomalyItemBehavior behavior) ? behavior.CanHitNPC(player, target) : null;

    public override bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, Player player, NPC target) =>
        item.TryGetBehavior(out AnomalyItemBehavior behavior) ? behavior.CanMeleeAttackCollideWithNPC(meleeAttackHitbox, player, target) : null;

    public override bool CanHitPvp(Item item, Player player, Player target) =>
        !item.TryGetBehavior(out AnomalyItemBehavior behavior) || behavior.CanHitPvp(player, target);
}
