using CalamityMod.NPCs.DevourerofGods;

namespace CalamityAnomalies.Publicizers.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record DevourerofGodsHead_Publicizer(DevourerofGodsHead Source) : Publicizer<DevourerofGodsHead>(Source)
{
    // LoadHeadIcons (static method)
    public static readonly MethodInfo s_m_LoadHeadIcons = GetStaticMethod("LoadHeadIcons");
    public delegate void Orig_LoadHeadIcons();
    public static readonly Orig_LoadHeadIcons s_d_LoadHeadIcons = s_m_LoadHeadIcons.CreateDelegate<Orig_LoadHeadIcons>();
    public static void LoadHeadIcons() => s_d_LoadHeadIcons();

    // laserVelocity (const)
    public const float laserVelocity = 14f;

    // shotSpacingMax (const)
    public const int shotSpacingMax = 1470;

    // shotSpacing (instance field)
    public static readonly FieldInfo i_f_shotSpacing = GetInstanceField("shotSpacing");
    public int shotSpacing
    {
        get => (int)i_f_shotSpacing.GetValue(Source);
        set => i_f_shotSpacing.SetValue(Source, value);
    }

    // totalShots (const)
    public const int totalShots = 14;

    // spacingVar (const)
    public const int spacingVar = 210;

    // laserWallType (instance field)
    public static readonly FieldInfo i_f_laserWallType = GetInstanceField("laserWallType");
    public int laserWallType
    {
        get => (int)i_f_laserWallType.GetValue(Source);
        set => i_f_laserWallType.SetValue(Source, value);
    }

    // laserWallSpacingOffset (const)
    public const float laserWallSpacingOffset = 16f;

    // tail (instance field)
    public static readonly FieldInfo i_f_tail = GetInstanceField("tail");
    public bool tail
    {
        get => (bool)i_f_tail.GetValue(Source);
        set => i_f_tail.SetValue(Source, value);
    }

    // minLength (instance field)
    public static readonly FieldInfo i_f_minLength = GetInstanceField("minLength");
    public int minLength
    {
        get => (int)i_f_minLength.GetValue(Source);
        set => i_f_minLength.SetValue(Source, value);
    }

    // maxLength (instance field)
    public static readonly FieldInfo i_f_maxLength = GetInstanceField("maxLength");
    public int maxLength
    {
        get => (int)i_f_maxLength.GetValue(Source);
        set => i_f_maxLength.SetValue(Source, value);
    }

    // spawnedGuardians (instance field)
    public static readonly FieldInfo i_f_spawnedGuardians = GetInstanceField("spawnedGuardians");
    public bool spawnedGuardians
    {
        get => (bool)i_f_spawnedGuardians.GetValue(Source);
        set => i_f_spawnedGuardians.SetValue(Source, value);
    }

    // spawnedGuardians2 (instance field)
    public static readonly FieldInfo i_f_spawnedGuardians2 = GetInstanceField("spawnedGuardians2");
    public bool spawnedGuardians2
    {
        get => (bool)i_f_spawnedGuardians2.GetValue(Source);
        set => i_f_spawnedGuardians2.SetValue(Source, value);
    }

    // spawnDoGCountdown (instance field)
    public static readonly FieldInfo i_f_spawnDoGCountdown = GetInstanceField("spawnDoGCountdown");
    public int spawnDoGCountdown
    {
        get => (int)i_f_spawnDoGCountdown.GetValue(Source);
        set => i_f_spawnDoGCountdown.SetValue(Source, value);
    }

    // hasCreatedPhase1Portal (instance field)
    public static readonly FieldInfo i_f_hasCreatedPhase1Portal = GetInstanceField("hasCreatedPhase1Portal");
    public bool hasCreatedPhase1Portal
    {
        get => (bool)i_f_hasCreatedPhase1Portal.GetValue(Source);
        set => i_f_hasCreatedPhase1Portal.SetValue(Source, value);
    }

    // shotSpacingMax_Phase2 (const)
    public const int shotSpacingMax_Phase2 = 1470;

    // shotSpacing_Phase2 (instance field)
    public static readonly FieldInfo i_f_shotSpacing_Phase2 = GetInstanceField("shotSpacing_Phase2");
    public int[] shotSpacing_Phase2
    {
        get => (int[])i_f_shotSpacing_Phase2.GetValue(Source);
        set => i_f_shotSpacing_Phase2.SetValue(Source, value);
    }

