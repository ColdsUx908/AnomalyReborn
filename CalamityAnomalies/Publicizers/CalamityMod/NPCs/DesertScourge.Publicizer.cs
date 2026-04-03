using CalamityMod.NPCs.DesertScourge;

namespace CalamityAnomalies.Publicizers.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record DesertScourgeHead_Publicizer(DesertScourgeHead Source) : Publicizer<DesertScourgeHead>(Source)
{
    // biomeEnrageTimer (instance field)
    public static readonly FieldInfo i_f_biomeEnrageTimer = GetInstanceField("biomeEnrageTimer");
    public int biomeEnrageTimer
    {
        get => (int)i_f_biomeEnrageTimer.GetValue(Source);
        set => i_f_biomeEnrageTimer.SetValue(Source, value);
    }

    // tailSpawned (instance field)
    public static readonly FieldInfo i_f_tailSpawned = GetInstanceField("tailSpawned");
    public bool tailSpawned
    {
        get => (bool)i_f_tailSpawned.GetValue(Source);
        set => i_f_tailSpawned.SetValue(Source, value);
    }

    // OpenMouthFrame (const)
    public const int OpenMouthStopFrame = 4;
}

public record DesertNuisanceHead_Publicizer(DesertNuisanceHead Source) : Publicizer<DesertNuisanceHead>(Source)
{
    // biomeEnrageTimer (instance field)
    public static readonly FieldInfo i_f_biomeEnrageTimer = GetInstanceField("biomeEnrageTimer");
    public int biomeEnrageTimer
    {
        get => (int)i_f_biomeEnrageTimer.GetValue(Source);
        set => i_f_biomeEnrageTimer.SetValue(Source, value);
    }

    // tailSpawned (instance field)
    public static readonly FieldInfo i_f_tailSpawned = GetInstanceField("tailSpawned");
    public bool tailSpawned
    {
        get => (bool)i_f_tailSpawned.GetValue(Source);
        set => i_f_tailSpawned.SetValue(Source, value);
    }

    // OpenMouthFrame (const)
    public const int OpenMouthStopFrame = 4;
}

public record DesertNuisanceHeadYoung_Publicizer(DesertNuisanceHeadYoung Source) : Publicizer<DesertNuisanceHeadYoung>(Source)
{
    // biomeEnrageTimer (instance field)
    public static readonly FieldInfo i_f_biomeEnrageTimer = GetInstanceField("biomeEnrageTimer");
    public int biomeEnrageTimer
    {
        get => (int)i_f_biomeEnrageTimer.GetValue(Source);
        set => i_f_biomeEnrageTimer.SetValue(Source, value);
    }

    // tailSpawned (instance field)
    public static readonly FieldInfo i_f_tailSpawned = GetInstanceField("tailSpawned");
    public bool tailSpawned
    {
        get => (bool)i_f_tailSpawned.GetValue(Source);
        set => i_f_tailSpawned.SetValue(Source, value);
    }

    // OpenMouthFrame (const)
    public const int OpenMouthStopFrame = 4;
}