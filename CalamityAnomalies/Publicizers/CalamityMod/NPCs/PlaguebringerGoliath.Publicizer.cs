using CalamityMod.NPCs.PlaguebringerGoliath;

namespace CalamityAnomalies.Publicizers.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record PlaguebringerGoliath_Publicizer(PlaguebringerGoliath Source) : Publicizer<PlaguebringerGoliath>(Source)
{
    // biomeEnrageTimer (instance field)
    public static readonly FieldInfo i_f_biomeEnrageTimer = GetInstanceField("biomeEnrageTimer");
    public int biomeEnrageTimer
    {
        get => (int)i_f_biomeEnrageTimer.GetValue(Source);
        set => i_f_biomeEnrageTimer.SetValue(Source, value);
    }

    // MissileAngleSpread (const)
    public const float MissileAngleSpread = 60;

    // MissileProjectiles (const)
    public const int MissileProjectiles = 8;

    // MissileCountdown (instance field)
    public static readonly FieldInfo i_f_MissileCountdown = GetInstanceField("MissileCountdown");
    public int MissileCountdown
    {
        get => (int)i_f_MissileCountdown.GetValue(Source);
        set => i_f_MissileCountdown.SetValue(Source, value);
    }

    // despawnTimer (instance field)
    public static readonly FieldInfo i_f_despawnTimer = GetInstanceField("despawnTimer");
    public int despawnTimer
    {
        get => (int)i_f_despawnTimer.GetValue(Source);
        set => i_f_despawnTimer.SetValue(Source, value);
    }

    // chargeDistance (instance field)
    public static readonly FieldInfo i_f_chargeDistance = GetInstanceField("chargeDistance");
    public int chargeDistance
    {
        get => (int)i_f_chargeDistance.GetValue(Source);
        set => i_f_chargeDistance.SetValue(Source, value);
    }

    // charging (instance field)
    public static readonly FieldInfo i_f_charging = GetInstanceField("charging");
    public bool charging
    {
        get => (bool)i_f_charging.GetValue(Source);
        set => i_f_charging.SetValue(Source, value);
    }

    // halfLife (instance field)
    public static readonly FieldInfo i_f_halfLife = GetInstanceField("halfLife");
    public bool halfLife
    {
        get => (bool)i_f_halfLife.GetValue(Source);
        set => i_f_halfLife.SetValue(Source, value);
    }

    // canDespawn (instance field)
    public static readonly FieldInfo i_f_canDespawn = GetInstanceField("canDespawn");
    public bool canDespawn
    {
        get => (bool)i_f_canDespawn.GetValue(Source);
        set => i_f_canDespawn.SetValue(Source, value);
    }

    // flyingFrame2 (instance field)
    public static readonly FieldInfo i_f_flyingFrame2 = GetInstanceField("flyingFrame2");
    public bool flyingFrame2
    {
        get => (bool)i_f_flyingFrame2.GetValue(Source);
        set => i_f_flyingFrame2.SetValue(Source, value);
    }

    // curTex (instance field)
    public static readonly FieldInfo i_f_curTex = GetInstanceField("curTex");
    public int curTex
    {
        get => (int)i_f_curTex.GetValue(Source);
        set => i_f_curTex.SetValue(Source, value);
    }

    // Movement (instance method)
    public static readonly MethodInfo i_m_Movement = GetInstanceMethod("Movement");
    public delegate void Orig_Movement(PlaguebringerGoliath self, float distanceAboveTarget, Player player, float enrageScale);
    public static readonly Orig_Movement i_d_Movement = i_m_Movement.CreateDelegate<Orig_Movement>();
    public void Movement(float distanceAboveTarget, Player player, float enrageScale) => i_d_Movement(Source, distanceAboveTarget, player, enrageScale);
}
