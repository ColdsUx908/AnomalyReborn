using CalamityMod.NPCs.Yharon;

namespace CalamityAnomalies.Publicizers.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record Yharon_Publicizer(Yharon Source) : Publicizer<Yharon>(Source)
{
    // safeBox (instance field)
    public static readonly FieldInfo i_f_safeBox = GetInstanceField("safeBox");
    public Rectangle safeBox
    {
        get => (Rectangle)i_f_safeBox.GetValue(Source);
        set => i_f_safeBox.SetValue(Source, value);
    }

    // enraged (instance field)
    public static readonly FieldInfo i_f_enraged = GetInstanceField("enraged");
    public bool enraged
    {
        get => (bool)i_f_enraged.GetValue(Source);
        set => i_f_enraged.SetValue(Source, value);
    }

    // protectionBoost (instance field)
    public static readonly FieldInfo i_f_protectionBoost = GetInstanceField("protectionBoost");
    public bool protectionBoost
    {
        get => (bool)i_f_protectionBoost.GetValue(Source);
        set => i_f_protectionBoost.SetValue(Source, value);
    }

    // moveCloser (instance field)
    public static readonly FieldInfo i_f_moveCloser = GetInstanceField("moveCloser");
    public bool moveCloser
    {
        get => (bool)i_f_moveCloser.GetValue(Source);
        set => i_f_moveCloser.SetValue(Source, value);
    }

    // useTornado (instance field)
    public static readonly FieldInfo i_f_useTornado = GetInstanceField("useTornado");
    public bool useTornado
    {
        get => (bool)i_f_useTornado.GetValue(Source);
        set => i_f_useTornado.SetValue(Source, value);
    }

    // secondPhasePhase (instance field)
    public static readonly FieldInfo i_f_secondPhasePhase = GetInstanceField("secondPhasePhase");
    public int secondPhasePhase
    {
        get => (int)i_f_secondPhasePhase.GetValue(Source);
        set => i_f_secondPhasePhase.SetValue(Source, value);
    }

    // teleportLocation (instance field)
    public static readonly FieldInfo i_f_teleportLocation = GetInstanceField("teleportLocation");
    public int teleportLocation
    {
        get => (int)i_f_teleportLocation.GetValue(Source);
        set => i_f_teleportLocation.SetValue(Source, value);
    }

    // startSecondAI (instance field)
    public static readonly FieldInfo i_f_startSecondAI = GetInstanceField("startSecondAI");
    public bool startSecondAI
    {
        get => (bool)i_f_startSecondAI.GetValue(Source);
        set => i_f_startSecondAI.SetValue(Source, value);
    }

    // spawnArena (instance field)
    public static readonly FieldInfo i_f_spawnArena = GetInstanceField("spawnArena");
    public bool spawnArena
    {
        get => (bool)i_f_spawnArena.GetValue(Source);
        set => i_f_spawnArena.SetValue(Source, value);
    }

    // invincibilityCounter (instance field)
    public static readonly FieldInfo i_f_invincibilityCounter = GetInstanceField("invincibilityCounter");
    public int invincibilityCounter
    {
        get => (int)i_f_invincibilityCounter.GetValue(Source);
        set => i_f_invincibilityCounter.SetValue(Source, value);
    }

    // fastChargeTelegraphTime (instance field)
    public static readonly FieldInfo i_f_fastChargeTelegraphTime = GetInstanceField("fastChargeTelegraphTime");
    public int fastChargeTelegraphTime
    {
        get => (int)i_f_fastChargeTelegraphTime.GetValue(Source);
        set => i_f_fastChargeTelegraphTime.SetValue(Source, value);
    }

    // ai2GateValue (const)
    public const float ai2GateValue = 0.55f;

    // ChargeDust (instance method)
    public static readonly MethodInfo i_m_ChargeDust = GetInstanceMethod("ChargeDust");
    public delegate void Orig_ChargeDust(Yharon self, int dustAmt, float pie);
    public static readonly Orig_ChargeDust i_d_ChargeDust = i_m_ChargeDust.CreateDelegate<Orig_ChargeDust>();
    public void ChargeDust(int dustAmt, float pie) => i_d_ChargeDust(Source, dustAmt, pie);

    // DoFlareDustBulletHell (instance method)
    public static readonly MethodInfo i_m_DoFlareDustBulletHell = GetInstanceMethod("DoFlareDustBulletHell");
    public delegate void Orig_DoFlareDustBulletHell(Yharon self, int attackType, int timer, int projectileDamage, int totalProjectiles, float projectileVelocity, float radialOffset, bool phase2);
    public static readonly Orig_DoFlareDustBulletHell i_d_DoFlareDustBulletHell = i_m_DoFlareDustBulletHell.CreateDelegate<Orig_DoFlareDustBulletHell>();
    public void DoFlareDustBulletHell(int attackType, int timer, int projectileDamage, int totalProjectiles, float projectileVelocity, float radialOffset, bool phase2) => i_d_DoFlareDustBulletHell(Source, attackType, timer, projectileDamage, totalProjectiles, projectileVelocity, radialOffset, phase2);
}
