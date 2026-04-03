using CalamityMod.NPCs.CeaselessVoid;

namespace CalamityAnomalies.Publicizers.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record DarkEnergy_Publicizer(DarkEnergy Source) : Publicizer<DarkEnergy>(Source)
{
    // start (instance field)
    public static readonly FieldInfo i_f_start = GetInstanceField("start");
    public bool start
    {
        get => (bool)i_f_start.GetValue(Source);
        set => i_f_start.SetValue(Source, value);
    }

    // minDistance (const)
    public const double minDistance = 10.0;

    // distance (instance field)
    public static readonly FieldInfo i_f_distance = GetInstanceField("distance");
    public double distance
    {
        get => (double)i_f_distance.GetValue(Source);
        set => i_f_distance.SetValue(Source, value);
    }

    // minMaxDistance (const)
    public const double minMaxDistance = 800.0;
}
