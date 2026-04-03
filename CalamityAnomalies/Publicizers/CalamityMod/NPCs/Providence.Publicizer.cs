using CalamityMod.NPCs.Providence;

namespace CalamityAnomalies.Publicizers.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record Providence_Publicizer(Providence Source) : Publicizer<Providence>(Source)
{
    // AIState (instance property)
    public static readonly PropertyInfo i_f_AIState = GetInstanceProperty("AIState");
    public float AIState
    {
        get => (float)i_f_AIState.GetValue(Source);
        set => i_f_AIState.SetValue(Source, value);
    }

    // text (instance field)
    public static readonly FieldInfo i_f_text = GetInstanceField("text");
    public bool text
    {
        get => (bool)i_f_text.GetValue(Source);
        set => i_f_text.SetValue(Source, value);
    }

    // useDefenseFrames (instance field)
    public static readonly FieldInfo i_f_useDefenseFrames = GetInstanceField("useDefenseFrames");
    public bool useDefenseFrames
    {
        get => (bool)i_f_useDefenseFrames.GetValue(Source);
        set => i_f_useDefenseFrames.SetValue(Source, value);
    }

    // bossLife (instance field)
    public static readonly FieldInfo i_f_bossLife = GetInstanceField("bossLife");
    public float bossLife
    {
        get => (float)i_f_bossLife.GetValue(Source);
        set => i_f_bossLife.SetValue(Source, value);
    }

    // biomeType (instance field)
    public static readonly FieldInfo i_f_biomeType = GetInstanceField("biomeType");
    public int biomeType
    {
        get => (int)i_f_biomeType.GetValue(Source);
        set => i_f_biomeType.SetValue(Source, value);
    }

    // flightPath (instance field)
    public static readonly FieldInfo i_f_flightPath = GetInstanceField("flightPath");
    public int flightPath
    {
        get => (int)i_f_flightPath.GetValue(Source);
        set => i_f_flightPath.SetValue(Source, value);
    }

    // phaseChange (instance field)
    public static readonly FieldInfo i_f_phaseChange = GetInstanceField("phaseChange");
    public int phaseChange
    {
        get => (int)i_f_phaseChange.GetValue(Source);
        set => i_f_phaseChange.SetValue(Source, value);
    }

    // frameUsed (instance field)
    public static readonly FieldInfo i_f_frameUsed = GetInstanceField("frameUsed");
    public int frameUsed
    {
        get => (int)i_f_frameUsed.GetValue(Source);
        set => i_f_frameUsed.SetValue(Source, value);
    }

    // healTimer (instance field)
    public static readonly FieldInfo i_f_healTimer = GetInstanceField("healTimer");
    public int healTimer
    {
        get => (int)i_f_healTimer.GetValue(Source);
        set => i_f_healTimer.SetValue(Source, value);
    }

    // challenge (instance field)
    public static readonly FieldInfo i_f_challenge = GetInstanceField("challenge");
    public bool challenge
    {
        get => (bool)i_f_challenge.GetValue(Source);
        set => i_f_challenge.SetValue(Source, value);
    }

    // hasTakenDaytimeDamage (instance field)
    public static readonly FieldInfo i_f_hasTakenDaytimeDamage = GetInstanceField("hasTakenDaytimeDamage");
    public bool hasTakenDaytimeDamage
    {
        get => (bool)i_f_hasTakenDaytimeDamage.GetValue(Source);
        set => i_f_hasTakenDaytimeDamage.SetValue(Source, value);
    }

    // TimeForStarDespawn (const)
    public const float TimeForStarDespawn = 120f;

    // TimeForShieldDespawn (const)
    public const float TimeForShieldDespawn = 120f;

    // CalculateBurnIntensity (instance method)
    public static readonly MethodInfo i_m_CalculateBurnIntensity = GetInstanceMethod("CalculateBurnIntensity");
    public delegate float Orig_CalculateBurnIntensity(Providence self, float attackDelayAfterCocoon);
    public static readonly Orig_CalculateBurnIntensity i_d_CalculateBurnIntensity = i_m_CalculateBurnIntensity.CreateDelegate<Orig_CalculateBurnIntensity>();
    public float CalculateBurnIntensity(float attackDelayAfterCocoon = 1f) => i_d_CalculateBurnIntensity(Source, attackDelayAfterCocoon);

    // DespawnSpecificProjectiles (instance method)
    public static readonly MethodInfo i_m_DespawnSpecificProjectiles = GetInstanceMethod("DespawnSpecificProjectiles");
    public delegate void Orig_DespawnSpecificProjectiles(Providence self, bool dying);
    public static readonly Orig_DespawnSpecificProjectiles i_d_DespawnSpecificProjectiles = i_m_DespawnSpecificProjectiles.CreateDelegate<Orig_DespawnSpecificProjectiles>();
    public void DespawnSpecificProjectiles(bool dying = false) => i_d_DespawnSpecificProjectiles(Source, dying);

    // DoDeathAnimation (instance method)
    public static readonly MethodInfo i_m_DoDeathAnimation = GetInstanceMethod("DoDeathAnimation");
    public delegate void Orig_DoDeathAnimation(Providence self);
    public static readonly Orig_DoDeathAnimation i_d_DoDeathAnimation = i_m_DoDeathAnimation.CreateDelegate<Orig_DoDeathAnimation>();
    public void DoDeathAnimation() => i_d_DoDeathAnimation(Source);

    // SpawnLootBox (instance method)
    public static readonly MethodInfo i_m_SpawnLootBox = GetInstanceMethod("SpawnLootBox");
    public delegate void Orig_SpawnLootBox(Providence self);
    public static readonly Orig_SpawnLootBox i_d_SpawnLootBox = i_m_SpawnLootBox.CreateDelegate<Orig_SpawnLootBox>();
    public void SpawnLootBox() => i_d_SpawnLootBox(Source);
}

public record ProvSpawnOffense_Publicizer(ProvSpawnOffense Source) : Publicizer<ProvSpawnOffense>(Source)
{
    // start (instance field)
    public static readonly FieldInfo i_f_start = GetInstanceField("start");
    public bool start
    {
        get => (bool)i_f_start.GetValue(Source);
        set => i_f_start.SetValue(Source, value);
    }
}

public record ProvSpawnDefense_Publicizer(ProvSpawnDefense Source) : Publicizer<ProvSpawnDefense>(Source)
{
    // start (instance field)
    public static readonly FieldInfo i_f_start = GetInstanceField("start");
    public bool start
    {
        get => (bool)i_f_start.GetValue(Source);
        set => i_f_start.SetValue(Source, value);
    }
}

public record ProvSpawnHealer_Publicizer(ProvSpawnHealer Source) : Publicizer<ProvSpawnHealer>(Source)
{
    // start (instance field)
    public static readonly FieldInfo i_f_start = GetInstanceField("start");
    public bool start
    {
        get => (bool)i_f_start.GetValue(Source);
        set => i_f_start.SetValue(Source, value);
    }
}
