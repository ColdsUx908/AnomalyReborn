namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendaryNightPhase2Arena
{
    public const int FirstStarburstMusicTime = 63 * 60;
    public const int SecondStarburstMusicTime = 77 * 60 + 15;
    public const int ThirdStarburstMusicTime = 86 * 60 + 40;
    public const int FourthStarburstMusicTime = 97 * 60 + 45;
    public const int FifthStarburstMusicTime = 100 * 60 + 55;
    public const int StarburstCount = 12;
    public const int StarburstEndMusicTime = 112 * 60;
    public const int StarburstInterval = 17 * 60;
    public const int RapidStarburstInterval = 95;
    public const int StarburstWarningDuration = 12;
    public const int StarburstDamageDuration = 24;
    public const int StarburstFadeDuration = 18;
    public const int StarburstChargeDuration = 90;
    public const int StarburstColorRecoveryDuration = 18;
    public const int OrbitRecoveryStartMusicTime = StarburstEndMusicTime + StarburstDamageDuration + StarburstFadeDuration;
    public const int OrbitRecoveryDuration = 90;
    public static readonly Color FastStarburstColor = new(255, 40, 40);
    public static readonly Color SlowStarburstColor = new(255, 155, 30);
    // 接口以每帧弧度定义；速度恒定的区段可直接预测交汇位置。
    // 慢法阵从首次拍击到首次星爆恰好公转一整圈，拍击时的位置就是首次爆点。
    // 首次交汇后按音乐节点分段积分，预测与实际公转共用计算。
    public const int SlowOrbitPeriod = FirstStarburstMusicTime - SkeletronLegendHead.Mechanic2Data.FirstStrikeMusicTime;
    public static float SlowOrbitSpeed => MathHelper.TwoPi / SlowOrbitPeriod;
    public float FirstStarburstAngle => Projectile.ai[2] - 1f;
    public Vector2 FirstStarburstPosition => Projectile.Center
        + FirstStarburstAngle.ToRotationVector2() * (MainCircleRadius + SmallCircleRadius);
    // 每段均从上一个星爆的累计角度积分，调速不会重置位置。
    // 两圈的相对角速度由相邻交汇时间约束；可独立调整各段慢圈速度。
    public static float GetSlowOrbitSpeed(int interval) => interval switch
    {
        0 => SlowOrbitSpeed,
        1 => SlowOrbitSpeed,
        _ => SlowOrbitSpeed
    };

    // 第四至第五次间先保持旧速，再以SmoothStep加速；积分恰好相差一圈。
    // 加速结束后两圈分别每95帧顺时针转150°、510°，角度与角速度均连续。
    public static float RapidSlowOrbitSpeed => MathHelper.ToRadians(150f) / RapidStarburstInterval;
    private static float TransitionDuration => FifthStarburstMusicTime - FourthStarburstMusicTime;
    private static float InitialRelativeSpeed => MathHelper.TwoPi / (FourthStarburstMusicTime - ThirdStarburstMusicTime);
    private static float RapidRelativeSpeed => MathHelper.TwoPi / RapidStarburstInterval;
    private static float AccelerationDelay =>
        ((InitialRelativeSpeed + RapidRelativeSpeed) * 0.5f * TransitionDuration - MathHelper.TwoPi)
        / ((RapidRelativeSpeed - InitialRelativeSpeed) * 0.5f);

    private static float IntegrateTransition(float initialSpeed, float finalSpeed, float age)
    {
        float duration = TransitionDuration - AccelerationDelay;
        float progress = MathHelper.Clamp((age - AccelerationDelay) / duration, 0f, 1f);
        // SmoothStep(u) = 3u² - 2u³，其原函数为u³ - u⁴/2。
        float integral = duration * (progress * progress * progress - 0.5f * MathF.Pow(progress, 4f));
        return initialSpeed * age + (finalSpeed - initialSpeed) * integral;
    }

    // 恢复段对SmoothStep角速度解析积分；结束后接初始匀速，始终保持角度连续。
    private static float IntegrateOrbitRecovery(float initialSpeed, float finalSpeed, int age)
    {
        int elapsed = Math.Clamp(age, 0, OrbitRecoveryDuration);
        float progress = elapsed / (float)OrbitRecoveryDuration;
        float integral = OrbitRecoveryDuration * (progress * progress * progress - 0.5f * MathF.Pow(progress, 4f));
        return initialSpeed * elapsed + (finalSpeed - initialSpeed) * integral
            + Math.Max(0, age - OrbitRecoveryDuration) * finalSpeed;
    }

    public static float GetRelativeOrbitTurns(int musicTime)
    {
        if (musicTime <= FirstStarburstMusicTime)
            return (musicTime - FirstStarburstMusicTime) / (float)StarburstInterval;
        if (musicTime <= SecondStarburstMusicTime)
            return (musicTime - FirstStarburstMusicTime) / (float)(SecondStarburstMusicTime - FirstStarburstMusicTime);
        if (musicTime <= ThirdStarburstMusicTime)
            return 1f + (musicTime - SecondStarburstMusicTime) / (float)(ThirdStarburstMusicTime - SecondStarburstMusicTime);
        if (musicTime <= FourthStarburstMusicTime)
            return 2f + (musicTime - ThirdStarburstMusicTime) / (float)(FourthStarburstMusicTime - ThirdStarburstMusicTime);
        if (musicTime <= FifthStarburstMusicTime)
            return 3f + IntegrateTransition(InitialRelativeSpeed, RapidRelativeSpeed,
                musicTime - FourthStarburstMusicTime) / MathHelper.TwoPi;
        return 4f + (Math.Min(musicTime, OrbitRecoveryStartMusicTime) - FifthStarburstMusicTime) / (float)RapidStarburstInterval
            + IntegrateOrbitRecovery(RapidRelativeSpeed, MathHelper.TwoPi / StarburstInterval,
                musicTime - OrbitRecoveryStartMusicTime) / MathHelper.TwoPi;
    }

    public static float GetOrbitSpeed(int index, int musicTime)
    {
        float slow = musicTime < SecondStarburstMusicTime ? GetSlowOrbitSpeed(0)
            : musicTime < ThirdStarburstMusicTime ? GetSlowOrbitSpeed(1) : GetSlowOrbitSpeed(2);
        float relative = musicTime < FirstStarburstMusicTime ? MathHelper.TwoPi / StarburstInterval
            : musicTime < SecondStarburstMusicTime ? MathHelper.TwoPi / (SecondStarburstMusicTime - FirstStarburstMusicTime)
            : musicTime < ThirdStarburstMusicTime ? MathHelper.TwoPi / (ThirdStarburstMusicTime - SecondStarburstMusicTime)
            : InitialRelativeSpeed;
        if (musicTime >= OrbitRecoveryStartMusicTime)
        {
            float progress = MathHelper.Clamp((musicTime - OrbitRecoveryStartMusicTime) / (float)OrbitRecoveryDuration, 0f, 1f);
            slow = MathHelper.SmoothStep(RapidSlowOrbitSpeed, SlowOrbitSpeed, progress);
            relative = MathHelper.SmoothStep(RapidRelativeSpeed, MathHelper.TwoPi / StarburstInterval, progress);
        }
        else if (musicTime >= FifthStarburstMusicTime)
        {
            slow = RapidSlowOrbitSpeed;
            relative = RapidRelativeSpeed;
        }
        else if (musicTime > FourthStarburstMusicTime)
        {
            float progress = MathHelper.Clamp((musicTime - FourthStarburstMusicTime - AccelerationDelay)
                / (TransitionDuration - AccelerationDelay), 0f, 1f);
            slow = MathHelper.SmoothStep(GetSlowOrbitSpeed(2), RapidSlowOrbitSpeed, progress);
            relative = MathHelper.SmoothStep(InitialRelativeSpeed, RapidRelativeSpeed, progress);
        }
        return slow + (index == 2 ? relative : 0f);
    }

    public float GetOrbitAngle(int index, int musicTime)
    {
        int first = FirstStarburstMusicTime;
        int second = SecondStarburstMusicTime;
        int third = ThirdStarburstMusicTime;
        int fourth = FourthStarburstMusicTime;
        int fifth = FifthStarburstMusicTime;
        float slowAngle = (Math.Min(musicTime, first) - SkeletronLegendHead.Mechanic2Data.FirstStrikeMusicTime) * SlowOrbitSpeed
            + Math.Clamp(musicTime - first, 0, second - first) * GetSlowOrbitSpeed(0)
            + Math.Clamp(musicTime - second, 0, third - second) * GetSlowOrbitSpeed(1)
            + Math.Clamp(musicTime - third, 0, fourth - third) * GetSlowOrbitSpeed(2)
            + IntegrateTransition(GetSlowOrbitSpeed(2), RapidSlowOrbitSpeed, Math.Clamp(musicTime - fourth, 0, fifth - fourth))
            + Math.Clamp(musicTime - fifth, 0, OrbitRecoveryStartMusicTime - fifth) * RapidSlowOrbitSpeed
            + IntegrateOrbitRecovery(RapidSlowOrbitSpeed, SlowOrbitSpeed, musicTime - OrbitRecoveryStartMusicTime);
        return FirstStarburstAngle + slowAngle
            + (index == 2 ? MathHelper.TwoPi * GetRelativeOrbitTurns(musicTime) : 0f);
    }

    public int StarburstIndex => GetStarburstIndex(MasterBehavior?.Phase2MusicElapsedTime ?? 0);

    public static int GetStarburstIndex(int musicTime)
    {
        // 最后一次允许完整结算伤害及淡出，不在1:52截断。
        if (musicTime < FirstStarburstMusicTime - StarburstWarningDuration
            || musicTime >= GetStarburstTime(StarburstCount - 1) + StarburstDamageDuration + StarburstFadeDuration)
            return -1;
        for (int i = StarburstCount - 1; i >= 0; i--)
            if (musicTime >= GetStarburstTime(i) - StarburstWarningDuration)
                return i;
        return -1;
    }

    public static int GetStarburstTime(int index) => index switch
    {
        0 => FirstStarburstMusicTime,
        1 => SecondStarburstMusicTime,
        2 => ThirdStarburstMusicTime,
        3 => FourthStarburstMusicTime,
        _ => FifthStarburstMusicTime + (index - 4) * RapidStarburstInterval
    };
    public Vector2 PredictStarburstPosition(int index) => index == 0 ? FirstStarburstPosition
        : Projectile.Center + GetOrbitAngle(1, GetStarburstTime(index)).ToRotationVector2()
            * (MainCircleRadius + SmallCircleRadius);
    public int StarburstAge => StarburstIndex < 0 ? int.MinValue
        : (MasterBehavior?.Phase2MusicElapsedTime ?? 0) - GetStarburstTime(StarburstIndex);
    // 偶数次星爆外圈略大于竞技场总体直径，场内仅保留爆点附近的内圈安全区。
    public float StarburstRadius => Radius * (StarburstIsRing ? 2.1f : 1.5f);
    public float StarburstSafeRadius => Radius * 0.65f;
    public bool StarburstIsRing => StarburstIndex % 2 == 1;
    public float StarburstGlow
    {
        get
        {
            int time = MasterBehavior?.Phase2MusicElapsedTime ?? 0;
            if (MasterBehavior is not { StarburstActive: true }
                || time >= GetStarburstTime(StarburstCount - 1) + StarburstColorRecoveryDuration)
                return 0f;
            int next = 0;
            while (next < StarburstCount - 1 && time > GetStarburstTime(next))
                next++;
            int nextTime = GetStarburstTime(next);
            // 快速连续星爆缩短蓄光，留出恢复原色的间隔。
            int chargeDuration = next >= 4 ? Math.Min(StarburstChargeDuration, RapidStarburstInterval / 3)
                : StarburstChargeDuration;
            float charge = nextTime >= time
                ? MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(1f - (nextTime - time) / (float)chargeDuration, 0f, 1f))
                : 0f;
            int previous = time > nextTime ? next : next - 1;
            float recovery = previous >= 0
                ? 1f - MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(
                    (time - GetStarburstTime(previous)) / (float)StarburstColorRecoveryDuration, 0f, 1f))
                : 0f;
            return Math.Max(charge, recovery);
        }
    }
    public float StarburstOpacity => StarburstAge < -StarburstWarningDuration ? 0f
        : StarburstAge < 0 ? 0.22f * (1f + StarburstAge / (float)StarburstWarningDuration)
        : 0.65f * MathHelper.Clamp((StarburstDamageDuration + StarburstFadeDuration - StarburstAge)
            / (float)StarburstFadeDuration, 0f, 1f);

    private void UpdateStarburst()
    {
        if (Main.dedServ || MasterBehavior is not { StarburstActive: true } || StarburstIndex < 0)
            return;
        int age = StarburstAge;
        if (age == 0)
            SoundEngine.PlaySound(CoinJudgment.ExplosionSound, PredictStarburstPosition(StarburstIndex));
        if (age is < 0 or >= StarburstDamageDuration)
            return;
        Player player = Main.LocalPlayer;
        Vector2 center = PredictStarburstPosition(StarburstIndex);
        bool hit = StarburstIsRing
            ? new Annulus(center, StarburstSafeRadius, StarburstRadius).Collides(player.Hitbox)
            : new Circle(center, StarburstRadius).Collides(player.Hitbox);
        if (hit)
            LegendaryNightDamage.HurtLocalPlayer(Projectile, Projectile.damage, true);
    }
}
