namespace Anomalies.Bosses.Skeletron;

public sealed class SkeletronHead : AnomalyNPCBehavior<SkeletronHead>
{
    public override int ApplyingType => NPCID.SkeletronHead;

    public override bool ShouldProcess => false;

    public static void NormalMovement(NPC npc, Player target, float maxSpeedX, float maxSpeedY, float accelerationX, float accelerationY, float targetDistanceAboveTarget, float rotationFactor = 15f)
    {
        npc.Face(target);

        // Y 方向：保持自己在玩家上方
        if (npc.Top.Y > target.Top.Y - targetDistanceAboveTarget)
        {
            if (npc.velocity.Y > 0f)
                npc.velocity.Y *= 0.98f;

            npc.velocity.Y -= accelerationY;
            if (npc.velocity.Y > maxSpeedY)
                npc.velocity.Y = maxSpeedY;
        }
        else if (npc.Top.Y < target.Top.Y - targetDistanceAboveTarget)
        {
            if (npc.velocity.Y < 0f)
                npc.velocity.Y *= 0.98f;

            npc.velocity.Y += accelerationY;
            if (npc.velocity.Y < -maxSpeedY)
                npc.velocity.Y = -maxSpeedY;
        }

        // X 方向：水平追踪玩家
        if (npc.Center.X > target.Center.X)
        {
            if (npc.velocity.X > 0f)
                npc.velocity.X *= 0.98f;

            npc.velocity.X -= accelerationX;
            if (npc.velocity.X > maxSpeedX)
                npc.velocity.X = maxSpeedX;
        }

        if (npc.Center.X < target.Center.X)
        {
            if (npc.velocity.X < 0f)
                npc.velocity.X *= 0.98f;

            npc.velocity.X += accelerationX;
            if (npc.velocity.X < -maxSpeedX)
                npc.velocity.X = -maxSpeedX;
        }

        npc.rotation = npc.velocity.X / rotationFactor;
    }

    public static void RotatingMovement(NPC npc, Player target, float rotationSpeed, float moveSpeed)
    {
        // 旋转：围绕自身朝向持续旋转
        npc.rotation += npc.direction * rotationSpeed;

        // 计算指向玩家的向量
        Vector2 targetVector = target.Center - npc.Center;
        float distance = targetVector.Length();

        if (distance > 0f)
        {
            // 归一化后乘以移动速度，等价于原代码的 8f / distance 缩放
            float speedFactor = moveSpeed / distance;
            npc.velocity.X = targetVector.X * speedFactor;
            npc.velocity.Y = targetVector.Y * speedFactor;
        }
        else
        {
            npc.velocity = Vector2.Zero;
        }
    }

    public static void FireSkull(NPC npc, Player target, int amount, float radian, float skullProjSpeed, int damage, bool fireFromMouth = true, int ai0 = 0)
    {
        Vector2 velocity = npc.GetVelocityTowards(target, skullProjSpeed) + npc.velocity;
        Vector2 position = npc.Center + velocity;
        if (fireFromMouth)
            position += new Vector2(0f, 25f * npc.scale).RotatedBy(npc.rotation);

        int type = ProjectileID.Skull;

        if (amount == 1)
            Projectile.NewProjectileAction(npc.GetSource_FromAI(), position, velocity, type, damage, 0f, action: FireSkullAction);
        else
            Projectile.NewProjectilesArc(amount, radian, npc.GetSource_FromAI(), position, velocity, type, damage, 0f, action: FireSkullAction);

        npc.netUpdate = true;

        void FireSkullAction(Projectile p)
        {
            p.ai[0] = ai0;
            p.timeLeft = 600;
        }
    }
}
