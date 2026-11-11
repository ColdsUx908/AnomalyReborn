namespace Anomalies.Bosses.Skeletron;

public sealed partial class SkeletronHead
{
    private const int Phase3VolleyDuration = 480;
    private const int Phase3PreparationTime = 60;
    private const int Phase3VolleyInterval = 16;
    private const int Phase3OppositeInterval = 36;

    // LocalPhase：0 准备、1 发射、2 收尾。
    private void Phase3AI()
    {
        SkeletronPhase3Arena arena = null;
        foreach (Projectile projectile in Projectile.ActiveProjectiles)
        {
            if (projectile.ModProjectile is SkeletronPhase3Arena candidate && candidate.Master == NPC && candidate.IsActivated)
            {
                arena = candidate;
                break;
            }
        }

        // 等待竞技场网络包；服务器不在缺少竞技场时继续执行不可见的攻击。
        NPC.damage = 0;
        if (arena is null || arena.Intensity < 1f)
        {
            NPC.velocity = Vector2.Zero;
            return;
        }

        Hexadecagon polygon = arena.ArenaPolygon;
        int direction = (Phase3Cycle & 1) == 0 ? 1 : -1;
        int startNode = Phase3Cycle * 3 % Hexadecagon.VertexCount;
        int age = (int)Timer++;
        switch (CurrentBehavior)
        {
            case Behavior.Phase3_NodeVolley:
                NodeVolley();
                break;
            case Behavior.Phase3_GuardianRain:
                GuardianRain();
                break;
            default:
                BeginBehavior(Behavior.Phase3_NodeVolley);
                break;
        }

        void Hover()
        {
            NPC.velocity = polygon.Center - NPC.Center;
            NPC.rotation = 0f;
        }
        void BeginBehavior(Behavior behavior)
        {
            if (!TONetworkEnvironment.NotClient)
                return;
            CurrentBehavior = behavior;
            CurrentLocalPhase = 0;
            Timer = AttackTimer = 0f;
            NPC.netUpdate = true;
        }

        void SetLocalPhase(int localPhase)
        {
            if (TONetworkEnvironment.NotClient)
                CurrentLocalPhase = localPhase;
        }

        void NodeVolley()
        {
            Hover();
            int shotAge = age - Phase3PreparationTime;
            if (shotAge >= 0 && shotAge % Phase3VolleyInterval == 0 && shotAge / Phase3VolleyInterval < Hexadecagon.VertexCount
                && TONetworkEnvironment.NotClient)
            {
                int node = startNode + direction * 5 * (shotAge / Phase3VolleyInterval);
                Vector2 position = polygon.GetVertex(node);
                Projectile.NewProjectileAction<SkeletronSkullWarning>(SourceAI, position, Vector2.Zero, SkullDamage, 0f,
                    action: p =>
                    {
                        p.ai[0] = Target.whoAmI + 1;
                        // 使用既有 60 帧预警，从年龄 12 开始：跟踪 36 帧，再固定 12 帧。
                        p.ai[1] = 12f;
                        p.ai[2] = SkeletronSkullWarning.EncodeSource(NPC.whoAmI, false);
                        p.rotation = (Target.Center - position).ToRotation();
                        p.GetModProjectile<SkeletronSkullWarning>().StopTrackingTime = 48;
                    });
            }
            int pairAge = age - Phase3PreparationTime;
            if (pairAge >= 0 && pairAge % Phase3OppositeInterval == 0 && pairAge / Phase3OppositeInterval < 8
                && TONetworkEnvironment.NotClient)
            {
                int node = startNode + direction * (pairAge / Phase3OppositeInterval);
                SpawnGuardian(polygon.GetVertex(node), polygon.GetVertex(node + 8));
                SpawnGuardian(polygon.GetVertex(node + 8), polygon.GetVertex(node));
            }
            SetLocalPhase(age < Phase3PreparationTime ? 0 : age < 360 ? 1 : 2);
            if (age >= Phase3VolleyDuration - 1)
                BeginBehavior(Behavior.Phase3_GuardianRain);
        }

        void GuardianRain()
        {
            Hover();
            const int RainStart = 60;
            const int RainEnd = 420;
            const int RecoveryTime = 150;
            const int RainInterval = 5;
            const float ColumnSpacing = 75f;
            const float SafeZoneSpacing = 640f;
            const float SafeZoneWidth = 288f;
            const float MinFallSpeed = 15f;
            const float MaxFallSpeed = 20f;
            if (age >= RainStart && age <= RainEnd && age % RainInterval == 0 && TONetworkEnvironment.NotClient)
            {
                // 参考突变体史莱姆雨：每个区间独立随机位置与速度，打散横排及左右镜像。
                float offset = 250f * MathF.Sin(MathHelper.TwoPi * (age - RainStart) / 480f);
                float horizontalRange = polygon.CircumRadius * 1.2f;
                for (float columnX = -horizontalRange; columnX < horizontalRange; columnX += ColumnSpacing)
                {
                    float x = Main.rand.NextFloat(columnX, Math.Min(columnX + ColumnSpacing, horizontalRange));
                    // 只镜像安全通道的位置，左右两侧弹幕各自取样。
                    float distanceToSafeZone = Math.Abs(MathF.IEEERemainder(-Math.Abs(x) - offset, SafeZoneSpacing));
                    if (distanceToSafeZone < SafeZoneWidth * 0.5f)
                        continue;
                    SpawnRain(x);
                }
            }
            if (age >= 150 && age <= RainEnd && (age - 150) % 30 == 0 && TONetworkEnvironment.NotClient)
                SkeletronHandler.FireSkull(NPC, Target, 5, MathHelper.TwoPi / 5f, 15f, SkullDamage, addNPCVelocity: false);
            SetLocalPhase(age < RainStart ? 0 : age <= RainEnd ? 1 : 2);
            if (age >= RainEnd + RecoveryTime - 1 && TONetworkEnvironment.NotClient)
            {
                Phase3Cycle = (Phase3Cycle + 1) % 32;
                BeginBehavior(Behavior.Phase3_NodeVolley);
            }

            void SpawnRain(float horizontalOffset)
            {
                Vector2 position = polygon.Center + new Vector2(horizontalOffset, -polygon.CircumRadius - 120f);
                Projectile.NewProjectileAction<SkeletronPhase3Guardian>(SourceAI, position, Vector2.UnitY * Main.rand.NextFloat(MinFallSpeed, MaxFallSpeed), SkullDamage, 0f,
                    action: p =>
                    {
                        p.ai[0] = NPC.whoAmI + 1;
                        p.ai[2] = 2f;
                        p.timeLeft = 150;
                    });
            }
        }

        void SpawnGuardian(Vector2 position, Vector2 destination)
        {
            Projectile.NewProjectileAction<SkeletronPhase3Guardian>(SourceAI, position, Vector2.Zero, SkullDamage, 0f,
                action: p =>
                {
                    p.ai[0] = NPC.whoAmI + 1;
                    p.GetModProjectile<SkeletronPhase3Guardian>().Destination = destination;
                });
        }
    }
}