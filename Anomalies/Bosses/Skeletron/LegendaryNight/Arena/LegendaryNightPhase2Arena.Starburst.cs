namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendaryNightPhase2Arena
{
    public const int FirstStarburstMusicTime = 63 * 60;
    public const int SecondStarburstMusicTime = 77 * 60 + 15;
    public const int ThirdStarburstMusicTime = 97 * 60;
    public const int StarburstEndMusicTime = 112 * 60;
    public const int StarburstInterval = 17 * 60;
    public const int RapidStarburstInterval = 48;
    public const int StarburstWarningDuration = 12;
    public const int StarburstDamageDuration = 24;
    public const int StarburstFadeDuration = 18;
    public const int StarburstChargeDuration = 90;
    public const int StarburstColorRecoveryDuration = 18;
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

    public static float GetRelativeOrbitTurns(int musicTime) => musicTime <= FirstStarburstMusicTime
        ? (musicTime - FirstStarburstMusicTime) / (float)StarburstInterval
        : musicTime <= SecondStarburstMusicTime
            ? (musicTime - FirstStarburstMusicTime) / (float)(SecondStarburstMusicTime - FirstStarburstMusicTime)
            : musicTime <= ThirdStarburstMusicTime
                ? 1f + (musicTime - SecondStarburstMusicTime) / (float)(ThirdStarburstMusicTime - SecondStarburstMusicTime)
                : 2f + (musicTime - ThirdStarburstMusicTime) / (float)RapidStarburstInterval;

    public float GetOrbitAngle(int index, int musicTime)
    {
        int first = FirstStarburstMusicTime;
        int second = SecondStarburstMusicTime;
        int third = ThirdStarburstMusicTime;
        float slowAngle = (Math.Min(musicTime, first) - SkeletronLegendHead.Mechanic2Data.FirstStrikeMusicTime) * SlowOrbitSpeed
            + Math.Clamp(musicTime - first, 0, second - first) * GetSlowOrbitSpeed(0)
            + Math.Clamp(musicTime - second, 0, third - second) * GetSlowOrbitSpeed(1)
            + Math.Max(0, musicTime - third) * GetSlowOrbitSpeed(2);
        return FirstStarburstAngle + slowAngle
            + (index == 2 ? MathHelper.TwoPi * GetRelativeOrbitTurns(musicTime) : 0f);
    }
    public int StarburstIndex
    {
        get
        {
            int time = MasterBehavior?.Phase2MusicElapsedTime ?? 0;
            if (time < FirstStarburstMusicTime - StarburstWarningDuration || time >= StarburstEndMusicTime)
                return -1;
            return time < ThirdStarburstMusicTime - StarburstWarningDuration
                ? (time < SecondStarburstMusicTime - StarburstWarningDuration ? 0 : 1)
                : 2 + Math.Max(0, (time - ThirdStarburstMusicTime + StarburstWarningDuration) / RapidStarburstInterval);
        }
    }

    public int GetStarburstTime(int index) => index < 2 ? (index == 0 ? FirstStarburstMusicTime : SecondStarburstMusicTime)
        : ThirdStarburstMusicTime + (index - 2) * RapidStarburstInterval;
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
            if (MasterBehavior is not { Mechanic2Active: true } || time >= StarburstEndMusicTime)
                return 0f;
            int next = time <= ThirdStarburstMusicTime
                ? (time <= FirstStarburstMusicTime ? 0 : time <= SecondStarburstMusicTime ? 1 : 2)
                : 2 + (int)MathF.Ceiling((time - ThirdStarburstMusicTime) / (float)RapidStarburstInterval);
            int nextTime = GetStarburstTime(next);
            // 快速连续星爆缩短蓄光，留出恢复原色的间隔。
            int chargeDuration = next > 2 ? Math.Min(StarburstChargeDuration, RapidStarburstInterval / 3)
                : StarburstChargeDuration;
            float charge = nextTime < StarburstEndMusicTime
                ? MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(1f - (nextTime - time) / (float)chargeDuration, 0f, 1f))
                : 0f;
            float recovery = next > 0
                ? 1f - MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(
                    (time - GetStarburstTime(next - 1)) / (float)StarburstColorRecoveryDuration, 0f, 1f))
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
        if (Main.dedServ || MasterBehavior is not { Mechanic2Active: true } || StarburstIndex < 0)
            return;
        int age = StarburstAge;
        if (age == 0)
            SoundEngine.PlaySound(CoinJudgment.ExplosionSound, PredictStarburstPosition(StarburstIndex));
        if (age < 0 || age >= StarburstDamageDuration)
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
