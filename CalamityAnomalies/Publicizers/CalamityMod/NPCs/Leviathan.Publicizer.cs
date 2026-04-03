using CalamityMod.NPCs.Leviathan;

namespace CalamityAnomalies.Publicizers.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record Leviathan_Publicizer(Leviathan Source) : Publicizer<Leviathan>(Source)
{
    // biomeEnrageTimer (instance field)
    public static readonly FieldInfo i_f_biomeEnrageTimer = GetInstanceField("biomeEnrageTimer");
    public int biomeEnrageTimer
    {
        get => (int)i_f_biomeEnrageTimer.GetValue(Source);
        set => i_f_biomeEnrageTimer.SetValue(Source, value);
    }

    // counter (instance field)
    public static readonly FieldInfo i_f_counter = GetInstanceField("counter");
    public int counter
    {
        get => (int)i_f_counter.GetValue(Source);
        set => i_f_counter.SetValue(Source, value);
    }

    // initialised (instance field)
    public static readonly FieldInfo i_f_initialised = GetInstanceField("initialised");
    public bool initialised
    {
        get => (bool)i_f_initialised.GetValue(Source);
        set => i_f_initialised.SetValue(Source, value);
    }

    // gfbAnaSummoned (instance field)
    public static readonly FieldInfo i_f_gfbAnaSummoned = GetInstanceField("gfbAnaSummoned");
    public bool gfbAnaSummoned
    {
        get => (bool)i_f_gfbAnaSummoned.GetValue(Source);
        set => i_f_gfbAnaSummoned.SetValue(Source, value);
    }

    // soundDelay (instance field)
    public static readonly FieldInfo i_f_soundDelay = GetInstanceField("soundDelay");
    public int soundDelay
    {
        get => (int)i_f_soundDelay.GetValue(Source);
        set => i_f_soundDelay.SetValue(Source, value);
    }

    // extrapitch (instance field)
    public static readonly FieldInfo i_f_extrapitch = GetInstanceField("extrapitch");
    public float extrapitch
    {
        get => (float)i_f_extrapitch.GetValue(Source);
        set => i_f_extrapitch.SetValue(Source, value);
    }
}

public record Anahita_Publicizer(Anahita Source) : Publicizer<Anahita>(Source)
{
    // biomeEnrageTimer (instance field)
    public static readonly FieldInfo i_f_biomeEnrageTimer = GetInstanceField("biomeEnrageTimer");
    public int biomeEnrageTimer
    {
        get => (int)i_f_biomeEnrageTimer.GetValue(Source);
        set => i_f_biomeEnrageTimer.SetValue(Source, value);
    }

    // spawnedLevi (instance field)
    public static readonly FieldInfo i_f_spawnedLevi = GetInstanceField("spawnedLevi");
    public bool spawnedLevi
    {
        get => (bool)i_f_spawnedLevi.GetValue(Source);
        set => i_f_spawnedLevi.SetValue(Source, value);
    }

    // forceChargeFrames (instance field)
    public static readonly FieldInfo i_f_forceChargeFrames = GetInstanceField("forceChargeFrames");
    public bool forceChargeFrames
    {
        get => (bool)i_f_forceChargeFrames.GetValue(Source);
        set => i_f_forceChargeFrames.SetValue(Source, value);
    }

    // frameUsed (instance field)
    public static readonly FieldInfo i_f_frameUsed = GetInstanceField("frameUsed");
    public int frameUsed
    {
        get => (int)i_f_frameUsed.GetValue(Source);
        set => i_f_frameUsed.SetValue(Source, value);
    }

    // ChargeRotation (instance method)
    public static readonly MethodInfo i_m_ChargeRotation = GetInstanceMethod("ChargeRotation");
    public delegate void Orig_ChargeRotation(Anahita self, Player player);
    public static readonly Orig_ChargeRotation i_d_ChargeRotation = i_m_ChargeRotation.CreateDelegate<Orig_ChargeRotation>();
    public void ChargeRotation(Player player) => i_d_ChargeRotation(Source, player);

    // ChargeLocation (instance method)
    public static readonly MethodInfo i_m_ChargeLocation = GetInstanceMethod("ChargeLocation");
    public delegate void Orig_ChargeLocation(Anahita self, Player player, bool leviAlive, bool revenge);
    public static readonly Orig_ChargeLocation i_d_ChargeLocation = i_m_ChargeLocation.CreateDelegate<Orig_ChargeLocation>();
    public void ChargeLocation(Player player, bool leviAlive, bool revenge) => i_d_ChargeLocation(Source, player, leviAlive, revenge);
}