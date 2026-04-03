using CalamityMod.NPCs.Polterghast;

namespace CalamityAnomalies.Publicizers.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record Polterghast_Publicizer(Polterghast Source) : Publicizer<Polterghast>(Source)
{
    // DespawnTimerMax (const)
    public const int DespawnTimerMax = 900;

    // despawnTimer (instance field)
    public static readonly FieldInfo i_f_despawnTimer = GetInstanceField("despawnTimer");
    public int despawnTimer
    {
        get => (int)i_f_despawnTimer.GetValue(Source);
        set => i_f_despawnTimer.SetValue(Source, value);
    }

    // soundTimer (instance field)
    public static readonly FieldInfo i_f_soundTimer = GetInstanceField("soundTimer");
    public int soundTimer
    {
        get => (int)i_f_soundTimer.GetValue(Source);
        set => i_f_soundTimer.SetValue(Source, value);
    }

    // reachedChargingPoint (instance field)
    public static readonly FieldInfo i_f_reachedChargingPoint = GetInstanceField("reachedChargingPoint");
    public bool reachedChargingPoint
    {
        get => (bool)i_f_reachedChargingPoint.GetValue(Source);
        set => i_f_reachedChargingPoint.SetValue(Source, value);
    }

    // threeAM (instance field)
    public static readonly FieldInfo i_f_threeAM = GetInstanceField("threeAM");
    public bool threeAM
    {
        get => (bool)i_f_threeAM.GetValue(Source);
        set => i_f_threeAM.SetValue(Source, value);
    }
}

public record PolterghastHook_Publicizer(PolterghastHook Source) : Publicizer<PolterghastHook>(Source)
{
    // despawnTimer (instance field)
    public static readonly FieldInfo i_f_despawnTimer = GetInstanceField("despawnTimer");
    public int despawnTimer
    {
        get => (int)i_f_despawnTimer.GetValue(Source);
        set => i_f_despawnTimer.SetValue(Source, value);
    }

    // phase2 (instance field)
    public static readonly FieldInfo i_f_phase2 = GetInstanceField("phase2");
    public bool phase2
    {
        get => (bool)i_f_phase2.GetValue(Source);
        set => i_f_phase2.SetValue(Source, value);
    }

    // Movement (instance method)
    public static readonly MethodInfo i_m_Movement = GetInstanceMethod("Movement");
    public delegate void Orig_Movement(PolterghastHook self, bool phase2, bool expertMode, bool revenge, bool death, bool speedBoost, bool despawnBoost, float lifeRatio, float tileEnrageMult, Player player);
    public static readonly Orig_Movement i_d_Movement = i_m_Movement.CreateDelegate<Orig_Movement>();
    public void Movement(bool phase2, bool expertMode, bool revenge, bool death, bool speedBoost, bool despawnBoost, float lifeRatio, float tileEnrageMult, Player player) => i_d_Movement(Source, phase2, expertMode, revenge, death, speedBoost, despawnBoost, lifeRatio, tileEnrageMult, player);
}

public record PolterPhantom_Publicizer(PolterPhantom Source) : Publicizer<PolterPhantom>(Source)
{
    // despawnTimer (instance field)
    public static readonly FieldInfo i_f_despawnTimer = GetInstanceField("despawnTimer");
    public int despawnTimer
    {
        get => (int)i_f_despawnTimer.GetValue(Source);
        set => i_f_despawnTimer.SetValue(Source, value);
    }

    // reachedChargingPoint (instance field)
    public static readonly FieldInfo i_f_reachedChargingPoint = GetInstanceField("reachedChargingPoint");
    public bool reachedChargingPoint
    {
        get => (bool)i_f_reachedChargingPoint.GetValue(Source);
        set => i_f_reachedChargingPoint.SetValue(Source, value);
    }
}

public record PhantomFuckYou_Publicizer(PhantomFuckYou Source) : Publicizer<PhantomFuckYou>(Source)
{
    // start (instance field)
    public static readonly FieldInfo i_f_start = GetInstanceField("start");
    public bool start
    {
        get => (bool)i_f_start.GetValue(Source);
        set => i_f_start.SetValue(Source, value);
    }
}