    // spacingVar_Phase2 (const)
    public const int spacingVar_Phase2 = 105;

    // totalShots_Phase2 (const)
    public const int totalShots_Phase2 = 28;

    // totalDiagonalShots (const)
    public const int totalDiagonalShots = 8;

    // diagonalSpacingVar (const)
    public const int diagonalSpacingVar = 366;

    // laserWallType_Phase2 (instance field)
    public static readonly FieldInfo i_f_laserWallType_Phase2 = GetInstanceField("laserWallType_Phase2");
    public int laserWallType_Phase2
    {
        get => (int)i_f_laserWallType_Phase2.GetValue(Source);
        set => i_f_laserWallType_Phase2.SetValue(Source, value);
    }

    // idleCounterMax (const)
    public const int idleCounterMax = 300;

    // idleCounter (instance field)
    public static readonly FieldInfo i_f_idleCounter = GetInstanceField("idleCounter");
    public int idleCounter
    {
        get => (int)i_f_idleCounter.GetValue(Source);
        set => i_f_idleCounter.SetValue(Source, value);
    }

    // postTeleportTimer (instance field)
    public static readonly FieldInfo i_f_postTeleportTimer = GetInstanceField("postTeleportTimer");
    public int postTeleportTimer
    {
        get => (int)i_f_postTeleportTimer.GetValue(Source);
        set => i_f_postTeleportTimer.SetValue(Source, value);
    }

    // teleportTimer (instance field)
    public static readonly FieldInfo i_f_teleportTimer = GetInstanceField("teleportTimer");
    public int teleportTimer
    {
        get => (int)i_f_teleportTimer.GetValue(Source);
        set => i_f_teleportTimer.SetValue(Source, value);
    }

    // TimeBeforeTeleport_Death (const)
    public const int TimeBeforeTeleport_Death = 120;

    // TimeBeforeTeleport_Revengeance (const)
    public const int TimeBeforeTeleport_Revengeance = 140;

    // TimeBeforeTeleport_Expert (const)
    public const int TimeBeforeTeleport_Expert = 160;

    // TimeBeforeTeleport_Normal (const)
    public const int TimeBeforeTeleport_Normal = 180;

    // spawnedGuardians3 (instance field)
    public static readonly FieldInfo i_f_spawnedGuardians3 = GetInstanceField("spawnedGuardians3");
    public bool spawnedGuardians3
    {
        get => (bool)i_f_spawnedGuardians3.GetValue(Source);
        set => i_f_spawnedGuardians3.SetValue(Source, value);
    }

    // alphaGateValue (const)
    public const float alphaGateValue = 669f;

    // SpawnTeleportLocation (instance method)
    public static readonly MethodInfo i_m_SpawnTeleportLocation = GetInstanceMethod("SpawnTeleportLocation");
    public delegate void Orig_SpawnTeleportLocation(DevourerofGodsHead self, Player player, bool phase2Transition);
    public static readonly Orig_SpawnTeleportLocation i_d_SpawnTeleportLocation = i_m_SpawnTeleportLocation.CreateDelegate<Orig_SpawnTeleportLocation>();
    public void SpawnTeleportLocation(Player player, bool phase2Transition) => i_d_SpawnTeleportLocation(Source, player, phase2Transition);

    // Teleport (instance method)
    public static readonly MethodInfo i_m_Teleport = GetInstanceMethod("Teleport");
    public delegate void Orig_Teleport(DevourerofGodsHead self, Player player, bool bossRush, bool death, bool revenge, bool expertMode, bool phase5);
    public static readonly Orig_Teleport i_d_Teleport = i_m_Teleport.CreateDelegate<Orig_Teleport>();
    public void Teleport(Player player, bool bossRush, bool death, bool revenge, bool expertMode, bool phase5) => i_d_Teleport(Source, player, bossRush, death, revenge, expertMode, phase5);

    // GetRiftLocation (instance method)
    public static readonly MethodInfo i_m_GetRiftLocation = GetInstanceMethod("GetRiftLocation");
    public delegate Vector2 Orig_GetRiftLocation(DevourerofGodsHead self, bool spawnDust);
    public static readonly Orig_GetRiftLocation i_d_GetRiftLocation = i_m_GetRiftLocation.CreateDelegate<Orig_GetRiftLocation>();
    public Vector2 GetRiftLocation(bool spawnDust) => i_d_GetRiftLocation(Source, spawnDust);
}

