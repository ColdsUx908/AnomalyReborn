namespace CalamityAnomalies.ModCompatibility.CalamityPublicizers;

#pragma warning disable IDE1006

internal record CalamityMod_Publicizer(CalamityMod_ Source) : InstancedPublicizer<CalamityMod_>(Source)
{
    // _Instance (static field)
    public static readonly FieldInfo s_f__Instance = GetStaticField("_Instance");

    public static CalamityMod_ _Instance
    {
        get => (CalamityMod_)s_f__Instance.GetValue(null);
        set => s_f__Instance.SetValue(null, value);
    }

    public static CalamityMod_ Instance => _Instance;
}
