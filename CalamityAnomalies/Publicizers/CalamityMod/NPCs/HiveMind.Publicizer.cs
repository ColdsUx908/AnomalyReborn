using CalamityMod.NPCs.HiveMind;

namespace CalamityAnomalies.Publicizers.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record HiveMind_Publicizer(HiveMind Source) : Publicizer<HiveMind>(Source)
{
    // LoadHeadIcons (static method)
    public static readonly MethodInfo s_m_LoadHeadIcons = GetStaticMethod("LoadHeadIcons");
    public delegate void Orig_LoadHeadIcons();
    public static readonly Orig_LoadHeadIcons s_d_LoadHeadIcons = s_m_LoadHeadIcons.CreateDelegate<Orig_LoadHeadIcons>();
    public static void LoadHeadIcons() => s_d_LoadHeadIcons();

    // biomeEnrageTimer (instance field)
    public static readonly FieldInfo i_f_biomeEnrageTimer = GetInstanceField("biomeEnrageTimer");
    public int biomeEnrageTimer
    {
        get => (int)i_f_biomeEnrageTimer.GetValue(Source);
        set => i_f_biomeEnrageTimer.SetValue(Source, value);
    }

    // burrowTimer (instance field)
    public static readonly FieldInfo i_f_burrowTimer = GetInstanceField("burrowTimer");
    public int burrowTimer
    {
        get => (int)i_f_burrowTimer.GetValue(Source);
        set => i_f_burrowTimer.SetValue(Source, value);
    }

    // minimumDriftTime (instance field)
    public static readonly FieldInfo i_f_minimumDriftTime = GetInstanceField("minimumDriftTime");
    public int minimumDriftTime
    {
        get => (int)i_f_minimumDriftTime.GetValue(Source);
        set => i_f_minimumDriftTime.SetValue(Source, value);
    }

    // teleportRadius (instance field)
    public static readonly FieldInfo i_f_teleportRadius = GetInstanceField("teleportRadius");
    public int teleportRadius
    {
        get => (int)i_f_teleportRadius.GetValue(Source);
        set => i_f_teleportRadius.SetValue(Source, value);
    }

    // decelerationTime (instance field)
    public static readonly FieldInfo i_f_decelerationTime = GetInstanceField("decelerationTime");
    public int decelerationTime
    {
        get => (int)i_f_decelerationTime.GetValue(Source);
        set => i_f_decelerationTime.SetValue(Source, value);
    }

    // reelbackFade (instance field)
    public static readonly FieldInfo i_f_reelbackFade = GetInstanceField("reelbackFade");
    public int reelbackFade
    {
        get => (int)i_f_reelbackFade.GetValue(Source);
        set => i_f_reelbackFade.SetValue(Source, value);
    }

    // arcTime (instance field)
    public static readonly FieldInfo i_f_arcTime = GetInstanceField("arcTime");
    public float arcTime
    {
        get => (float)i_f_arcTime.GetValue(Source);
        set => i_f_arcTime.SetValue(Source, value);
    }

    // driftSpeed (instance field)
    public static readonly FieldInfo i_f_driftSpeed = GetInstanceField("driftSpeed");
    public float driftSpeed
    {
        get => (float)i_f_driftSpeed.GetValue(Source);
        set => i_f_driftSpeed.SetValue(Source, value);
    }

    // driftBoost (instance field)
    public static readonly FieldInfo i_f_driftBoost = GetInstanceField("driftBoost");
    public float driftBoost
    {
        get => (float)i_f_driftBoost.GetValue(Source);
        set => i_f_driftBoost.SetValue(Source, value);
    }

    // lungeDelay (instance field)
    public static readonly FieldInfo i_f_lungeDelay = GetInstanceField("lungeDelay");
    public int lungeDelay
    {
        get => (int)i_f_lungeDelay.GetValue(Source);
        set => i_f_lungeDelay.SetValue(Source, value);
    }

    // lungeTime (instance field)
    public static readonly FieldInfo i_f_lungeTime = GetInstanceField("lungeTime");
    public int lungeTime
    {
        get => (int)i_f_lungeTime.GetValue(Source);
        set => i_f_lungeTime.SetValue(Source, value);
    }

    // lungeFade (instance field)
    public static readonly FieldInfo i_f_lungeFade = GetInstanceField("lungeFade");
    public int lungeFade
    {
        get => (int)i_f_lungeFade.GetValue(Source);
        set => i_f_lungeFade.SetValue(Source, value);
    }

    // lungeRots (instance field)
    public static readonly FieldInfo i_f_lungeRots = GetInstanceField("lungeRots");
    public double lungeRots
    {
        get => (double)i_f_lungeRots.GetValue(Source);
        set => i_f_lungeRots.SetValue(Source, value);
    }

    // dashStarted (instance field)
    public static readonly FieldInfo i_f_dashStarted = GetInstanceField("dashStarted");
    public bool dashStarted
    {
        get => (bool)i_f_dashStarted.GetValue(Source);
        set => i_f_dashStarted.SetValue(Source, value);
    }

