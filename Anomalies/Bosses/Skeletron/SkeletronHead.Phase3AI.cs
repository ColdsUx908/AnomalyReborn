namespace Anomalies.Bosses.Skeletron;

public sealed partial class SkeletronHead
{
    private const int Phase3VolleyDuration = 420;
    private const int Phase3OppositeDuration = 480;
    private const int Phase3ChordDuration = 420;
    private const int Phase3RingDuration = 480;
    private const int Phase3PreparationTime = 60;
    private const int Phase3VolleyInterval = 16;
    private const int Phase3OppositeInterval = 36;
    private const int Phase3ChordRoundTime = 120;
    private const int Phase3ChordApproachTime = 30;
    private const int Phase3ChordWarningTime = 30;
    private const int Phase3ChordFlightTime = 45;
    private const int Phase3RingInterval = 120;
    private const int Phase3GapNodeCount = 4;
    private const float Phase3ChordRadiusFactor = 0.82f;
    private const float Phase3HoverHeightFactor = 0.55f;
    private const float Phase3HoverSpeed = 8f;
    private const float Phase3HoverResponse = 0.08f;
    private const float Phase3ChargeRotationSpeed = 0.15f;

    // LocalPhase：节点招式为 0 准备、1 发射、2 收尾；弦线冲锋为 round * 4 +
    // (0 就位、1 预警、2 冲锋、3 刹停)，12 为三连冲结束后的休息。
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
            case Behavior.Phase3_OppositeGuardians:
                OppositeGuardians();
                break;
            case Behavior.Phase3_ChordCharge:
                ChordCharge();
                break;
            case Behavior.Phase3_ClosingRing:
                ClosingRing();
                break;
            default:
                BeginBehavior(Behavior.Phase3_NodeVolley);
                break;
        }

        void Hover()
        {
            Vector2 destination = polygon.Center - Vector2.UnitY * polygon.CircumRadius * Phase3HoverHeightFactor;
            Vector2 velocity = (destination - NPC.Center) * Phase3HoverResponse;
            float speed = velocity.Length();
            NPC.velocity = speed > Phase3HoverSpeed ? velocity * (Phase3HoverSpeed / speed) : velocity;
            NPC.rotation = NPC.velocity.X / 15f;
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
            SetLocalPhase(age < Phase3PreparationTime ? 0 : age < 360 ? 1 : 2);
            if (age >= Phase3VolleyDuration - 1)
                BeginBehavior(Behavior.Phase3_OppositeGuardians);
        }

        void OppositeGuardians()
        {
            Hover();
            int pairAge = age - Phase3PreparationTime;
            if (pairAge >= 0 && pairAge % Phase3OppositeInterval == 0 && pairAge / Phase3OppositeInterval < 8
                && TONetworkEnvironment.NotClient)
            {
                int node = startNode + direction * (pairAge / Phase3OppositeInterval);
                SpawnGuardian(polygon.GetVertex(node), polygon.GetVertex(node + 8), false);
                SpawnGuardian(polygon.GetVertex(node + 8), polygon.GetVertex(node), false);
            }
            SetLocalPhase(age < Phase3PreparationTime ? 0 : age < 360 ? 1 : 2);
            if (age >= Phase3OppositeDuration - 1)
                BeginBehavior(Behavior.Phase3_ChordCharge);
        }

        void ChordCharge()
        {
            if (age >= Phase3ChordRoundTime * ChargesPerRound)
            {
                Hover();
                SetLocalPhase(ChargesPerRound * 4);
                if (age >= Phase3ChordDuration - 1)
                    BeginBehavior(Behavior.Phase3_ClosingRing);
                return;
            }

            int round = age / Phase3ChordRoundTime;
            int roundAge = age % Phase3ChordRoundTime;
            if (roundAge == 0 && TONetworkEnvironment.NotClient)
            {
                int node = startNode + direction * round * 5;
                SetVector(0, polygon.Center + (polygon.GetVertex(node) - polygon.Center) * Phase3ChordRadiusFactor);
                SetVector(1, polygon.Center + (polygon.GetVertex(node + direction * (6 + round % 2)) - polygon.Center) * Phase3ChordRadiusFactor);
                SetVector(2, NPC.Center);
                NPC.netUpdate = true;
            }

            Vector2 start = AnomalyNPC.AnomalyAI64[0].GetValue<Vector2>();
            Vector2 end = AnomalyNPC.AnomalyAI64[1].GetValue<Vector2>();
            if (roundAge < Phase3ChordApproachTime)
            {
                SetLocalPhase(round * 4);
                float progress = (roundAge + 1f) / Phase3ChordApproachTime;
                Vector2 position = Vector2.SmoothStep(AnomalyNPC.AnomalyAI64[2].GetValue<Vector2>(), start, progress);
                NPC.velocity = position - NPC.Center;
                NPC.rotation *= 0.85f;
            }
            else if (roundAge < Phase3ChordApproachTime + Phase3ChordWarningTime)
            {
                SetLocalPhase(round * 4 + 1);
                NPC.velocity = Vector2.Zero;
            }
            else if (roundAge < Phase3ChordApproachTime + Phase3ChordWarningTime + Phase3ChordFlightTime)
            {
                SetLocalPhase(round * 4 + 2);
                NPC.damage = (int)(NPC.defDamage * SpinDamageMult);
                NPC.rotation += direction * Phase3ChargeRotationSpeed;
                NPC.velocity = (end - start) / Phase3ChordFlightTime;
            }
            else
            {
                SetLocalPhase(round * 4 + 3);
                NPC.velocity = Vector2.Zero;
                if (roundAge == Phase3ChordApproachTime + Phase3ChordWarningTime + Phase3ChordFlightTime && TONetworkEnvironment.NotClient)
                {
                    Vector2 forward = (polygon.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                    for (int i = -1; i <= 1; i++)
                    {
                        Projectile.NewProjectileAction(SourceAI, NPC.Center, forward.RotatedBy(i * MathHelper.PiOver4) * 8f,
                            ProjectileID.Skull, SkullDamage, 0f, action: p =>
                            {
                                Skull.InitializeFlight(p, Skull.Straight, 8f, 1f);
                                Skull.SetSource(p, NPC);
                                p.timeLeft = 70;
                            });
                    }
                }
            }
        }

        void ClosingRing()
        {
            Hover();
            int ringAge = age - Phase3PreparationTime;
            if (ringAge >= 0 && ringAge % Phase3RingInterval == 0 && ringAge / Phase3RingInterval < 3
                && TONetworkEnvironment.NotClient)
            {
                int gapStart = startNode + direction * 2 * (ringAge / Phase3RingInterval);
                for (int i = Phase3GapNodeCount; i < Hexadecagon.VertexCount; i++)
                {
                    int node = gapStart + direction * i;
                    Vector2 position = polygon.GetVertex(node);
                    Vector2 end = polygon.Center + (position - polygon.Center).SafeNormalize(Vector2.UnitY) * 300f;
                    SpawnGuardian(position, end, true);
                }
            }
            SetLocalPhase(age < Phase3PreparationTime ? 0 : age < 430 ? 1 : 2);
            if (age >= Phase3RingDuration - 1 && TONetworkEnvironment.NotClient)
            {
                Phase3Cycle = (Phase3Cycle + 1) % 32;
                BeginBehavior(Behavior.Phase3_NodeVolley);
            }
        }

        void SpawnGuardian(Vector2 position, Vector2 destination, bool closingRing)
        {
            Projectile.NewProjectileAction<SkeletronPhase3Guardian>(SourceAI, position, Vector2.Zero, SkullDamage, 0f,
                action: p =>
                {
                    p.ai[0] = NPC.whoAmI + 1;
                    p.ai[2] = closingRing ? 1f : 0f;
                    p.GetModProjectile<SkeletronPhase3Guardian>().Destination = destination;
                });
        }

        void SetVector(int index, Vector2 value)
        {
            AnomalyNPC.AnomalyAI64[index].SetValue(value);
            AnomalyNPC.AIChanged64[index] = true;
        }
    }

    private void DrawPhase3ChordWarning(SpriteBatch spriteBatch)
    {
        if (!Phase3 || CurrentBehavior != Behavior.Phase3_ChordCharge || Timer >= Phase3ChordRoundTime * ChargesPerRound)
            return;
        int roundAge = ((int)Timer - 1) % Phase3ChordRoundTime;
        if (roundAge < Phase3ChordApproachTime || roundAge >= Phase3ChordApproachTime + Phase3ChordWarningTime)
            return;
        Vector2 start = AnomalyNPC.AnomalyAI64[0].GetValue<Vector2>();
        Vector2 edge = AnomalyNPC.AnomalyAI64[1].GetValue<Vector2>() - start;
        float opacity = MathHelper.Clamp((roundAge - Phase3ChordApproachTime + 1f) / 8f, 0f, 1f);
        AnomalyDrawUtils.DrawLayeredWarning(spriteBatch, start, edge.ToRotation(), edge.Length(),
            NPC.width * NPC.scale, SkeletronHandler.BoneColor, Color.White, opacity);
    }
}
