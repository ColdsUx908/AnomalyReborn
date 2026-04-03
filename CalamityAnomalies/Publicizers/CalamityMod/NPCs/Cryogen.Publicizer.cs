using CalamityMod.NPCs.Cryogen;

namespace CalamityAnomalies.Publicizers.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record Cryogen_Publicizer(Cryogen Source) : Publicizer<Cryogen>(Source)
{
    // LoadHeadIcons (static method)
    public static readonly MethodInfo s_m_LoadHeadIcons = GetStaticMethod("LoadHeadIcons");
    public delegate void Orig_LoadHeadIcons();
    public static readonly Orig_LoadHeadIcons s_d_LoadHeadIcons = s_m_LoadHeadIcons.CreateDelegate<Orig_LoadHeadIcons>();
    public static void LoadHeadIcons() => s_d_LoadHeadIcons();

    // biomeEnrageTimer (instance field)
    public static readonly FieldInfo i_f_biomeEnrageTimer = GetInstanceField("biomeEnrageTimer");
    public int biomeEnrageTimer
    {
        get => (int)i_f_biomeEnrageTimer.GetValue(Source);
        set => i_f_biomeEnrageTimer.SetValue(Source, value);
    }

    // currentPhase (instance field)
    public static readonly FieldInfo i_f_currentPhase = GetInstanceField("currentPhase");
    public int currentPhase
    {
        get => (int)i_f_currentPhase.GetValue(Source);
        set => i_f_currentPhase.SetValue(Source, value);
    }

    // teleportLocationX (instance field)
    public static readonly FieldInfo i_f_teleportLocationX = GetInstanceField("teleportLocationX");
    public int teleportLocationX
    {
        get => (int)i_f_teleportLocationX.GetValue(Source);
        set => i_f_teleportLocationX.SetValue(Source, value);
    }

    // HandlePhaseTransition (instance method)
    public static readonly MethodInfo i_m_HandlePhaseTransition = GetInstanceMethod("HandlePhaseTransition");
    public delegate void Orig_HandlePhaseTransition(Cryogen self, int newPhase);
    public static readonly Orig_HandlePhaseTransition i_d_HandlePhaseTransition = i_m_HandlePhaseTransition.CreateDelegate<Orig_HandlePhaseTransition>();
    public void HandlePhaseTransition(int newPhase) => i_d_HandlePhaseTransition(Source, newPhase);
}