    // vileSpitFireRate (instance field)
    public static readonly FieldInfo i_f_vileSpitFireRate = GetInstanceField("vileSpitFireRate");
    public int vileSpitFireRate
    {
        get => (int)i_f_vileSpitFireRate.GetValue(Source);
        set => i_f_vileSpitFireRate.SetValue(Source, value);
    }

    // phase2timer (instance field)
    public static readonly FieldInfo i_f_phase2timer = GetInstanceField("phase2timer");
    public int phase2timer
    {
        get => (int)i_f_phase2timer.GetValue(Source);
        set => i_f_phase2timer.SetValue(Source, value);
    }

    // rotationDirection (instance field)
    public static readonly FieldInfo i_f_rotationDirection = GetInstanceField("rotationDirection");
    public int rotationDirection
    {
        get => (int)i_f_rotationDirection.GetValue(Source);
        set => i_f_rotationDirection.SetValue(Source, value);
    }

    // rotation (instance field)
    public static readonly FieldInfo i_f_rotation = GetInstanceField("rotation");
    public double rotation
    {
        get => (double)i_f_rotation.GetValue(Source);
        set => i_f_rotation.SetValue(Source, value);
    }

    // rotationIncrement (instance field)
    public static readonly FieldInfo i_f_rotationIncrement = GetInstanceField("rotationIncrement");
    public double rotationIncrement
    {
        get => (double)i_f_rotationIncrement.GetValue(Source);
        set => i_f_rotationIncrement.SetValue(Source, value);
    }

    // state (instance field)
    public static readonly FieldInfo i_f_state = GetInstanceField("state");
    public int state
    {
        get => (int)i_f_state.GetValue(Source);
        set => i_f_state.SetValue(Source, value);
    }

    // previousState (instance field)
    public static readonly FieldInfo i_f_previousState = GetInstanceField("previousState");
    public int previousState
    {
        get => (int)i_f_previousState.GetValue(Source);
        set => i_f_previousState.SetValue(Source, value);
    }

    // nextState (instance field)
    public static readonly FieldInfo i_f_nextState = GetInstanceField("nextState");
    public int nextState
    {
        get => (int)i_f_nextState.GetValue(Source);
        set => i_f_nextState.SetValue(Source, value);
    }

    // reelCount (instance field)
    public static readonly FieldInfo i_f_reelCount = GetInstanceField("reelCount");
    public int reelCount
    {
        get => (int)i_f_reelCount.GetValue(Source);
        set => i_f_reelCount.SetValue(Source, value);
    }

    // deceleration (instance field)
    public static readonly FieldInfo i_f_deceleration = GetInstanceField("deceleration");
    public Vector2 deceleration
    {
        get => (Vector2)i_f_deceleration.GetValue(Source);
        set => i_f_deceleration.SetValue(Source, value);
    }

    // frameX (instance field)
    public static readonly FieldInfo i_f_frameX = GetInstanceField("frameX");
    public int frameX
    {
        get => (int)i_f_frameX.GetValue(Source);
        set => i_f_frameX.SetValue(Source, value);
    }

    // frameY (instance field)
    public static readonly FieldInfo i_f_frameY = GetInstanceField("frameY");
    public int frameY
    {
        get => (int)i_f_frameY.GetValue(Source);
        set => i_f_frameY.SetValue(Source, value);
    }

    // maxFramesX_Phase2 (const)
    public const int maxFramesX_Phase2 = 2;

    // maxFramesY_Phase2 (const)
    public const int maxFramesY_Phase2 = 8;

    // height_Phase2 (const)
    public const int height_Phase2 = 142;

    // SpawnStuff (instance method)
    public static readonly MethodInfo i_m_SpawnStuff = GetInstanceMethod("SpawnStuff");
    public delegate void Orig_SpawnStuff(HiveMind self);
    public static readonly Orig_SpawnStuff i_d_SpawnStuff = i_m_SpawnStuff.CreateDelegate<Orig_SpawnStuff>();
    public void SpawnStuff() => i_d_SpawnStuff(Source);

    // ReelBack (instance method)
    public static readonly MethodInfo i_m_ReelBack = GetInstanceMethod("ReelBack");
    public delegate void Orig_ReelBack(HiveMind self);
    public static readonly Orig_ReelBack i_d_ReelBack = i_m_ReelBack.CreateDelegate<Orig_ReelBack>();
    public void ReelBack() => i_d_ReelBack(Source);
}

public record HiveBlob_Publicizer(HiveBlob Source) : Publicizer<HiveBlob>(Source)
{
    // ShootGateValue (const)
    public const float ShootGateValue = 240f;

    // TelegraphDuration (const)
    public const float TelegraphDuration = 120f;

    // ShowTelegraphValue (const)
    public const float ShowTelegraphValue = ShootGateValue - TelegraphDuration;
}
