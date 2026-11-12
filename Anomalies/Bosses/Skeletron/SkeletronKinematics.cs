namespace Anomalies.Bosses.Skeletron;

/// <summary>普通骷髅王与传说之夜共用的运动；调用方负责状态、目标及网络权威。</summary>
internal static class SkeletronKinematics
{
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

    public static float GetSpinMoveSpeed(float distance, bool decelerate = true) =>
        distance < SkeletronHead.SpinSlowdownEndDistance
            ? Utils.Remap(distance, SkeletronHead.SpinSlowdownMinDistance, SkeletronHead.SpinSlowdownEndDistance, SkeletronHead.SpinMinMoveSpeed, SkeletronHead.SpinBaseMoveSpeed)
            : Utils.Remap(distance, SkeletronHead.SpinAccelerationStartDistance, SkeletronHead.SpinAccelerationMaxDistance, SkeletronHead.SpinBaseMoveSpeed, SkeletronHead.SpinMaxMoveSpeed);

    public static void SpinMovement(NPC npc, Player target, float spinSpeed, float moveSpeed)
    {
        // 头部持续自转
        npc.rotation += npc.direction * spinSpeed;

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

    // 保留实体顶部参考点，不能直接改成中心高度偏移。
    internal static Vector2 GetIdleOffset(int side, int handHeight, int headHeight, float yMultiplier = 1.3f, bool raised = false) =>
        new(side * (raised ? 120f : 200f), (raised ? -100f : 230f) * yMultiplier + (handHeight - headHeight) * 0.5f);

    internal static void UpdateIdleMovement(NPC hand, Vector2 destination, float velocityIncrement, float velocityMultiplier, float deceleration,
        Vector2? followVelocity = null)
    {
        // 启用跟随时，接近待机点后改为相对头部的阻尼收敛，避免固定加速度反复跨越目标。
        // 此分支直接替代逐轴加速，不在同一帧叠加两套归位。
        if (followVelocity is Vector2 ownerVelocity)
        {
            const float SettlingRadius = 48f;
            const float SettlingSpeed = 6f;
            const float VelocityDamping = 0.65f;
            const float PositionResponse = 0.035f;
            Vector2 relativeOffset = hand.Center - destination + ownerVelocity;
            Vector2 relativeVelocity = hand.velocity - ownerVelocity;
            if (relativeOffset.LengthSquared() <= SettlingRadius * SettlingRadius
                && relativeVelocity.LengthSquared() <= SettlingSpeed * SettlingSpeed)
            {
                Vector2 nextVelocity = relativeVelocity * VelocityDamping - relativeOffset * PositionResponse;
                hand.velocity = ownerVelocity + nextVelocity;
                return;
            }
        }

        float maxX = velocityIncrement * 100f * velocityMultiplier;
        float maxY = velocityIncrement * 100f * velocityMultiplier;

        if (hand.Center.Y > destination.Y)
        {
            if (hand.velocity.Y > 0f)
                hand.velocity.Y *= deceleration;
            hand.velocity.Y -= velocityIncrement;
            if (hand.velocity.Y > maxY)
                hand.velocity.Y = maxY;
        }
        else if (hand.Center.Y < destination.Y)
        {
            if (hand.velocity.Y < 0f)
                hand.velocity.Y *= deceleration;
            hand.velocity.Y += velocityIncrement;
            if (hand.velocity.Y < -maxY)
                hand.velocity.Y = -maxY;
        }

        if (hand.Center.X > destination.X)
        {
            if (hand.velocity.X > 0f)
                hand.velocity.X *= deceleration;
            hand.velocity.X -= velocityIncrement;
            if (hand.velocity.X > maxX)
                hand.velocity.X = maxX;
        }

        if (hand.Center.X < destination.X)
        {
            if (hand.velocity.X < 0f)
                hand.velocity.X *= deceleration;
            hand.velocity.X += velocityIncrement;
            if (hand.velocity.X < -maxX)
                hand.velocity.X = -maxX;
        }
    }

    internal static void UpdateIdleRotation(NPC hand, NPC master)
    {
        Vector2 handCenter = hand.Center;
        float offsetX = master.Center.X - 200f * hand.ai[0] - handCenter.X;
        float offsetY = master.Top.Y + 230f - handCenter.Y;
        hand.rotation = (float)Math.Atan2(offsetY, offsetX) + MathHelper.PiOver2;
    }

    internal static void UpdateVerticalDrawbackMovement(NPC hand, float velocityIncrement)
    {
        hand.velocity.X *= 0.95f;
        hand.velocity.Y -= velocityIncrement;
        if (hand.velocity.Y < -14f)
            hand.velocity.Y = -14f;
        else if (hand.velocity.Y > 10f)
            hand.velocity.Y = 10f;
    }

    internal static void UpdateHorizontalDrawbackMovement(NPC hand, float velocityIncrement)
    {
        hand.velocity.Y *= 0.95f;
        hand.velocity.X += velocityIncrement * -hand.ai[0];
        if (hand.velocity.X < -10f)
            hand.velocity.X = -10f;
        else if (hand.velocity.X > 14f)
            hand.velocity.X = 14f;
    }
}
