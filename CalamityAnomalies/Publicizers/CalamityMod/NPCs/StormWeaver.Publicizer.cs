using CalamityMod.NPCs.StormWeaver;

namespace CalamityAnomalies.Publicizers.CalamityMod.NPCs;

#pragma warning disable IDE1006
public record StormWeaverHead_Publicizer(StormWeaverHead Source) : Publicizer<StormWeaverHead>(Source)
{
    // LoadHeadIcons (static method)
    public static readonly MethodInfo s_m_LoadHeadIcons = GetStaticMethod("LoadHeadIcons");
    public delegate void Orig_LoadHeadIcons();
    public static readonly Orig_LoadHeadIcons s_d_LoadHeadIcons = s_m_LoadHeadIcons.CreateDelegate<Orig_LoadHeadIcons>();
    public static void LoadHeadIcons() => s_d_LoadHeadIcons();

    // BoltAngleSpread (const)
    public const float BoltAngleSpread = 280;

    // tail (instance field)
    public static readonly FieldInfo i_f_tail = GetInstanceField("tail");
    public bool tail
    {
        get => (bool)i_f_tail.GetValue(Source);
        set => i_f_tail.SetValue(Source, value);
    }

    // lightningDecay (instance field)
    public static readonly FieldInfo i_f_lightningDecay = GetInstanceField("lightningDecay");
    public float lightningDecay
    {
        get => (float)i_f_lightningDecay.GetValue(Source);
        set => i_f_lightningDecay.SetValue(Source, value);
    }

    // lightningSpeed (instance field)
    public static readonly FieldInfo i_f_lightningSpeed = GetInstanceField("lightningSpeed");
    public float lightningSpeed
    {
        get => (float)i_f_lightningSpeed.GetValue(Source);
        set => i_f_lightningSpeed.SetValue(Source, value);
    }
}

public record StormWeaverTail_Publicizer(StormWeaverTail Source) : Publicizer<StormWeaverTail>(Source)
{
    // invinceTime (instance field)
    public static readonly FieldInfo i_f_invinceTime = GetInstanceField("invinceTime");
    public int invinceTime
    {
        get => (int)i_f_invinceTime.GetValue(Source);
        set => i_f_invinceTime.SetValue(Source, value);
    }
}
