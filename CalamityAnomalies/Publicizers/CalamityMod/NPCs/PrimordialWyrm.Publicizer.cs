using CalamityMod.NPCs.PrimordialWyrm;

namespace CalamityAnomalies.Publicizers.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record PrimordialWyrmHead_Publicizer(PrimordialWyrmHead Source) : Publicizer<PrimordialWyrmHead>(Source)
{
    // baseDistance (const)
    public const float baseDistance = 1000f;

    // baseAttackTriggerDistance (const)
    public const float baseAttackTriggerDistance = 80f;

    // soundDistance (const)
    public const float soundDistance = 2800f;

    // minLength (const)
    public const int minLength = 40;

    // maxLength (const)
    public const int maxLength = 41;

    // TailSpawned (instance field)
    public static readonly FieldInfo i_f_TailSpawned = GetInstanceField("TailSpawned");
    public bool TailSpawned
    {
        get => (bool)i_f_TailSpawned.GetValue(Source);
        set => i_f_TailSpawned.SetValue(Source, value);
    }

    // rotationDirection (instance field)
    public static readonly FieldInfo i_f_rotationDirection = GetInstanceField("rotationDirection");
    public int rotationDirection
    {
        get => (int)i_f_rotationDirection.GetValue(Source);
        set => i_f_rotationDirection.SetValue(Source, value);
    }

    // chargeVelocityScalar (instance field)
    public static readonly FieldInfo i_f_chargeVelocityScalar = GetInstanceField("chargeVelocityScalar");
    public float chargeVelocityScalar
    {
        get => (float)i_f_chargeVelocityScalar.GetValue(Source);
        set => i_f_chargeVelocityScalar.SetValue(Source, value);
    }

    // fastChargeGateValue (const)
    public const float fastChargeGateValue = 120f;

    // ChargeDust (instance method)
    public static readonly MethodInfo i_m_ChargeDust = GetInstanceMethod("ChargeDust");
    public delegate void Orig_ChargeDust(PrimordialWyrmHead self, int dustAmt, float pie);
    public static readonly Orig_ChargeDust i_d_ChargeDust = i_m_ChargeDust.CreateDelegate<Orig_ChargeDust>();
    public void ChargeDust(int dustAmt, float pie) => i_d_ChargeDust(Source, dustAmt, pie);

    // CanMinionsDropThings (static method)
    public static readonly MethodInfo s_m_CanMinionsDropThings = GetStaticMethod("CanMinionsDropThings");
    public delegate bool Orig_CanMinionsDropThings();
    public static readonly Orig_CanMinionsDropThings s_d_CanMinionsDropThings = s_m_CanMinionsDropThings.CreateDelegate<Orig_CanMinionsDropThings>();
    public static bool CanMinionsDropThings() => s_d_CanMinionsDropThings();
}
