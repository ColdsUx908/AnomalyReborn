using CalamityMod.NPCs.BrimstoneElemental;

namespace CalamityAnomalies.Publicizers.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record Brimling_Publicizer(Brimling Source) : Publicizer<Brimling>(Source)
{
    // boostDR (instance field)
    public static readonly FieldInfo i_f_boostDR = GetInstanceField("boostDR");
    public bool boostDR
    {
        get => (bool)i_f_boostDR.GetValue(Source);
        set => i_f_boostDR.SetValue(Source, value);
    }
}
