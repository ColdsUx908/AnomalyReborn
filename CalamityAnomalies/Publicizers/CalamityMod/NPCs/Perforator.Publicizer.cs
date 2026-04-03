using CalamityMod.NPCs.Perforator;

namespace CalamityAnomalies.Publicizers.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record PerforatorHive_Publicizer(PerforatorHive Source) : Publicizer<PerforatorHive>(Source)
{
    // biomeEnrageTimer (instance field)
    public static readonly FieldInfo i_f_biomeEnrageTimer = GetInstanceField("biomeEnrageTimer");
    public int biomeEnrageTimer
    {
        get => (int)i_f_biomeEnrageTimer.GetValue(Source);
        set => i_f_biomeEnrageTimer.SetValue(Source, value);
    }

    // small (instance field)
    public static readonly FieldInfo i_f_small = GetInstanceField("small");
    public bool small
    {
        get => (bool)i_f_small.GetValue(Source);
        set => i_f_small.SetValue(Source, value);
    }

    // medium (instance field)
    public static readonly FieldInfo i_f_medium = GetInstanceField("medium");
    public bool medium
    {
        get => (bool)i_f_medium.GetValue(Source);
        set => i_f_medium.SetValue(Source, value);
    }

    // large (instance field)
    public static readonly FieldInfo i_f_large = GetInstanceField("large");
    public bool large
    {
        get => (bool)i_f_large.GetValue(Source);
        set => i_f_large.SetValue(Source, value);
    }

    // wormsAlive (instance field)
    public static readonly FieldInfo i_f_wormsAlive = GetInstanceField("wormsAlive");
    public int wormsAlive
    {
        get => (int)i_f_wormsAlive.GetValue(Source);
        set => i_f_wormsAlive.SetValue(Source, value);
    }

    // Movement (instance method)
    public static readonly MethodInfo i_m_Movement = GetInstanceMethod("Movement");
    public delegate void Orig_Movement(PerforatorHive self, Player target, float velocity, float acceleration, float y);
    public static readonly Orig_Movement i_d_Movement = i_m_Movement.CreateDelegate<Orig_Movement>();
    public void Movement(Player target, float velocity, float acceleration, float y) => i_d_Movement(Source, target, velocity, acceleration, y);
}

public record PerforatorHeadSmall_Publicizer(PerforatorHeadSmall Source) : Publicizer<PerforatorHeadSmall>(Source)
{
    // biomeEnrageTimer (instance field)
    public static readonly FieldInfo i_f_biomeEnrageTimer = GetInstanceField("biomeEnrageTimer");
    public int biomeEnrageTimer
    {
        get => (int)i_f_biomeEnrageTimer.GetValue(Source);
        set => i_f_biomeEnrageTimer.SetValue(Source, value);
    }

    // MsgType (const)
    public const int MsgType = 23;

    // TailSpawned (instance field)
    public static readonly FieldInfo i_f_TailSpawned = GetInstanceField("tailSpawned");
    public bool TailSpawned
    {
        get => (bool)i_f_TailSpawned.GetValue(Source);
        set => i_f_TailSpawned.SetValue(Source, value);
    }
}

public record PerforatorHeadLarge_Publicizer(PerforatorHeadLarge Source) : Publicizer<PerforatorHeadLarge>(Source)
{
    // biomeEnrageTimer (instance field)
    public static readonly FieldInfo i_f_biomeEnrageTimer = GetInstanceField("biomeEnrageTimer");
    public int biomeEnrageTimer
    {
        get => (int)i_f_biomeEnrageTimer.GetValue(Source);
        set => i_f_biomeEnrageTimer.SetValue(Source, value);
    }

    // TailSpawned (instance field)
    public static readonly FieldInfo i_f_TailSpawned = GetInstanceField("tailSpawned");
    public bool TailSpawned
    {
        get => (bool)i_f_TailSpawned.GetValue(Source);
        set => i_f_TailSpawned.SetValue(Source, value);
    }
}
