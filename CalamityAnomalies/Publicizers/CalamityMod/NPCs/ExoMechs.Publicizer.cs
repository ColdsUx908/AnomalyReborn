using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.Projectiles.Boss;

namespace CalamityAnomalies.Publicizers.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record Artemis_Publicizer(Artemis Source) : Publicizer<Artemis>(Source)
{
    // LoadHeadIcons (static method)
    public static readonly MethodInfo s_m_LoadHeadIcons = GetStaticMethod("LoadHeadIcons");
    public delegate void Orig_LoadHeadIcons();
    public static readonly Orig_LoadHeadIcons s_d_LoadHeadIcons = s_m_LoadHeadIcons.CreateDelegate<Orig_LoadHeadIcons>();
    public static void LoadHeadIcons() => s_d_LoadHeadIcons();

    // velocityBoostMult (instance field)
    public static readonly FieldInfo i_f_velocityBoostMult = GetInstanceField("velocityBoostMult");
    public float velocityBoostMult
    {
        get => (float)i_f_velocityBoostMult.GetValue(Source);
        set => i_f_velocityBoostMult.SetValue(Source, value);
    }

    // chargeVelocityNormalized (instance field)
    public static readonly FieldInfo i_f_chargeVelocityNormalized = GetInstanceField("chargeVelocityNormalized");
    public Vector2 chargeVelocityNormalized
    {
        get => (Vector2)i_f_chargeVelocityNormalized.GetValue(Source);
        set => i_f_chargeVelocityNormalized.SetValue(Source, value);
    }

    // maxFramesX (const)
    public const int maxFramesX = 10;

    // maxFramesY (const)
    public const int maxFramesY = 9;

    // frameX (instance field)
    public static readonly FieldInfo i_f_frameX = GetInstanceField("frameX");
    public int frameX
    {
        get => (int)i_f_frameX.GetValue(Source);
        set => i_f_frameX.SetValue(Source, value);
    }

    // frameY (instance field)
    public static readonly FieldInfo i_f_frameY = GetInstanceField("frameY");
    public int frameY
    {
        get => (int)i_f_frameY.GetValue(Source);
        set => i_f_frameY.SetValue(Source, value);
    }

    // normalFrameLimit_Phase1 (const)
    public const int normalFrameLimit_Phase1 = 9;

    // chargeUpFrameLimit_Phase1 (const)
    public const int chargeUpFrameLimit_Phase1 = 19;

    // attackFrameLimit_Phase1 (const)
    public const int attackFrameLimit_Phase1 = 29;

    // phaseTransitionFrameLimit (const)
    public const int phaseTransitionFrameLimit = 59;

    // normalFrameLimit_Phase2 (const)
    public const int normalFrameLimit_Phase2 = 69;

    // chargeUpFrameLimit_Phase2 (const)
    public const int chargeUpFrameLimit_Phase2 = 79;

    // attackFrameLimit_Phase2 (const)
    public const int attackFrameLimit_Phase2 = 89;

    // defaultLifeRatio (const)
    public const float defaultLifeRatio = 5f;

    // soundDistance (const)
    public const float soundDistance = 2800f;

    // defaultAnimationDuration (const)
    public const float defaultAnimationDuration = 60f;

    // phaseTransitionDuration (const)
    public const float phaseTransitionDuration = 180f;

    // lensPopTime (const)
    public const float lensPopTime = 48f;

    // deathrayTelegraphDuration (const)
    public const float deathrayTelegraphDuration = 60f;

    // PauseDurationBeforeLaserActuallyFires (const)
    public const float PauseDurationBeforeLaserActuallyFires = ArtemisLaser.TelegraphTotalTime;

    // pointToLookAt (instance field)
    public static readonly FieldInfo i_f_pointToLookAt = GetInstanceField("pointToLookAt");
    public Vector2 pointToLookAt
    {
        get => (Vector2)i_f_pointToLookAt.GetValue(Source);
        set => i_f_pointToLookAt.SetValue(Source, value);
    }

    // deathrayDuration (const)
    public const float deathrayDuration = 180f;

