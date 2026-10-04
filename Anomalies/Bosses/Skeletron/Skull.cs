namespace Anomalies.Bosses.Skeletron;

public sealed class Skull : AnomalyProjectileBehavior<Skull>
{
    public override int ApplyingType => ProjectileID.Skull;

    public override bool AllowCalamityLogic(CalamityLogicType_ProjectileBehavior type) => type switch
    {
        CalamityLogicType_ProjectileBehavior.PreAI => false,
        _ => true
    };

    public override bool PreAI()
    {
        // -------------------------------------------------------------------------
        // 1. 初始化效果（仅执行一次）
        // -------------------------------------------------------------------------
        if (Projectile.localAI[0] == 0f)
        {
            Projectile.localAI[0] = 1f; // 标记已初始化

            // 播放音效
            SoundEngine.PlaySound(SoundID.Item8, Projectile.position);

            // 生成 10 个尘土粒子，呈向内收缩的效果
            for (int i = 0; i < 10; i++)
            {
                int dustIndex = Dust.NewDust(
                    new Vector2(Projectile.position.X, Projectile.position.Y),
                    Projectile.width,
                    Projectile.height,
                    DustID.Blood, // 尘土类型
                    Projectile.velocity.X,
                    Projectile.velocity.Y,
                    0,
                    default,
                    2f // 缩放
                );

                Dust dust = Main.dust[dustIndex];
                dust.noGravity = true;

                // 尘土速度指向弹幕中心，然后反向并加上弹幕速度的一半
                dust.velocity = Projectile.Center - dust.position;
                dust.velocity.Normalize();
                dust.velocity *= -5f;
                dust.velocity += Projectile.velocity / 2f;
            }
        }

        // -------------------------------------------------------------------------
        // 2. 特殊行为
        // -------------------------------------------------------------------------
        switch ((int)Projectile.ai[0])
        {
            case 0: //追踪玩家
                // 查找距离弹幕中心最近的玩家索引
                int closestPlayerIndex = Player.FindClosest(Projectile.Center, 1, 1);

                // ai[1] 作为计时器，每帧递增
                Projectile.ai[1] += 1f;

                // 在计时器位于 (30, 110) 区间时，执行平滑追踪转向
                if (Projectile.ai[1] is > 30f and < 110f)
                {
                    // 当前速度大小
                    float currentSpeed = Projectile.velocity.Length();

                    // 计算指向玩家的方向向量，并缩放到当前速度大小
                    Vector2 directionToPlayer = Main.player[closestPlayerIndex].Center - Projectile.Center;
                    directionToPlayer.Normalize();
                    directionToPlayer *= currentSpeed;

                    // 平滑插值：新速度 = (旧速度 * 24 + 目标速度) / 25
                    // 相当于向目标方向缓慢转向，同时保持速度大小不变
                    Projectile.velocity = (Projectile.velocity * 24f + directionToPlayer) / 25f;

                    // 归一化后再乘以原速度大小，确保速度大小不变
                    Projectile.velocity.Normalize();
                    Projectile.velocity *= currentSpeed;
                }

                float maxSpeed = 18f; // 最大速度

                // 如果速度低于 18，则逐渐加速
                if (Projectile.velocity.Length() < maxSpeed)
                    Projectile.velocity *= 1.02f;
                break;
        }

        // -------------------------------------------------------------------------
        // 3. 透明度处理
        // -------------------------------------------------------------------------
        // 对于 270，强制透明度为 0（完全可见）
        Projectile.alpha = 0;

        // 以下 alpha 调整在 alpha 已为 0 的情况下不会生效，
        // 但保留以完全维持原逻辑（原代码中此段对 270 无实际影响）
        if (Projectile.alpha > 0)
            Projectile.alpha -= 50;
        if (Projectile.alpha < 0)
            Projectile.alpha = 0;

        // -------------------------------------------------------------------------
        // 4. 帧动画
        // -------------------------------------------------------------------------
        Projectile.frame++;
        if (Projectile.frame > 2)
            Projectile.frame = 0;

        // -------------------------------------------------------------------------
        // 5. 持续产生尘土粒子
        // -------------------------------------------------------------------------
        for (int i = 0; i < 2; i++)
        {
            int dustIndex = Dust.NewDust(
                new Vector2(Projectile.position.X + 4f, Projectile.position.Y + 4f),
                Projectile.width - 8,
                Projectile.height - 8,
                DustID.Blood, // 尘土类型
                Projectile.velocity.X * 0.2f,
                Projectile.velocity.Y * 0.2f,
                100,
                default,
                1.5f // 缩放
            );

            Dust dust = Main.dust[dustIndex];
            // 位置稍微向后偏移
            dust.position -= Projectile.velocity;
            dust.noGravity = true;
            // 速度衰减
            dust.velocity.X *= 0.3f;
            dust.velocity.Y *= 0.3f;
        }

        // -------------------------------------------------------------------------
        // 6. 旋转和朝向
        // -------------------------------------------------------------------------
        Projectile.spriteDirection = Projectile.direction;

        // 根据方向设置旋转角度，使其面向速度方向
        if (Projectile.direction < 0)
            Projectile.rotation = (float)Math.Atan2(-Projectile.velocity.Y, -Projectile.velocity.X);
        else
            Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X);

        return false; // 禁止原始 AI 执行
    }
}
