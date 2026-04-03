using CalamityMod;

namespace CalamityAnomalies.Publicizers.CalamityMod;

public record TrueMeleeDamageClass_Publicizer : PublicizerNoSource<TrueMeleeDamageClass>
{
    // Instance (static field)
    public static readonly FieldInfo s_f_Instance = GetStaticField("Instance");
    public static TrueMeleeDamageClass Instance
    {
        get => (TrueMeleeDamageClass)s_f_Instance.GetValue(null);
        set => s_f_Instance.SetValue(null, value);
    }
}

public record TrueMeleeNoSpeedDamageClass_Publicizer : PublicizerNoSource<TrueMeleeNoSpeedDamageClass>
{
    // Instance (static field)
    public static readonly FieldInfo s_f_Instance = GetStaticField("Instance");
    public static TrueMeleeNoSpeedDamageClass Instance
    {
        get => (TrueMeleeNoSpeedDamageClass)s_f_Instance.GetValue(null);
        set => s_f_Instance.SetValue(null, value);
    }
}

public record AverageDamageClass_Publicizer : PublicizerNoSource<AverageDamageClass>
{
    // Instance (static field)
    public static readonly FieldInfo s_f_Instance = GetStaticField("Instance");
    public static AverageDamageClass Instance
    {
        get => (AverageDamageClass)s_f_Instance.GetValue(null);
        set => s_f_Instance.SetValue(null, value);
    }
}