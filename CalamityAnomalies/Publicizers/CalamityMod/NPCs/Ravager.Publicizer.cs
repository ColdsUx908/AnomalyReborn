using CalamityMod.NPCs.Ravager;

namespace CalamityAnomalies.Publicizers.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record RavagerBody_Publicizer(RavagerBody Source) : Publicizer<RavagerBody>(Source)
{
    // velocityY (instance field)
    public static readonly FieldInfo i_f_velocityY = GetInstanceField("velocityY");
    public float velocityY
    {
        get => (float)i_f_velocityY.GetValue(Source);
        set => i_f_velocityY.SetValue(Source, value);
    }
}
