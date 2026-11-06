namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>跨机制星爆的数量、相对时长、音效和颜色；音乐绝对帧由时间轴提供。</summary>
public static class LegendaryNightStarburstParameters
{
    /// <summary>星爆数量。</summary>
    public const int StarburstCount = 12;
    /// <summary>星爆间隔，单位为游戏帧。</summary>
    public const int StarburstInterval = 17 * 60;
    /// <summary>快速星爆间隔，单位为游戏帧。</summary>
    public const int RapidStarburstInterval = 95;
    /// <summary>星爆预警持续时间，单位为游戏帧。</summary>
    public const int StarburstWarningDuration = 12;
    /// <summary>星爆音效提前时刻，单位为游戏帧。</summary>
    public const int StarburstSoundLeadTime = 6;
    /// <summary>星爆伤害持续时间，单位为游戏帧。</summary>
    public const int StarburstDamageDuration = 24;
    /// <summary>星爆渐隐持续时间，单位为游戏帧。</summary>
    public const int StarburstFadeDuration = 18;
    /// <summary>星爆蓄力持续时间，单位为游戏帧。</summary>
    public const int StarburstChargeDuration = 90;
    /// <summary>星爆颜色恢复持续时间，单位为游戏帧。</summary>
    public const int StarburstColorRecoveryDuration = 18;
    /// <summary>公转恢复持续时间，单位为游戏帧。</summary>
    public const int OrbitRecoveryDuration = 90;
    /// <summary>星爆音效。</summary>
    public static readonly SoundStyle StarburstSound = new(LegendaryNightHandler.LegendaryNightPath + "Attacks/Starburst/Starburst") { MaxInstances = 0 };
    /// <summary>快速星爆颜色。</summary>
    public static readonly Color FastStarburstColor = new(255, 40, 40);
    /// <summary>慢速星爆颜色。</summary>
    public static readonly Color SlowStarburstColor = new(255, 155, 30);
}