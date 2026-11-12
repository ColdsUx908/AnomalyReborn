namespace Anomalies.Tweaks.PreHardmode.Projectiles;

/// <summary>
/// 普通蜜蜂和巨型蜜蜂修改：
/// <list type="bullet">
/// <item/><description/>在 PreAI 中完整实现蜜蜂 AI，并返回 false 跳过原版 AI。
/// <item/><description/>弹幕生命周期缩短至 5 秒。
/// <item/><description/>追踪目标速度随存活时间每游戏刻增加 0.1，目标速度与实际速度最高为 20。
/// <item/><description/>基础转向加速度提高至原版的 3 倍，并按当前目标速度与基础速度的比例增强，改善高速追踪。
/// </list>
/// 保留原版索敌延迟、目标筛选、动画及液体处理规则。
/// </summary>
public abstract class BeeBehavior : AnomalyProjectileBehavior
{
    private const float MaxHomingSpeed = 20f;
    // Increase the desired speed by 6 pixels per tick over one second of life.
    private const float HomingAcceleration = 0.1f;
    private const float TurnAccelerationMultiplier = 3f;
    private const float TargetSearchDistance = 800f;
    private const float HomingDelay = 30f;

    public override void SetDefaultsFinal()
    {
        Projectile.timeLeft = 300;
    }

    public override bool PreAI()
    {
        // ai[0] retains vanilla's homing delay; ai[2] tracks age in game ticks.
        Projectile.ai[2] += 1f / Projectile.MaxUpdates;

        if (Projectile.wet && !Projectile.honeyWet && !Projectile.shimmerWet)
        {
            Projectile.Kill();
            return false;
        }

        if (Projectile.alpha > 0)
            Projectile.alpha = Math.Max(0, Projectile.alpha - 50);
        else
            Projectile.extraUpdates = 0;

        if (Projectile.velocity.X > 0f)
            Projectile.spriteDirection = 1;
        else if (Projectile.velocity.X < 0f)
            Projectile.spriteDirection = -1;

        Projectile.rotation = Projectile.velocity.X * 0.1f;
        if (++Projectile.frameCounter >= 3)
        {
            Projectile.frameCounter = 0;
            if (++Projectile.frame >= 3)
                Projectile.frame = 0;
        }

        // Vanilla uses integer half-sizes and Manhattan distance for target selection.
        Vector2 searchOrigin = Projectile.position + new Vector2(Projectile.width / 2, Projectile.height / 2);
        Vector2 targetPosition = searchOrigin + Projectile.velocity * 100f;
        float closestDistance = TargetSearchDistance;
        Projectile.ai[0] += 1f;
        if (Projectile.ai[0] > HomingDelay)
        {
            Projectile.ai[0] = HomingDelay;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy(Projectile) || (npc.wet && npc.type != NPCID.DukeFishron))
                    continue;

                Vector2 npcCenter = npc.position + new Vector2(npc.width / 2, npc.height / 2);
                float distance = Math.Abs(searchOrigin.X - npcCenter.X) + Math.Abs(searchOrigin.Y - npcCenter.Y);
                if (distance < closestDistance && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height))
                {
                    closestDistance = distance;
                    targetPosition = npcCenter;
                }
            }
        }

        bool giantBee = Projectile.type == ProjectileID.GiantBee;
        float baseSpeed = giantBee ? 6.8f : 6f;
        float homingSpeed = Math.Min(MaxHomingSpeed, baseSpeed + Projectile.ai[2] * HomingAcceleration);
        // Scale steering with flight speed so older bees can still follow moving targets.
        float turnAcceleration = (giantBee ? 0.14f : 0.1f) * TurnAccelerationMultiplier * homingSpeed / baseSpeed;
        // Avoid dividing by zero when the bee is stationary or overlaps its target's center.
        Vector2 desiredVelocity = (targetPosition - Projectile.Center).SafeNormalize(Vector2.Zero) * homingSpeed;
        SteerAxis(ref Projectile.velocity.X, desiredVelocity.X);
        SteerAxis(ref Projectile.velocity.Y, desiredVelocity.Y);

        if (Projectile.velocity.Length() > MaxHomingSpeed)
            Projectile.velocity.Modulus = MaxHomingSpeed;

        return false;

        void SteerAxis(ref float velocity, float desired)
        {
            if (velocity < desired)
            {
                velocity += turnAcceleration;
                if (velocity < 0f && desired > 0f)
                    velocity += turnAcceleration * 2f;
            }
            else if (velocity > desired)
            {
                velocity -= turnAcceleration;
                if (velocity > 0f && desired < 0f)
                    velocity -= turnAcceleration * 2f;
            }
        }
    }
}

/// <inheritdoc cref="BeeBehavior"/>
public sealed class Bee : BeeBehavior
{
    public override int ApplyingType => ProjectileID.Bee;
}

/// <inheritdoc cref="BeeBehavior"/>
public sealed class GiantBee : BeeBehavior
{
    public override int ApplyingType => ProjectileID.GiantBee;
}
