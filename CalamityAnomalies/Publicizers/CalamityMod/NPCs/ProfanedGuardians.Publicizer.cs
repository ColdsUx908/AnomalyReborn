using CalamityMod.NPCs.ProfanedGuardians;

namespace CalamityAnomalies.Publicizers.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record ProfanedGuardianCommander_Publicizer(ProfanedGuardianCommander Source) : Publicizer<ProfanedGuardianCommander>(Source)
{
    // spearType (instance field)
    public static readonly FieldInfo i_f_spearType = GetInstanceField("spearType");
    public int spearType
    {
        get => (int)i_f_spearType.GetValue(Source);
        set => i_f_spearType.SetValue(Source, value);
    }

    // healTimer (instance field)
    public static readonly FieldInfo i_f_healTimer = GetInstanceField("healTimer");
    public int healTimer
    {
        get => (int)i_f_healTimer.GetValue(Source);
        set => i_f_healTimer.SetValue(Source, value);
    }

    // TimeForShieldDespawn (const)
    public const float TimeForShieldDespawn = 120f;
}

public record ProfanedGuardianDefender_Publicizer(ProfanedGuardianDefender Source) : Publicizer<ProfanedGuardianDefender>(Source)
{
    // healTimer (instance field)
    public static readonly FieldInfo i_f_healTimer = GetInstanceField("healTimer");
    public int healTimer
    {
        get => (int)i_f_healTimer.GetValue(Source);
        set => i_f_healTimer.SetValue(Source, value);
    }

    // TimeForShieldDespawn (const)
    public const float TimeForShieldDespawn = 120f;
}

public record ProfanedGuardianHealer_Publicizer(ProfanedGuardianHealer Source) : Publicizer<ProfanedGuardianHealer>(Source)
{
    // AIState (instance property)
    public static readonly PropertyInfo i_f_AIState = GetInstanceProperty("AIState");
    public float AIState
    {
        get => (float)i_f_AIState.GetValue(Source);
        set => i_f_AIState.SetValue(Source, value);
    }

    // AITimer (instance property)
    public static readonly PropertyInfo i_f_AITimer = GetInstanceProperty("AITimer");
    public float AITimer
    {
        get => (float)i_f_AITimer.GetValue(Source);
        set => i_f_AITimer.SetValue(Source, value);
    }
}

public record ProfanedRocks_Publicizer(ProfanedRocks Source) : Publicizer<ProfanedRocks>(Source)
{
    // start (instance field)
    public static readonly FieldInfo i_f_start = GetInstanceField("start");
    public bool start
    {
        get => (bool)i_f_start.GetValue(Source);
        set => i_f_start.SetValue(Source, value);
    }

    // MinDistance (const)
    public const double MinDistance = 200D;

    // distance (instance field)
    public static readonly FieldInfo i_f_distance = GetInstanceField("distance");
    public double distance
    {
        get => (double)i_f_distance.GetValue(Source);
        set => i_f_distance.SetValue(Source, value);
    }

    // MinMaxDistance (const)
    public const double MinMaxDistance = 300D;
}