    // pickNewLocation (instance field)
    public static readonly FieldInfo i_f_pickNewLocation = GetInstanceField("pickNewLocation");
    public bool pickNewLocation
    {
        get => (bool)i_f_pickNewLocation.GetValue(Source);
        set => i_f_pickNewLocation.SetValue(Source, value);
    }

    // rotationDirection (instance field)
    public static readonly FieldInfo i_f_rotationDirection = GetInstanceField("rotationDirection");
    public int rotationDirection
    {
        get => (int)i_f_rotationDirection.GetValue(Source);
        set => i_f_rotationDirection.SetValue(Source, value);
    }

    // spinningPoint (instance field)
    public static readonly FieldInfo i_f_spinningPoint = GetInstanceField("spinningPoint");
    public Vector2 spinningPoint
    {
        get => (Vector2)i_f_spinningPoint.GetValue(Source);
        set => i_f_spinningPoint.SetValue(Source, value);
    }

    // spinVelocity (instance field)
    public static readonly FieldInfo i_f_spinVelocity = GetInstanceField("spinVelocity");
    public Vector2 spinVelocity
    {
        get => (Vector2)i_f_spinVelocity.GetValue(Source);
        set => i_f_spinVelocity.SetValue(Source, value);
    }

    // DeathraySoundSlot (instance field)
    public static readonly FieldInfo i_f_DeathraySoundSlot = GetInstanceField("DeathraySoundSlot");
    public SlotId DeathraySoundSlot
    {
        get => (SlotId)i_f_DeathraySoundSlot.GetValue(Source);
        set => i_f_DeathraySoundSlot.SetValue(Source, value);
    }
}

public record Apollo_Publicizer(Apollo Source) : Publicizer<Apollo>(Source)
{
    // LoadHeadIcons (static method)
    public static readonly MethodInfo s_m_LoadHeadIcons = GetStaticMethod("LoadHeadIcons");
    public delegate void Orig_LoadHeadIcons();
    public static readonly Orig_LoadHeadIcons s_d_LoadHeadIcons = s_m_LoadHeadIcons.CreateDelegate<Orig_LoadHeadIcons>();
    public static void LoadHeadIcons() => s_d_LoadHeadIcons();

    // velocityBoostMult (instance field)
    public static readonly FieldInfo i_f_velocityBoostMult = GetInstanceField("velocityBoostMult");
    public float velocityBoostMult
    {
        get => (float)i_f_velocityBoostMult.GetValue(Source);
        set => i_f_velocityBoostMult.SetValue(Source, value);
    }

    // maxFramesX (const)
    public const int maxFramesX = 10;

    // maxFramesY (const)
    public const int maxFramesY = 9;

    // frameX (instance field)
    public static readonly FieldInfo i_f_frameX = GetInstanceField("frameX");
    public int frameX
    {
        get => (int)i_f_frameX.GetValue(Source);
        set => i_f_frameX.SetValue(Source, value);
    }

    // frameY (instance field)
    public static readonly FieldInfo i_f_frameY = GetInstanceField("frameY");
    public int frameY
    {
        get => (int)i_f_frameY.GetValue(Source);
        set => i_f_frameY.SetValue(Source, value);
    }

    // normalFrameLimit_Phase1 (const)
    public const int normalFrameLimit_Phase1 = 9;

    // chargeUpFrameLimit_Phase1 (const)
    public const int chargeUpFrameLimit_Phase1 = 19;

    // attackFrameLimit_Phase1 (const)
    public const int attackFrameLimit_Phase1 = 29;

    // phaseTransitionFrameLimit (const)
    public const int phaseTransitionFrameLimit = 59;

    // normalFrameLimit_Phase2 (const)
    public const int normalFrameLimit_Phase2 = 69;

    // chargeUpFrameLimit_Phase2 (const)
    public const int chargeUpFrameLimit_Phase2 = 79;

    // attackFrameLimit_Phase2 (const)
    public const int attackFrameLimit_Phase2 = 89;

    // defaultLifeRatio (const)
    public const float defaultLifeRatio = 5f;

    // soundDistance (const)
    public const float soundDistance = 2800f;

    // defaultAnimationDuration (const)
    public const float defaultAnimationDuration = 60f;

