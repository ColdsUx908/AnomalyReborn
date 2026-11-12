namespace Anomalies.Bosses.Skeletron;

public sealed partial class SkeletronHead
{
    internal int Phase3PatternDirection => (Phase3Cycle & 1) == 0 ? 1 : -1;
    internal float Phase3PatternStartAngle => Phase3Cycle * 3 * Hexadecagon.VertexAngle;

    // LocalPhase：0 准备、1 发射、2 收尾。
    private void Phase3AI()
    {
        SkeletronPhase3Arena arena = FindActivePhase3Arena();
        // 等待竞技场网络包；服务器不在缺少竞技场时继续执行不可见的攻击。
        NPC.damage = 0;
        if (arena is null || arena.Intensity < 1f)
        {
            NPC.velocity = Vector2.Zero;
            return;
        }

        Hexadecagon polygon = arena.ArenaPolygon;
        int direction = Phase3PatternDirection;
        float startAngle = Phase3PatternStartAngle;
        int age = (int)Timer++;
        switch (CurrentBehavior)
        {
            case Behavior.Phase3_GuardianPatrol:
                GuardianPatrol();
                break;
            case Behavior.Phase3_BoneTide:
                BoneTide();
                break;
            case Behavior.Phase3_GuardianRain:
                GuardianRain();
                break;
            default:
                BeginBehavior(Behavior.Phase3_GuardianRain);
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

        void GuardianRain()
        {
            Hover();
            if (age >= Phase3Patterns.RainStart && age <= Phase3Patterns.RainEnd && age % Phase3Patterns.RainInterval == 0 && TONetworkEnvironment.NotClient)
            {
                // 参考突变体史莱姆雨：每个区间独立随机位置与速度，打散横排及左右镜像。
                float offset = 250f * MathF.Sin(MathHelper.TwoPi * (age - Phase3Patterns.RainStart) / 480f);
                float horizontalRange = polygon.CircumRadius * 1.2f;
                for (float columnX = -horizontalRange; columnX < horizontalRange; columnX += Phase3Patterns.RainColumnSpacing)
                {
                    float x = Main.rand.NextFloat(columnX, Math.Min(columnX + Phase3Patterns.RainColumnSpacing, horizontalRange));
                    // 只镜像安全通道的位置，左右两侧弹幕各自取样。
                    float distanceToSafeZone = Math.Abs(MathF.IEEERemainder(-Math.Abs(x) - offset, Phase3Patterns.RainSafeZoneSpacing));
                    if (distanceToSafeZone < Phase3Patterns.RainSafeZoneWidth * 0.5f)
                        continue;
                    SpawnRain(x);
                }
            }
            if (age >= Phase3Patterns.SkullStart && age <= Phase3Patterns.RainEnd && (age - Phase3Patterns.SkullStart) % Phase3Patterns.SkullInterval == 0 && TONetworkEnvironment.NotClient)
                FirePatternSkulls();
            SetLocalPhase(age < Phase3Patterns.RainStart ? 0 : age <= Phase3Patterns.RainEnd ? 1 : 2);
            if (age >= Phase3Patterns.RainEnd + Phase3Patterns.RainRecoveryTime - 1 && TONetworkEnvironment.NotClient)
            {
                BeginBehavior(Behavior.Phase3_GuardianPatrol);
            }

            void SpawnRain(float horizontalOffset)
            {
                Vector2 position = polygon.Center + new Vector2(horizontalOffset, -polygon.CircumRadius - 120f);
                Projectile.NewProjectileAction<SkeletronPhase3Guardian>(SourceAI, position, Vector2.UnitY * Main.rand.NextFloat(Phase3Patterns.RainMinFallSpeed, Phase3Patterns.RainMaxFallSpeed), SkullDamage, 0f,
                    action: p =>
                    {
                        p.ai[0] = NPC.whoAmI + 1;
                        p.ai[2] = SkeletronPhase3Guardian.Rain;
                        p.timeLeft = (int)MathF.Ceiling((polygon.CircumRadius * 2f + 360f) / Phase3Patterns.RainMinFallSpeed);
                    });
            }
        }

        void GuardianPatrol()
        {
            Hover();
            const int PreparationTime = Phase3Patterns.PreparationTime;
            const int PatrolTime = Phase3Patterns.PatrolTime;
            // 相同偶数采样数和相同初始缺口，保证整圈/半圈相差后弹幕与双缺口均对齐。
            if (age == 0 && TONetworkEnvironment.NotClient)
            {
                for (int layer = 0; layer < Phase3Patterns.PatrolLayers; layer++)
                {
                    float radius = polygon.CircumRadius * Phase3Patterns.PatrolRadiusFactor(layer);
                    const int Count = Phase3Patterns.PatrolCount;
                    float angularSpeed = direction * Phase3Patterns.PatrolSpeed(layer);
                    for (int i = 0; i < Count; i++)
                    {
                        float relativeAngle = MathHelper.TwoPi * i / Count;
                        // 入场始终向左右留空；每圈的总转角均为半圈整数倍，离场也朝左右。
                        float angle = relativeAngle;
                        if (Phase3Patterns.IsPatrolGap(relativeAngle))
                            continue;
                        SpawnPattern(SkeletronPhase3Guardian.Patrol, radius, angle, angularSpeed);
                    }
                }
            }
            // 与雨幕一致：从第 150 帧起每 30 帧释放五向骷髅头。
            if (Phase3Patterns.ShouldFireSkull(age) && TONetworkEnvironment.NotClient)
                FirePatternSkulls();
            SetLocalPhase(age < PreparationTime ? 0 : age < PreparationTime + PatrolTime ? 1 : 2);
            if (age >= Phase3Patterns.PatternDuration - 1)
                BeginBehavior(Behavior.Phase3_BoneTide);
        }

        void BoneTide()
        {
            Hover();
            const int WaveInterval = Phase3Patterns.TideInterval;
            const int WaveCount = Phase3Patterns.TideWaves;
            const int PreparationTime = Phase3Patterns.PreparationTime;
            const int GuardiansPerWave = Phase3Patterns.GuardiansPerTideWave;
            int wave = age / WaveInterval;
            if (age % WaveInterval == 0 && wave < WaveCount && TONetworkEnvironment.NotClient)
            {
                float gapAngle = startAngle + direction * wave * Phase3Patterns.TideGapStep;
                for (int i = 0; i < GuardiansPerWave; i++)
                {
                    float angle = MathHelper.TwoPi * i / GuardiansPerWave;
                    if (Phase3Patterns.IsTideGap(angle - gapAngle))
                        continue;
                    // 从实际十六边形边界外侧入场，不在场内突然生成。
                    float sectorAngle = MathF.IEEERemainder(angle - polygon.Rotation - Hexadecagon.VertexAngle * 0.5f, Hexadecagon.VertexAngle);
                    float radius = polygon.Apothem / MathF.Cos(sectorAngle) + 80f;
                    SpawnPattern(SkeletronPhase3Guardian.Tide, radius, angle, direction * Phase3Patterns.TideAngularSpeed);
                }
            }
            int lastRelease = (WaveCount - 1) * WaveInterval + PreparationTime;
            if (Phase3Patterns.ShouldFireSkull(age) && TONetworkEnvironment.NotClient)
                FirePatternSkulls();
            SetLocalPhase(age < PreparationTime ? 0 : age < lastRelease ? 1 : 2);
            if (age >= Phase3Patterns.PatternDuration - 1 && TONetworkEnvironment.NotClient)
            {
                Phase3Cycle = (Phase3Cycle + 1) % 32;
                BeginBehavior(Behavior.Phase3_GuardianRain);
            }
        }

        void FirePatternSkulls() =>
            SkeletronCombat.FireSkull(NPC, Target, Phase3Patterns.SkullCount,
                MathHelper.TwoPi / Phase3Patterns.SkullCount, Phase3Patterns.SkullSpeed, SkullDamage, addNPCVelocity: false);

        void SpawnPattern(int mode, float radius, float angle, float angularSpeed)
        {
            Vector2 offset = angle.ToRotationVector2() * radius;
            Projectile.NewProjectileAction<SkeletronPhase3Guardian>(SourceAI, polygon.Center + offset, Vector2.Zero, SkullDamage, 0f,
                action: p =>
                {
                    p.ai[0] = NPC.whoAmI + 1;
                    p.ai[2] = mode;
                    p.timeLeft = 720;
                    SkeletronPhase3Guardian guardian = p.GetModProjectile<SkeletronPhase3Guardian>();
                    guardian.Destination = polygon.Center;
                    guardian.InitialOffset = offset;
                    guardian.AngularSpeed = angularSpeed;
                    guardian.ArenaRadius = polygon.CircumRadius;
                });
        }
    }
}
