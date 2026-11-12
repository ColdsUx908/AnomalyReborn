namespace Anomalies.Bosses.Skeletron;

public sealed partial class SkeletronHead
{
    // 阶段与退场
    public const float Phase2_2LifeRatioAnomaly = 0.5f;
    public const float Phase2_2LifeRatioUltra = 0.6f;
    public static float Phase2_2LifeRatio => Ultra ? Phase2_2LifeRatioUltra : Phase2_2LifeRatioAnomaly;
    public const float Phase3LifeRatioAnomaly = 0f;
    public const float Phase3LifeRatioUltra = 0.1f;
    public static float Phase3LifeRatio => Ultra ? Phase3LifeRatioUltra : Phase3LifeRatioAnomaly;
    public const float FinalLifeRatio = 0.03f;
    public const int FinalStillDuration = 60;
    public const int FinalChargeDuration = 120;
    public const float FinalChaseSpeed = 8f;
    public const float FinalSkullLifeRatio = 0.02f;
    public const float FinalGuardianLifeRatio = 0.01f;
    public static int FinalSkullCount => Ultra ? 5 : 3;
    public const float FinalSkullSpeed = 15f;
    public static int FinalGuardianCount => Ultra ? 9 : 7;
    public const int FinalGuardianInterval = Phase3Patterns.SkullInterval / 6;
    public static float FinalGuardianSpeed => Ultra ? 16f : 14.4f;
    public const float Phase3HealLifeRatio = 0.33f;
    public const int Phase3StillDuration = 60;
    public const int Phase3HealDuration = 60;
    public const int Phase3CenterMoveDelay = 30;
    public const int Phase3CenterMoveDuration = 120;
    public const int Phase3ApproachDuration = 63;
    public const int Phase3BrakeDuration = 30;
    public const float Phase3ApproachHeight = 320f;
    public const float Phase3ApproachSpeed = 40f;

    public const float DespawnDistance = 8000f;

    // 手部
    public const int WithHandsDefense = 9999;
    public static float UpperHandNegativeSideInitialTimer => Ultra ? -75f : 0f;
    public static float UpperHandPositiveSideInitialTimer => Ultra ? 75f : 150f;

    // 悬停与头骨
    public const int WithHandsDuration = 480;
    public const int MinNormalSkullDuration = 120;
    public const int MaxNormalSkullDuration = 300;
    public const int MinRapidSkullDuration = 180;
    public const int MaxRapidSkullDuration = 300;
    public const int QuietDuration = 60;
    public const int PostSpinHoverQuietDuration = 30;
    public static float HoverBaseAccelerationX => Ultra ? 0.21f : 0.16f;
    public static float HoverBaseAccelerationY => Ultra ? 0.07f : 0.06f;
    public static float HoverLowHealthAccelerationX => Ultra ? 0f : 0.08f;
    public static float HoverLowHealthAccelerationY => Ultra ? 0f : 0.04f;
    public const int SkullInterval = 30;
    public static int EnhancedSkullInterval => Ultra ? 10 : 15;
    public static int HoverDoubleSkullChanceDenominator => Ultra ? 3 : 0;
    public const float HeadSkullSpeed = 20f;

    private static ProjectileDamageContainer _SkullDamage = new(30, 60, 90, 120, 90, 120);
    public static int SkullDamage => _SkullDamage.Value;
    public static float HoverCrossboneSpeed = Ultra ? 16.5f : 15f;
    public static float CrossboneMaxSpeed => Skull.MaxSpeed;

    // 旋转与预警
    public const float ChargeGateValue = 600f;
    public const float ChargeTelegraphTime = 120f;
    public const float SpinDamageMult = 1.3f;
    public const float SpinSpeed = 0.3f;
    public const float SpinBaseMoveSpeed = 10f;
    public const float SpinMinMoveSpeed = 7.5f;
    public const float SpinMaxMoveSpeed = 40f;
    public const float SpinSlowdownMinDistance = 80f;
    public const float SpinSlowdownEndDistance = 160f;
    public const float SpinAccelerationStartDistance = 320f;
    public const float SpinAccelerationMaxDistance = 1280f;
    public const int HandsClearedHoverQuietDuration = 90;
    public const int HandsClearedMinSpinRemaining = 120;
    public const int HandsClearedSpinDuration = 240;
    public const int EnhancedSpinDuration = 360;
    public const int WarningSpinQuietDuration = 60;
    public static int SkullWarningInterval => Ultra ? 60 : 75;
    public static int SkullWarningCount => Ultra ? 4 : 3;
    public static int Phase22SkullWarningCount => Ultra ? 6 : 5;
    public const int ForwardSkullSpinDuration = 375;
    public const int ForwardSkullInterval = 75;
    public static int ForwardSkullCount => Ultra ? 12 : 8;
    public static float ForwardSkullArc => Ultra ? 60f : 40f;
    public const float ForwardSkullRadius = 1200f;

    // 三连旋冲与环形散射
    public const float SpinChargeDuration = 210f;
    public const int ChargesPerRound = 3;
    public const int ChargeRetreatDuration = 30;
    public const int MaxChargeRetreatDuration = 120;
    public static float RetreatSpeedMultiplier => Ultra ? 1.2f : 1f;
    public const float ChargeDistance = 320f;
    public static float ChargeTrackingSpeedMultiplier => Ultra ? 1.3f : 1f;
    public static int ChargeEndSkullCount => Ultra ? 8 : 4;
    public const float ChargeEndSkullSpacing = MathHelper.PiOver4;
    public const float ChargeEndSkullSpeed = 6f;
    public const float RadialSkullMaxSpeed = 12f;
    public const float RadialSkullAcceleration = 1.008f;
    public const int RadialSkullHomingStartFrame = 16;
    public const int RadialSkullHomingDuration = 84;
    public static int FinalChargeFanCount => Ultra ? 5 : 3;
    public static float FinalChargeFanHalfAngle => Ultra ? 80f : 60f;
    public static float FinalChargeSkullSpeed => Ultra ? 8f : ChargeEndSkullSpeed;
    public static float FinalChargeSkullMaxSpeed => Skull.MaxSpeed;
    public const float FinalChargeSpawnOffset = 5f;

    // Shared with LegendaryNight's accelerating spread.
    public const int MaximumSpreadCount = 36;
    public const float MaximumSpreadDegrees = 180f;
}
