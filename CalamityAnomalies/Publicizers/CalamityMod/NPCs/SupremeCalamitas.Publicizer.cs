using CalamityMod.NPCs.SupremeCalamitas;

namespace CalamityAnomalies.Publicizers.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record SupremeCalamitas_Publicizer(SupremeCalamitas Source) : Publicizer<SupremeCalamitas>(Source)
{
    // PermafrostPhotonRipperDashVelocity (const)
    public const float PermafrostPhotonRipperDashVelocity = 6f;

    // PermafrostPhotonRipperMinDistanceFromTarget (const)
    public const float PermafrostPhotonRipperMinDistanceFromTarget = 64f;

    // PermafrostPhotonRipperDashAcceleration (const)
    public const float PermafrostPhotonRipperDashAcceleration = 0.3f;

    // GiveUpCounterMax (const)
    public const int GiveUpCounterMax = 1200;

    // musicSyncCounterMax (const)
    public const int musicSyncCounterMax = 2082;

    // LoadHeadIcons (static method)
    public static readonly MethodInfo s_m_LoadHeadIcons = GetStaticMethod("LoadHeadIcons");
    public delegate void Orig_LoadHeadIcons();
    public static readonly Orig_LoadHeadIcons s_d_LoadHeadIcons = s_m_LoadHeadIcons.CreateDelegate<Orig_LoadHeadIcons>();
    public static void LoadHeadIcons() => s_d_LoadHeadIcons();
}

public record SepulcherHead_Publicizer(SepulcherHead Source) : Publicizer<SepulcherHead>(Source)
{
    // minLength (const)
    public const int minLength = 51;

    // maxLength (const)
    public const int maxLength = 52;

    // passedVar (instance field)
    public static readonly FieldInfo i_f_passedVar = GetInstanceField("passedVar");
    public float passedVar
    {
        get => (float)i_f_passedVar.GetValue(Source);
        set => i_f_passedVar.SetValue(Source, value);
    }

    // TailSpawned (instance field)
    public static readonly FieldInfo i_f_TailSpawned = GetInstanceField("TailSpawned");
    public bool TailSpawned
    {
        get => (bool)i_f_TailSpawned.GetValue(Source);
        set => i_f_TailSpawned.SetValue(Source, value);
    }

    // AttackCooldown (instance field)
    public static readonly FieldInfo i_f_AttackCooldown = GetInstanceField("AttackCooldown");
    public float AttackCooldown
    {
        get => (float)i_f_AttackCooldown.GetValue(Source);
        set => i_f_AttackCooldown.SetValue(Source, value);
    }
}

public record SepulcherBody_Publicizer(SepulcherBody Source) : Publicizer<SepulcherBody>(Source)
{
    // setAlpha (instance field)
    public static readonly FieldInfo i_f_setAlpha = GetInstanceField("setAlpha");
    public bool setAlpha
    {
        get => (bool)i_f_setAlpha.GetValue(Source);
        set => i_f_setAlpha.SetValue(Source, value);
    }
}

public record SepulcherBodyEnergyBall_Publicizer(SepulcherBodyEnergyBall Source) : Publicizer<SepulcherBodyEnergyBall>(Source)
{
    // setAlpha (instance field)
    public static readonly FieldInfo i_f_setAlpha = GetInstanceField("setAlpha");
    public bool setAlpha
    {
        get => (bool)i_f_setAlpha.GetValue(Source);
        set => i_f_setAlpha.SetValue(Source, value);
    }
}

public record SepulcherTail_Publicizer(SepulcherTail Source) : Publicizer<SepulcherTail>(Source)
{
    // setAlpha (instance field)
    public static readonly FieldInfo i_f_setAlpha = GetInstanceField("setAlpha");
    public bool setAlpha
    {
        get => (bool)i_f_setAlpha.GetValue(Source);
        set => i_f_setAlpha.SetValue(Source, value);
    }
}

public record SoulSeekerSupreme_Publicizer(SoulSeekerSupreme Source) : Publicizer<SoulSeekerSupreme>(Source)
{
    // timer (instance field)
    public static readonly FieldInfo i_f_timer = GetInstanceField("timer");
    public int timer
    {
        get => (int)i_f_timer.GetValue(Source);
        set => i_f_timer.SetValue(Source, value);
    }

    // start (instance field)
    public static readonly FieldInfo i_f_start = GetInstanceField("start");
    public bool start
    {
        get => (bool)i_f_start.GetValue(Source);
        set => i_f_start.SetValue(Source, value);
    }
}