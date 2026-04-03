using CalamityMod.NPCs.CalClone;

namespace CalamityAnomalies.Publicizers.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record SoulSeeker_Publicizer(SoulSeeker Source) : Publicizer<SoulSeeker>(Source)
{
    // timer (instance field)
    public static readonly FieldInfo i_f_timer = GetInstanceField("timer");
    public int timer
    {
        get => (int)i_f_timer.GetValue(Source);
        set => i_f_timer.SetValue(Source, value);
    }

    // start (instance field)
    public static readonly FieldInfo i_f_start = GetInstanceField("start");
    public bool start
    {
        get => (bool)i_f_start.GetValue(Source);
        set => i_f_start.SetValue(Source, value);
    }
}
