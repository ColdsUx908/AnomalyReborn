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
        if (!npc.TryGetBehavior(out AnomalyNPCBehavior behavior))
            return null;
        using EntityBehavior<NPC>.EntityLock entityLock = behavior.LockEntity();
        return behavior.CanBeHitByItem(player, item);
    }

    public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
    {
        if (!AnomalyDepthDrawSystem.IsVisible(Z))
            return false;
        if (!npc.TryGetBehavior(out AnomalyNPCBehavior behavior))
            return null;
        using EntityBehavior<NPC>.EntityLock entityLock = behavior.LockEntity();
        return behavior.CanBeHitByProjectile(projectile);
    }

    public override bool CanBeHitByNPC(NPC npc, NPC attacker)
    {
        if (Z != 0f)
            return false;
        if (!npc.TryGetBehavior(out AnomalyNPCBehavior behavior))
            return true;
        using EntityBehavior<NPC>.EntityLock entityLock = behavior.LockEntity();
        return behavior.CanBeHitByNPC(attacker);
    }

    public override bool? CanCollideWithPlayerMeleeAttack(NPC npc, Player player, Item item, Rectangle meleeAttackHitbox)
    {
        if (!npc.TryGetBehavior(out AnomalyNPCBehavior behavior))
            return null;
        using EntityBehavior<NPC>.EntityLock entityLock = behavior.LockEntity();
        return behavior.CanCollideWithPlayerMeleeAttack(player, item, meleeAttackHitbox);
    }

    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
    {
        if (!npc.TryGetBehavior(out AnomalyNPCBehavior behavior))
            return true;
        using EntityBehavior<NPC>.EntityLock entityLock = behavior.LockEntity();
        return behavior.CanHitPlayer(target, ref cooldownSlot);
    }

    public override bool CanHitNPC(NPC npc, NPC target)
    {
        if (!npc.TryGetBehavior(out AnomalyNPCBehavior behavior))
            return true;
        using EntityBehavior<NPC>.EntityLock entityLock = behavior.LockEntity();
        return behavior.CanHitNPC(target);
    }
}

public sealed partial class AnomalyGlobalProjectile
{
    public override bool? CanHitNPC(Projectile projectile, NPC target)
    {
        if (!projectile.TryGetBehavior(out AnomalyProjectileBehavior behavior))
            return null;
        using EntityBehavior<Projectile>.EntityLock entityLock = behavior.LockEntity();
        return behavior.CanHitNPC(target);
    }

    public override bool CanHitPvp(Projectile projectile, Player target)
    {
        if (!projectile.TryGetBehavior(out AnomalyProjectileBehavior behavior))
            return true;
        using EntityBehavior<Projectile>.EntityLock entityLock = behavior.LockEntity();
        return behavior.CanHitPvp(target);
    }

    public override bool CanHitPlayer(Projectile projectile, Player target)
    {
        if (!projectile.TryGetBehavior(out AnomalyProjectileBehavior behavior))
            return true;
        using EntityBehavior<Projectile>.EntityLock entityLock = behavior.LockEntity();
        return behavior.CanHitPlayer(target);
    }
}

public sealed partial class AnomalyGlobalItem
{
    public override bool? CanHitNPC(Item item, Player player, NPC target)
    {
        if (!item.TryGetBehavior(out AnomalyItemBehavior behavior))
            return null;
        using EntityBehavior<Item>.EntityLock entityLock = behavior.LockEntity();
        return behavior.CanHitNPC(player, target);
    }

    public override bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, Player player, NPC target)
    {
        if (!item.TryGetBehavior(out AnomalyItemBehavior behavior))
            return null;
        using EntityBehavior<Item>.EntityLock entityLock = behavior.LockEntity();
        return behavior.CanMeleeAttackCollideWithNPC(meleeAttackHitbox, player, target);
    }

    public override bool CanHitPvp(Item item, Player player, Player target)
    {
        if (!item.TryGetBehavior(out AnomalyItemBehavior behavior))
            return true;
        using EntityBehavior<Item>.EntityLock entityLock = behavior.LockEntity();
        return behavior.CanHitPvp(player, target);
    }
}
