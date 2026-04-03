using CalamityMod.NPCs.Signus;

namespace CalamityAnomalies.Publicizers.CalamityMod.NPCs;
#pragma warning disable IDE1006
public record Signus_Publicizer(Signus Source) : Publicizer<Signus>(Source)
{
    // spawnX (instance field)
    public static readonly FieldInfo i_f_spawnX = GetInstanceField("spawnX");
    public int spawnX
    {
        get => (int)i_f_spawnX.GetValue(Source);
        set => i_f_spawnX.SetValue(Source, value);
    }

    // spawnY (instance field)
    public static readonly FieldInfo i_f_spawnY = GetInstanceField("spawnY");
    public int spawnY
    {
        get => (int)i_f_spawnY.GetValue(Source);
        set => i_f_spawnY.SetValue(Source, value);
    }

    // lifeToAlpha (instance field)
    public static readonly FieldInfo i_f_lifeToAlpha = GetInstanceField("lifeToAlpha");
    public int lifeToAlpha
    {
        get => (int)i_f_lifeToAlpha.GetValue(Source);
        set => i_f_lifeToAlpha.SetValue(Source, value);
    }

    // stealthTimer (instance field)
    public static readonly FieldInfo i_f_stealthTimer = GetInstanceField("stealthTimer");
    public int stealthTimer
    {
        get => (int)i_f_stealthTimer.GetValue(Source);
        set => i_f_stealthTimer.SetValue(Source, value);
    }
}