    // phaseTransitionDuration (const)
    public const float phaseTransitionDuration = 180f;

    // lensPopTime (const)
    public const float lensPopTime = 48f;

    // pickNewLocation (instance field)
    public static readonly FieldInfo i_f_pickNewLocation = GetInstanceField("pickNewLocation");
    public bool pickNewLocation
    {
        get => (bool)i_f_pickNewLocation.GetValue(Source);
        set => i_f_pickNewLocation.SetValue(Source, value);
    }

    // maxCharges (const)
    public const int maxCharges = 4;

    // KillProjectiles (instance method)
    public static readonly MethodInfo i_m_KillProjectiles = GetInstanceMethod("KillProjectiles");
    public delegate void Orig_KillProjectiles(Apollo self);
    public static readonly Orig_KillProjectiles i_d_KillProjectiles = i_m_KillProjectiles.CreateDelegate<Orig_KillProjectiles>();
    public void KillProjectiles() => i_d_KillProjectiles(Source);
}

public record ThanatosHead_Publicizer(ThanatosHead Source) : Publicizer<ThanatosHead>(Source)
{
    // LoadHeadIcons (static method)
    public static readonly MethodInfo s_m_LoadHeadIcons = GetStaticMethod("LoadHeadIcons");
    public delegate void Orig_LoadHeadIcons();
    public static readonly Orig_LoadHeadIcons s_d_LoadHeadIcons = s_m_LoadHeadIcons.CreateDelegate<Orig_LoadHeadIcons>();
    public static void LoadHeadIcons() => s_d_LoadHeadIcons();

    // noContactDamageTimer (instance field)
    public static readonly FieldInfo i_f_noContactDamageTimer = GetInstanceField("noContactDamageTimer");
    public int noContactDamageTimer
    {
        get => (int)i_f_noContactDamageTimer.GetValue(Source);
        set => i_f_noContactDamageTimer.SetValue(Source, value);
    }

    // vulnerable (instance field)
    public static readonly FieldInfo i_f_vulnerable = GetInstanceField("vulnerable");
    public bool vulnerable
    {
        get => (bool)i_f_vulnerable.GetValue(Source);
        set => i_f_vulnerable.SetValue(Source, value);
    }

    // ventDuration (const)
    public const float ventDuration = 180f;

    // ventCloudSpawnRate (const)
    public const int ventCloudSpawnRate = 10;

    // defaultLifeRatio (const)
    public const float defaultLifeRatio = 5f;

    // baseDistance (const)
    public const float baseDistance = 800f;

    // baseTurnDistance (const)
    public const float baseTurnDistance = 160f;

    // soundDistance (const)
    public const float soundDistance = 2800f;

    // maxLength (const)
    public const int maxLength = 101;

    // tailSpawned (instance field)
    public static readonly FieldInfo i_f_tailSpawned = GetInstanceField("tailSpawned");
    public bool tailSpawned
    {
        get => (bool)i_f_tailSpawned.GetValue(Source);
        set => i_f_tailSpawned.SetValue(Source, value);
    }

    // chargeVelocityScalar (instance field)
    public static readonly FieldInfo i_f_chargeVelocityScalar = GetInstanceField("chargeVelocityScalar");
    public float chargeVelocityScalar
    {
        get => (float)i_f_chargeVelocityScalar.GetValue(Source);
        set => i_f_chargeVelocityScalar.SetValue(Source, value);
    }

    // deathrayTelegraphDuration (const)
    public const float deathrayTelegraphDuration = 180f;

    // deathrayDuration (const)
    public const float deathrayDuration = 180f;
}

public record ThanatosBody1_Publicizer(ThanatosBody1 Source) : Publicizer<ThanatosBody1>(Source)
{
    // LoadHeadIcons (static method)
    public static readonly MethodInfo s_m_LoadHeadIcons = GetStaticMethod("LoadHeadIcons");
    public delegate void Orig_LoadHeadIcons();
    public static readonly Orig_LoadHeadIcons s_d_LoadHeadIcons = s_m_LoadHeadIcons.CreateDelegate<Orig_LoadHeadIcons>();
    public static void LoadHeadIcons() => s_d_LoadHeadIcons();

