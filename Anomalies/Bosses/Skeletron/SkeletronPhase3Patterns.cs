namespace Anomalies.Bosses.Skeletron;

public sealed partial class SkeletronHead
{
    internal static class Phase3Patterns
    {
        public const int RainStart = 60;
        public const int RainEnd = 420;
        public const int RainRecoveryTime = 150;
        public const int RainInterval = 5;
        public const float RainColumnSpacing = 75f;
        public const float RainSafeZoneSpacing = 640f;
        public const float RainSafeZoneWidth = 288f;
        public const float RainMinFallSpeed = 15f;
        public const float RainMaxFallSpeed = 20f;
        public const int SkullCount = 5;
        public const float SkullSpeed = 15f;
        public const int GuardiansPerTideWave = 144;

        public const int PreparationTime = 60;
        public const int SkullStart = 150;
        public const int SkullInterval = 30;
        public const int SkullVolleys = 10;
        public const int AttackEnd = SkullStart + SkullInterval * SkullVolleys;
        public const int RecoveryTime = 180;
        public const int PatternDuration = AttackEnd + RecoveryTime;
        public const int PatrolTime = AttackEnd - PreparationTime;
        public const int PatrolCount = 96;
        public const int PatrolLayers = 5;
        public const int PatrolExitInterval = 6;
        public const float PatrolBaseSpeed = 2f * MathHelper.Pi / PatrolTime;
        public const float PatrolGapHalfAngle = MathHelper.Pi / 8f;
        public const int TideInterval = 30;
        public const int TideWaves = (AttackEnd - PreparationTime) / TideInterval + 1;
        public const float TideAngularSpeed = 0.012f;
        public const float TideGapHalfAngle = MathHelper.Pi / 12f;
        public const float TideGapPeriod = MathHelper.TwoPi / 3f;

        // 各层在 390 帧内转过 3、2.5、2、1.5、1 圈，结束时双缺口均回到左右。
        public static float PatrolSpeed(int layer) => PatrolBaseSpeed + (PatrolLayers - 1 - layer) * MathHelper.Pi / PatrolTime;
        public static float PatrolRadiusFactor(int layer) => 0.1f + layer * 0.2f;
        // 按层与出生采样点确定差速，不使用客户端随机数；同圈每枚加速度唯一，以较大差距形成前后散开的退场。
        public static float PatrolExitAcceleration(int layer, int node) => (0.4f + layer * 0.05f)
            * (0.55f + 0.9f * ((node * 37 + layer * 17) % 97) / 96f);
        // 间隔减半、生成缺口的平均角速度提高至原来的 1.5 倍，不为凑满一周额外提速。
        public const float TideGapStep = Hexadecagon.VertexAngle * 1.125f;
        public static bool ShouldFireSkull(int age) => age >= SkullStart && age < AttackEnd
            && (age - SkullStart) % SkullInterval == 0;
        // 包含边界并容纳浮点舍入，避免相对两侧/三组边沿多出一枚守卫。
        public static bool IsPatrolGap(float angle) => Math.Abs(MathF.IEEERemainder(angle, MathHelper.Pi)) <= PatrolGapHalfAngle + 0.000001f;
        public static bool IsTideGap(float angle) => Math.Abs(MathF.IEEERemainder(angle, TideGapPeriod)) <= TideGapHalfAngle + 0.000001f;
    }
}
