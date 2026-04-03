namespace Transoceanic.Framework.Publicizers.Terraria;

#pragma warning disable IDE1006

public sealed record NPC_HitModifiers_Publicizer(NPC.HitModifiers Source) : Publicizer<NPC.HitModifiers>(Source)
{
    // _damageLimit (instance field)
    public static readonly FieldInfo i_f__damageLimit = GetInstanceField("_damageLimit");
    public int _damageLimit
    {
        get => (int)i_f__damageLimit.GetValue(Source);
        set => i_f__damageLimit.SetValue(Source, value);
    }

    // _critOverride (instance field)
    public static readonly FieldInfo i_f__critOverride = GetInstanceField("_critOverride");
    public bool? _critOverride
    {
        get => (bool?)i_f__critOverride.GetValue(Source);
        set => i_f__critOverride.SetValue(Source, value);
    }

    // _knockbackDisabled (instance field)
    public static readonly FieldInfo i_f__knockbackDisabled = GetInstanceField("_knockbackDisabled");
    public bool _knockbackDisabled
    {
        get => (bool)i_f__knockbackDisabled.GetValue(Source);
        set => i_f__knockbackDisabled.SetValue(Source, value);
    }

    // _instantKill (instance field)
    public static readonly FieldInfo i_f__instantKill = GetInstanceField("_instantKill");
    public bool _instantKill
    {
        get => (bool)i_f__instantKill.GetValue(Source);
        set => i_f__instantKill.SetValue(Source, value);
    }

    // _combatTextHidden (instance field)
    public static readonly FieldInfo i_f__combatTextHidden = GetInstanceField("_combatTextHidden");
    public bool _combatTextHidden
    {
        get => (bool)i_f__combatTextHidden.GetValue(Source);
        set => i_f__combatTextHidden.SetValue(Source, value);
    }

    // HitDirectionOverride (instance property)
    public static readonly PropertyInfo i_p_HitDirectionOverride = GetInstanceProperty("HitDirectionOverride");
    public int? HitDirectionOverride
    {
        get => (int?)i_p_HitDirectionOverride.GetValue(Source);
        set => i_p_HitDirectionOverride.SetValue(Source, value);
    }
}