    // vulnerable (instance field)
    public static readonly FieldInfo i_f_vulnerable = GetInstanceField("vulnerable");
    public bool vulnerable
    {
        get => (bool)i_f_vulnerable.GetValue(Source);
        set => i_f_vulnerable.SetValue(Source, value);
    }

    // defaultLifeRatio (const)
    public const float defaultLifeRatio = 5f;

    // noContactDamageTimer (instance field)
    public static readonly FieldInfo i_f_noContactDamageTimer = GetInstanceField("noContactDamageTimer");
    public int noContactDamageTimer
    {
        get => (int)i_f_noContactDamageTimer.GetValue(Source);
        set => i_f_noContactDamageTimer.SetValue(Source, value);
    }

    // timeToOpenAndFireLasers (const)
    public const float timeToOpenAndFireLasers = 36f;

    // segmentCloseTimerDecrement (const)
    public const float segmentCloseTimerDecrement = 0.2f;
}

public record ThanatosBody2_Publicizer(ThanatosBody2 Source) : Publicizer<ThanatosBody2>(Source)
{
    // LoadHeadIcons (static method)
    public static readonly MethodInfo s_m_LoadHeadIcons = GetStaticMethod("LoadHeadIcons");
    public delegate void Orig_LoadHeadIcons();
    public static readonly Orig_LoadHeadIcons s_d_LoadHeadIcons = s_m_LoadHeadIcons.CreateDelegate<Orig_LoadHeadIcons>();
    public static void LoadHeadIcons() => s_d_LoadHeadIcons();

    // vulnerable (instance field)
    public static readonly FieldInfo i_f_vulnerable = GetInstanceField("vulnerable");
    public bool vulnerable
    {
        get => (bool)i_f_vulnerable.GetValue(Source);
        set => i_f_vulnerable.SetValue(Source, value);
    }

    // defaultLifeRatio (const)
    public const float defaultLifeRatio = 5f;

    // noContactDamageTimer (instance field)
    public static readonly FieldInfo i_f_noContactDamageTimer = GetInstanceField("noContactDamageTimer");
    public int noContactDamageTimer
    {
        get => (int)i_f_noContactDamageTimer.GetValue(Source);
        set => i_f_noContactDamageTimer.SetValue(Source, value);
    }

    // timeToOpenAndFireLasers (const)
    public const float timeToOpenAndFireLasers = 36f;

    // segmentCloseTimerDecrement (const)
    public const float segmentCloseTimerDecrement = 0.2f;
}

public record ThanatosTail_Publicizer(ThanatosTail Source) : Publicizer<ThanatosTail>(Source)
{
    // LoadHeadIcons (static method)
    public static readonly MethodInfo s_m_LoadHeadIcons = GetStaticMethod("LoadHeadIcons");
    public delegate void Orig_LoadHeadIcons();
    public static readonly Orig_LoadHeadIcons s_d_LoadHeadIcons = s_m_LoadHeadIcons.CreateDelegate<Orig_LoadHeadIcons>();
    public static void LoadHeadIcons() => s_d_LoadHeadIcons();

    // vulnerable (instance field)
    public static readonly FieldInfo i_f_vulnerable = GetInstanceField("vulnerable");
    public bool vulnerable
    {
        get => (bool)i_f_vulnerable.GetValue(Source);
        set => i_f_vulnerable.SetValue(Source, value);
    }

    // noContactDamageTimer (instance field)
    public static readonly FieldInfo i_f_noContactDamageTimer = GetInstanceField("noContactDamageTimer");
    public int noContactDamageTimer
    {
        get => (int)i_f_noContactDamageTimer.GetValue(Source);
        set => i_f_noContactDamageTimer.SetValue(Source, value);
    }

    // defaultLifeRatio (const)
    public const float defaultLifeRatio = 5f;

    // timeToOpenAndFireLasers (const)
    public const float timeToOpenAndFireLasers = 36f;

    // segmentCloseTimerDecrement (const)
    public const float segmentCloseTimerDecrement = 0.2f;
}

