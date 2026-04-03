using CalamityMod;

namespace CalamityAnomalies.ModCompatibility.CalamityPublicizers;

internal record TrueMeleeDamageClass_Publicizer : Publicizer<TrueMeleeDamageClass>
{
    // Instance (static field)
    public static readonly FieldInfo s_f_Instance = GetStaticField("Instance");
    public static TrueMeleeDamageClass Instance
    {
        get => (TrueMeleeDamageClass)s_f_Instance.GetValue(null);
        set => s_f_Instance.SetValue(null, value);
    }
}

internal record TrueMeleeNoSpeedDamageClass_Publicizer : Publicizer<TrueMeleeNoSpeedDamageClass>
{
    // Instance (static field)
    public static readonly FieldInfo s_f_Instance = GetStaticField("Instance");
    public static TrueMeleeNoSpeedDamageClass Instance
    {
        get => (TrueMeleeNoSpeedDamageClass)s_f_Instance.GetValue(null);
        set => s_f_Instance.SetValue(null, value);
    }
}

internal record AverageDamageClass_Publicizer : Publicizer<AverageDamageClass>
{
    // Instance (static field)
    public static readonly FieldInfo s_f_Instance = GetStaticField("Instance");
    public static AverageDamageClass Instance
    {
        get => (AverageDamageClass)s_f_Instance.GetValue(null);
        set => s_f_Instance.SetValue(null, value);
    }
}