public record DevourerofGodsBody_Publicizer(DevourerofGodsBody Source) : Publicizer<DevourerofGodsBody>(Source)
{
    // LoadHeadIcons (static method)
    public static readonly MethodInfo s_m_LoadHeadIcons = GetStaticMethod("LoadHeadIcons");
    public delegate void Orig_LoadHeadIcons();
    public static readonly Orig_LoadHeadIcons s_d_LoadHeadIcons = s_m_LoadHeadIcons.CreateDelegate<Orig_LoadHeadIcons>();
    public static void LoadHeadIcons() => s_d_LoadHeadIcons();

    // LaserVelocityMultiplierMin (const)
    public const float LaserVelocityMultiplierMin = 0.5f;

    // LaserVelocityDistanceMultiplier (const)
    public const float LaserVelocityDistanceMultiplier = LaserVelocityMultiplierMin * 0.05f;

    // invinceTime (instance field)
    public static readonly FieldInfo i_f_invinceTime = GetInstanceField("invinceTime");
    public int invinceTime
    {
        get => (int)i_f_invinceTime.GetValue(Source);
        set => i_f_invinceTime.SetValue(Source, value);
    }

    // setOpacity (instance field)
    public static readonly FieldInfo i_f_setOpacity = GetInstanceField("setOpacity");
    public bool setOpacity
    {
        get => (bool)i_f_setOpacity.GetValue(Source);
        set => i_f_setOpacity.SetValue(Source, value);
    }

    // phase2Started (instance field)
    public static readonly FieldInfo i_f_phase2Started = GetInstanceField("phase2Started");
    public bool phase2Started
    {
        get => (bool)i_f_phase2Started.GetValue(Source);
        set => i_f_phase2Started.SetValue(Source, value);
    }

    // AnyTeleportRifts (instance method)
    public static readonly MethodInfo i_m_AnyTeleportRifts = GetInstanceMethod("AnyTeleportRifts");
    public delegate bool Orig_AnyTeleportRifts(DevourerofGodsBody self);
    public static readonly Orig_AnyTeleportRifts i_d_AnyTeleportRifts = i_m_AnyTeleportRifts.CreateDelegate<Orig_AnyTeleportRifts>();
    public bool AnyTeleportRifts() => i_d_AnyTeleportRifts(Source);
}

public record DevourerofGodsTail_Publicizer(DevourerofGodsTail Source) : Publicizer<DevourerofGodsTail>(Source)
{
    // LoadHeadIcons (static method)
    public static readonly MethodInfo s_m_LoadHeadIcons = GetStaticMethod("LoadHeadIcons");
    public delegate void Orig_LoadHeadIcons();
    public static readonly Orig_LoadHeadIcons s_d_LoadHeadIcons = s_m_LoadHeadIcons.CreateDelegate<Orig_LoadHeadIcons>();
    public static void LoadHeadIcons() => s_d_LoadHeadIcons();

    // invinceTime (instance field)
    public static readonly FieldInfo i_f_invinceTime = GetInstanceField("invinceTime");
    public int invinceTime
    {
        get => (int)i_f_invinceTime.GetValue(Source);
        set => i_f_invinceTime.SetValue(Source, value);
    }

    // setOpacity (instance field)
    public static readonly FieldInfo i_f_setOpacity = GetInstanceField("setOpacity");
    public bool setOpacity
    {
        get => (bool)i_f_setOpacity.GetValue(Source);
        set => i_f_setOpacity.SetValue(Source, value);
    }

    // phase2Started (instance field)
    public static readonly FieldInfo i_f_phase2Started = GetInstanceField("phase2Started");
    public bool phase2Started
    {
        get => (bool)i_f_phase2Started.GetValue(Source);
        set => i_f_phase2Started.SetValue(Source, value);
    }

    // setInvulTime (instance method)
    public static readonly MethodInfo i_m_setInvulTime = GetInstanceMethod("setInvulTime");
    public delegate void Orig_setInvulTime(DevourerofGodsTail self, int time);
    public static readonly Orig_setInvulTime i_d_setInvulTime = i_m_setInvulTime.CreateDelegate<Orig_setInvulTime>();
    public void setInvulTime(int time) => i_d_setInvulTime(Source, time);
}