public record AresBody_Publicizer(AresBody Source) : Publicizer<AresBody>(Source)
{
    // maxFramesX (const)
    public const int maxFramesX = 6;

    // maxFramesY (const)
    public const int maxFramesY = 8;

    // ventCloudSpawnRate (const)
    public const int ventCloudSpawnRate = 3;

    // telegraphParticlesSpawnRate (const)
    public const int telegraphParticlesSpawnRate = 5;

    // frameX (instance field)
    public static readonly FieldInfo i_f_frameX = GetInstanceField("frameX");
    public int frameX
    {
        get => (int)i_f_frameX.GetValue(Source);
        set => i_f_frameX.SetValue(Source, value);
    }

    // frameY (instance field)
    public static readonly FieldInfo i_f_frameY = GetInstanceField("frameY");
    public int frameY
    {
        get => (int)i_f_frameY.GetValue(Source);
        set => i_f_frameY.SetValue(Source, value);
    }

    // normalFrameLimit (const)
    public const int normalFrameLimit = 11;

    // firstStageDeathrayChargeFrameLimit (const)
    public const int firstStageDeathrayChargeFrameLimit = 23;

    // secondStageDeathrayChargeFrameLimit (const)
    public const int secondStageDeathrayChargeFrameLimit = 35;

    // finalStageDeathrayChargeFrameLimit (const)
    public const int finalStageDeathrayChargeFrameLimit = 47;

    // defaultLifeRatio (const)
    public const float defaultLifeRatio = 5f;

    // deathrayTelegraphDuration_Normal (const)
    public const float deathrayTelegraphDuration_Normal = 150f;

    // deathrayTelegraphDuration_Expert (const)
    public const float deathrayTelegraphDuration_Expert = 120f;

    // deathrayTelegraphDuration_Rev (const)
    public const float deathrayTelegraphDuration_Rev = 105f;

    // deathrayTelegraphDuration_Death (const)
    public const float deathrayTelegraphDuration_Death = 90f;

    // deathrayTelegraphDuration_BossRush (const)
    public const float deathrayTelegraphDuration_BossRush = 60f;

    // deathrayDuration (const)
    public const float deathrayDuration = 600f;

    // soundDistance (const)
    public const float soundDistance = 4800f;

    // DeathrayEnrageDistance (const)
    public const float DeathrayEnrageDistance = 2480f;

    // armsSpawned (instance field)
    public static readonly FieldInfo i_f_armsSpawned = GetInstanceField("armsSpawned");
    public bool armsSpawned
    {
        get => (bool)i_f_armsSpawned.GetValue(Source);
        set => i_f_armsSpawned.SetValue(Source, value);
    }

    // WidthFunction (instance method)
    public static readonly MethodInfo i_m_WidthFunction = GetInstanceMethod("WidthFunction");
    public delegate float Orig_WidthFunction(AresBody self, float completionRatio);
    public static readonly Orig_WidthFunction i_d_WidthFunction = i_m_WidthFunction.CreateDelegate<Orig_WidthFunction>();
    public float WidthFunction(float completionRatio) => i_d_WidthFunction(Source, completionRatio);

    // ColorFunction (instance method)
    public static readonly MethodInfo i_m_ColorFunction = GetInstanceMethod("ColorFunction");
    public delegate Color Orig_ColorFunction(AresBody self, float completionRatio);
    public static readonly Orig_ColorFunction i_d_ColorFunction = i_m_ColorFunction.CreateDelegate<Orig_ColorFunction>();
    public Color ColorFunction(float completionRatio) => i_d_ColorFunction(Source, completionRatio);

    // BackgroundWidthFunction (instance method)
    public static readonly MethodInfo i_m_BackgroundWidthFunction = GetInstanceMethod("ColorFunction");
    public delegate Color Orig_BackgroundWidthFunction(AresBody self, float completionRatio);
    public static readonly Orig_BackgroundWidthFunction i_d_BackgroundWidthFunction = i_m_BackgroundWidthFunction.CreateDelegate<Orig_BackgroundWidthFunction>();
    public Color BackgroundWidthFunction(float completionRatio) => i_d_BackgroundWidthFunction(Source, completionRatio);
}
