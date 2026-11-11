namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHead
{
    public enum Phase : byte
    {
        Initialize,
        Phase1,
        PhaseChange_1To2,
        Phase2_1,
        Phase2_2,
        Phase2_3,
        Phase2_4,
        Phase2_5 = 8,
        Phase2_6 = 9,
        Phase2_7 = 10,
        Phase2_Finale = 7, // 保留已有同步值。
    }

    public enum Behavior : byte
    {
        Despawn = byte.MaxValue,

        None = 0,

        Normal,
        Spin,
    }

    public enum Spell : byte
    {
        None = 0,

        Slime,
        Star,
        PlatinumCoin,
    }

    public enum Avatar : byte
    {
        None,
        FlyingSlime,
        Star,
        PlatinumCoin,
    }

    #region 伤害数据

    // 弹幕预设伤害经容器反算后用于 Projectile.damage，保留现有难度换算。
    /// <summary>头骨弹幕伤害参数。</summary>
    private static readonly ProjectileDamageContainer _SkullDamage = new(120);
    /// <summary>头骨弹幕伤害参数。</summary>
    public static int SkullDamage => _SkullDamage.Value;

    /// <summary>史莱姆坐骑伤害参数。</summary>
    private static readonly ProjectileDamageContainer _SlimeMountDamage = new(120);
    /// <summary>史莱姆坐骑伤害参数。</summary>
    public static int SlimeMountDamage => _SlimeMountDamage.Value;

    /// <summary>陨星伤害。</summary>
    private static readonly ProjectileDamageContainer _MeteorDamage = new(240);
    /// <summary>陨星伤害。</summary>
    public static int MeteorDamage => _MeteorDamage.Value;

    /// <summary>坠落星体伤害。</summary>
    private static readonly ProjectileDamageContainer _FallingStarDamage = new(120);
    /// <summary>坠落星体伤害。</summary>
    public static int FallingStarDamage => _FallingStarDamage.Value;

    /// <summary>铂金币伤害。</summary>
    private static readonly ProjectileDamageContainer _PlatinumCoinDamage = new(120);
    /// <summary>铂金币伤害。</summary>
    public static int PlatinumCoinDamage => _PlatinumCoinDamage.Value;

    /// <summary>金币审判伤害。</summary>
    private static readonly ProjectileDamageContainer _CoinJudgmentDamage = new(300);
    /// <summary>金币审判伤害。</summary>
    public static int CoinJudgmentDamage => _CoinJudgmentDamage.Value;

    /// <summary>中心恒星接触伤害。</summary>
    private static readonly ProjectileDamageContainer _EnergySphereDamage = new(300);
    /// <summary>中心恒星接触伤害。</summary>
    public static int EnergySphereDamage => _EnergySphereDamage.Value;

    private static readonly ProjectileDamageContainer _RedEnergySphereDamage = new(450);
    internal static int RedEnergySphereDamage => _RedEnergySphereDamage.Value;
    private static readonly ProjectileDamageContainer _EnergyLaserDamage = new(180);
    internal static int EnergyLaserDamage => _EnergyLaserDamage.Value;
    private static readonly ProjectileDamageContainer _StarburstDamage = new(240);
    internal static int StarburstDamage => _StarburstDamage.Value;
    private static readonly ProjectileDamageContainer _LargePlatinumCoinDamage = new(SpinContactDamage);

    private const int Phase1ContactDamage = 192;
    private const int SpinContactDamage = 240;
    // 原版 FTW 大师接触伤害：基础 32/20 × 4，加三分之一，再乘 1.1（原版逐步截断）。
    private const int Phase2HeadContactDamage = 187;
    internal const int Phase2HandContactDamage = 116;

    /// <summary>场地伤害。</summary>
    /// <summary>场地伤害。</summary>
    public static int ArenaDamage => SkeletronBoneArena.BoundaryDamage;

    // 二阶段法阵跳过 tML 难度倍率，常规碰撞仍有原版敌对弹幕的 2 倍倍率。
    internal const int Phase2ArenaDamage = 240 / 2;

    // 直接结算伤害，不经过 ProjectileDamageContainer 的弹幕基础伤害反算。
    /// <summary>错色光球拦截惩罚伤害。</summary>
    public const int IncorrectLightOrbDamage = 180;
    internal const int SlapStarDamage = 1;
    internal const int SlapDamage = 396;
    private const int OutsideArenaMinDamage = 30;
    private const int OutsideArenaMaxDamage = 396;
    private const int OutsideArenaRampTime = 120;

    internal static int GetOutsideArenaDamage(int time) =>
        OutsideArenaMinDamage + (OutsideArenaMaxDamage - OutsideArenaMinDamage) * Math.Min(time, OutsideArenaRampTime) / OutsideArenaRampTime;

    internal static int GetNextOutsideArenaTime(int time, bool outside) =>
        outside ? time + 1 : Math.Max(0, time - 1);
    /// <summary>伴星撞击的基础结算伤害。</summary>
    private const int CompanionShockwaveBaseDamage = 100;
    /// <summary>伴星撞击伤害随吸收光球数量增长的指数。</summary>
    private const double CompanionShockwaveDamageExponent = 1.5d;
    /// <summary>按中心恒星吸收的光球数量计算伴星撞击伤害。</summary>
    internal static int CompanionShockwaveDamage(int count) => count == 0 ? CompanionShockwaveBaseDamage
        : (int)(CompanionShockwaveBaseDamage * Math.Pow(count, CompanionShockwaveDamageExponent));

    /// <summary>飞行史莱姆的预设接触伤害。</summary>
    private static readonly NPCContactDamageContainer _FlyingSlimeContactDamage = new(192);
    /// <summary>飞行史莱姆难度缩放前的 NPC 基础接触伤害。</summary>
    public static int FlyingSlimeContactDamage
    {
        get
        {
            int damage = _FlyingSlimeContactDamage.Value;
            if (!Main.getGoodWorld || Main.GameModeInfo.IsJourneyMode)
                return damage;

            // FTW 给原版难度乘数加一，大师难度缩放后再加三分之一。
            return Main.masterMode ? damage * 9 / 16 : Main.expertMode ? damage * 2 / 3 : damage / 2;
        }
    }
    /// <summary>传说之夜手部每次受击的固定伤害。</summary>
    public const int HandIncomingDamage = 1;
    /// <summary>终幕试炼失败时直接致死所记录的伤害。</summary>
    public const int TrialFailedDamage = int.MaxValue;

    /// <summary>每层光球祝福增加的伤害比例，层数相加后统一乘算。</summary>
    private const float LightOrbDamageBonusPerStack = 0.05f;
    /// <summary>计算光球祝福的最终伤害倍率。</summary>
    internal static float GetLightOrbDamageBonusMultiplier(int count) => 1f + LightOrbDamageBonusPerStack * count;
    /// <summary>群星威压施加后的玩家输出伤害倍率。</summary>
    public const float AstralSuppressionDamageMultiplier = 0.5f;

    #endregion

    #region AI 数组映射数据与状态查询

    // 手的 SetDefaults 执行时，Master 和 CustomAIType 尚未由召唤方赋值。
    // AnomalyAI32[5].bits[0]：正在召唤手。
    public bool IsSummoningHands
    {
        get => AnomalyNPC.AnomalyAI32[5].Bits[0];
        private set
        {
            if (AnomalyNPC.AnomalyAI32[5].Bits[0] == value)
                return;
            AnomalyNPC.AnomalyAI32[5].Bits[0] = value;
            AnomalyNPC.AIChanged32[5] = true;
        }
    }

    public float SkyOrangeBlend => Phase2GlowOpacity;

    public bool AnomalyStarActive => Phase2
        || CurrentPhase == Phase.PhaseChange_1To2 && Timer1 >= PhaseChangeHealStart;

    public bool SuppressSkyClouds => Phase2
        || CurrentPhase == Phase.PhaseChange_1To2 && Timer1 >= PhaseChangeHealStart;

    public bool StarGodBlessingActive => CurrentBehavior != Behavior.Despawn
        && (Phase2 || CurrentPhase == Phase.PhaseChange_1To2 && Timer1 >= PhaseChangeBlessingTime);

    public float StarGodBlessingOpacity => !StarGodBlessingActive ? 0f : Phase2 ? 1f
        : MathHelper.Clamp((Timer1 - PhaseChangeBlessingTime) / 20f, 0f, 1f);

    // AnomalyAI32[4].i：二阶段总运行时间，独立于攻击状态机计时器。
    public int Phase2ElapsedTime
    {
        get => AnomalyNPC.AnomalyAI32[4].I;
        private set
        {
            if (AnomalyNPC.AnomalyAI32[4].I == value)
                return;
            AnomalyNPC.AnomalyAI32[4].I = value;
            AnomalyNPC.AIChanged32[4] = true;
        }
    }

    public bool Phase2DeathLocked => Phase2 && Phase2MusicElapsedTime < Mechanic7Timeline.DeathUnlockMusicTime;

    public bool DeathLocked => !Phase2 || Phase2DeathLocked;

    // AnomalyAI64[1]：竞技场弹幕的 owner + identity 网络引用。
    public Projectile ArenaProjectile
    {
        get => AnomalyNPC.GetProjectileReference(1);
        set => AnomalyNPC.SetProjectileReference(NPC, 1, value);
    }

    public bool ArenaProjectileAlive => ArenaProjectile.active && ArenaProjectile.ModProjectile is LegendaryNightArena arena && arena.Master == NPC;
    public LegendaryNightArena ArenaModProjectile => ArenaProjectile.GetModProjectile<LegendaryNightArena>();
    public bool Phase2ArenaProjectileAlive => ArenaProjectile.active
        && ArenaProjectile.ModProjectile is LegendaryNightPhase2Arena arena && arena.Master == NPC;

    public Phase CurrentPhase
    {
        get
        {
            Union32 union = AI_Union_0;
            return (Phase)union.Byte0;
        }
        set
        {
            if (CurrentPhase != value && Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
            Union32 union = AI_Union_0;
            union.Byte0 = (byte)value;
            AI_Union_0 = union;
        }
    }

    public bool Phase2 => IsPhase2(CurrentPhase);

    /// <summary>显式识别二阶段，保留历史终幕值，不依赖网络枚举的数值顺序。</summary>
    public static bool IsPhase2(Phase phase) => phase is Phase.Phase2_1 or Phase.Phase2_2
        or Phase.Phase2_3 or Phase.Phase2_4 or Phase.Phase2_5 or Phase.Phase2_6
        or Phase.Phase2_7 or Phase.Phase2_Finale;

    public bool Phase2MusicActive => CurrentPhase == Phase.PhaseChange_1To2 || Phase2;

    // 从二阶段音乐激活（转场开始）起累计的帧数，不随攻击或阶段状态重置。
    public int Phase2MusicElapsedTime => Timer5;

    public bool ShouldEnterPhase2 => NPC.LifeRatio < Phase2LifeRatio;
    public bool InvalidPhase1 => CurrentPhase == Phase.Phase1 && ShouldEnterPhase2;

    public Behavior CurrentBehavior
    {
        get
        {
            Union32 union = AI_Union_0;
            return (Behavior)union.Byte1;
        }
        set
        {
            if (CurrentBehavior != value && Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
            Union32 union = AI_Union_0;
            union.Byte1 = (byte)value;
            AI_Union_0 = union;
        }
    }

    public Spell CurrentSpell
    {
        get
        {
            Union32 union = AI_Union_0;
            return (Spell)union.Byte2;
        }
        set
        {
            if (CurrentSpell != value && Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
            Union32 union = AI_Union_0;
            union.Byte2 = (byte)value;
            AI_Union_0 = union;
        }
    }

    // byte0：符卡池剩余位掩码；byte2：首次循环是否已结束。
    public bool FirstSpellCycleCompleted
    {
        get => AI_Union_2.Byte2 != 0;
        set
        {
            Union32 union = AI_Union_2;
            union.Byte2 = (byte)(value ? 1 : 0);
            AI_Union_2 = union;
        }
    }

    public int CurrentLocalPhase
    {
        get => (int)NPC.ai[1];
        set
        {
            if (NPC.ai[1] != value)
            {
                NPC.ai[1] = value;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.netUpdate = true;
            }
        }
    }

    // 6：三次拍击的随机左右侧位图（bit3 标记已初始化）；7：本轮机制的音乐起点。
    public int Mechanic2HandPattern
    {
        get => AnomalyNPC.AnomalyAI32[6].I;
        private set
        {
            AnomalyNPC.AnomalyAI32[6].I = value;
            AnomalyNPC.AIChanged32[6] = true;
        }
    }

    public int Mechanic2StartMusicTime
    {
        get => AnomalyNPC.AnomalyAI32[7].I;
        private set
        {
            AnomalyNPC.AnomalyAI32[7].I = value;
            AnomalyNPC.AIChanged32[7] = true;
        }
    }

    public int Mechanic2Age => Phase2MusicElapsedTime - Mechanic2StartMusicTime;
    public bool Mechanic3Active => CurrentPhase == Phase.Phase2_3;
    public bool Mechanic4Active => CurrentPhase == Phase.Phase2_4;
    public bool Mechanic6Active => CurrentPhase == Phase.Phase2_6;
    public bool Mechanic7Active => CurrentPhase == Phase.Phase2_7;
    public bool StellarMechanicsActive => Mechanic6Active || Mechanic7Active;
    public bool Mechanic5Active => CurrentPhase == Phase.Phase2_5;
    public bool Phase2CoinAvatarActive => Mechanic4Active;
    public bool StarburstActive => Mechanic2Active || Mechanic3Active || Mechanic4Active || Mechanic5Active;
    public bool Phase2SlimeAvatarActive => Mechanic3Active
        && Phase2MusicElapsedTime >= Mechanic3Timeline.StartMusicTime
        && Phase2MusicElapsedTime < Mechanic3Timeline.LandingMusicTime;
    public bool Phase2SlimeJumpActive => Phase2SlimeAvatarActive && Phase2MusicElapsedTime >= Mechanic3Timeline.JumpMusicTime;

    public bool Mechanic2Active => CurrentPhase == Phase.Phase2_2;
    public bool Mechanic2InBackground => Mechanic2Active && Mechanic2Age < Mechanic2Parameters.ReturnStartAge + Mechanic2Parameters.ReturnDuration;
    public int Mechanic2StrikeSide(int strike) => (Mechanic2HandPattern & (1 << strike)) == 0 ? -1 : 1;
    public LegendaryNightPhase2Arena Phase2Arena => Phase2ArenaProjectileAlive ? ArenaProjectile.GetModProjectile<LegendaryNightPhase2Arena>() : null;

    // Phase2_1 的 LocalPhase：0 自转，1 常态星星雨，2 史莱姆，3 叠加恶魔之影，4 再叠加铂金币。
    public bool Mechanic1Spinning => CurrentPhase == Phase.Phase2_1 && CurrentLocalPhase == 0;
    public bool Phase2SlimeRainActive => CurrentPhase == Phase.Phase2_1 && CurrentLocalPhase >= 2
        && Phase2MusicElapsedTime < Mechanic1Timeline.SlimeEndMusicTime;
    public bool Phase2DemonShadowActive => CurrentPhase == Phase.Phase2_1 && CurrentLocalPhase >= 3
        && Phase2MusicElapsedTime < Mechanic1Timeline.DemonShadowEndMusicTime;
    private bool Phase2PlatinumCoinActive => CurrentPhase == Phase.Phase2_1 && CurrentLocalPhase >= 4
        && Phase2MusicElapsedTime < Mechanic1Timeline.PlatinumCoinEndMusicTime;
    private bool Phase2StarRainActive => CurrentPhase == Phase.Phase2_1 && CurrentLocalPhase >= 1
        && Phase2MusicElapsedTime < Mechanic1Timeline.StarRainEndMusicTime;
    public bool Phase2ArenaRequested => Phase2 && CurrentPhase != Phase.Phase2_1;
    public int Phase2ArenaStartMusicTime => Mechanic1Timeline.EndMusicTime;

    public bool HandsHidden => Mechanic3Active || Mechanic4Active || Mechanic2Active && Mechanic2Age >= Mechanic2Parameters.ReturnStartAge;

    // AnomalyAI32[1].byte0：当前化身。AnomalyAI32[2].i：当前化身持续帧数。
    public Avatar CurrentAvatar
    {
        get => (Avatar)AnomalyNPC.AnomalyAI32[1].Byte0;
        private set
        {
            if (AnomalyNPC.AnomalyAI32[1].Byte0 != (byte)value)
            {
                AnomalyNPC.AnomalyAI32[1].Byte0 = (byte)value;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }

    public int AvatarTimer
    {
        get => AnomalyNPC.AnomalyAI32[2].I;
        private set
        {
            if (AnomalyNPC.AnomalyAI32[2].I != value)
            {
                AnomalyNPC.AnomalyAI32[2].I = value;
                AnomalyNPC.AIChanged32[2] = true;
            }
        }
    }

    // AnomalyAI32[3].f：化身独立旋转角，与 NPC.rotation 无关。
    public float AvatarRotation
    {
        get => AnomalyNPC.AnomalyAI32[3].F;
        private set
        {
            if (AnomalyNPC.AnomalyAI32[3].F != value)
            {
                AnomalyNPC.AnomalyAI32[3].F = value;
                AnomalyNPC.AIChanged32[3] = true;
            }
        }
    }

    public bool StarAvatarActive => CurrentAvatar == Avatar.Star;

    #region 二阶段复用槽位

    // AI64[2]：停靠/拍击/跳跃/回中起点；AI64[3]：终点、落点或冲刺速度。
    // AI64[4]：停靠初速度，仅机制一停靠期间有效。AI64[1] 保留竞技场引用。
    // AI32[8]：停靠/拍击锁定状态，机制三复用为雨目标索引 + 1。
    // 别名只在所属机制内有效；服务器先锁定数据再推进局部步骤。
    /// <summary>机制一停靠起点，复用运动起点槽。</summary>
    private Vector2 ArenaApproachOrigin
    {
        get => ReadPhase2Vector(2);
        set => WritePhase2Vector(2, value);
    }

    /// <summary>机制一停靠终点，复用运动终点槽。</summary>
    private Vector2 ArenaApproachDestination
    {
        get => ReadPhase2Vector(3);
        set => WritePhase2Vector(3, value);
    }

    /// <summary>机制一停靠的初始速度，制动完成后不再读取。</summary>
    private Vector2 ArenaApproachInitialVelocity
    {
        get => ReadPhase2Vector(4);
        set => WritePhase2Vector(4, value);
    }

    /// <summary>机制二当前拍击的锁定起点。</summary>
    private Vector2 Mechanic2StrikeOrigin
    {
        get => ReadPhase2Vector(2);
        set => WritePhase2Vector(2, value);
    }

    /// <summary>机制二当前拍击的锁定目标。</summary>
    private Vector2 Mechanic2StrikeTarget
    {
        get => ReadPhase2Vector(3);
        set => WritePhase2Vector(3, value);
    }

    /// <summary>机制三起跳时锁定的起点。</summary>
    private Vector2 Mechanic3JumpOrigin
    {
        get => ReadPhase2Vector(2);
        set => WritePhase2Vector(2, value);
    }

    /// <summary>机制三起跳时锁定的落地位置。</summary>
    private Vector2 Mechanic3LandingPosition
    {
        get => ReadPhase2Vector(3);
        set => WritePhase2Vector(3, value);
    }

    /// <summary>机制四最后一次冲刺后的回中起点。</summary>
    private Vector2 Mechanic4ReturnOrigin
    {
        get => ReadPhase2Vector(2);
        set => WritePhase2Vector(2, value);
    }

    /// <summary>机制四当前冲刺的锁定速度。</summary>
    private Vector2 Mechanic4ChargeVelocity
    {
        get => ReadPhase2Vector(3);
        set => WritePhase2Vector(3, value);
    }

    /// <summary>机制一停靠锁定状态：-2 为仪式停靠点，-1 为刹停锁定。</summary>
    private int ArenaApproachState
    {
        get => AnomalyNPC.AnomalyAI32[8].I;
        set
        {
            AnomalyNPC.AnomalyAI32[8].I = value;
            AnomalyNPC.AIChanged32[8] = true;
        }
    }

    /// <summary>机制二已锁定的拍击序号加一，零表示尚未锁定。</summary>
    private int Mechanic2LockedStrike
    {
        get => AnomalyNPC.AnomalyAI32[8].I;
        set
        {
            AnomalyNPC.AnomalyAI32[8].I = value;
            AnomalyNPC.AIChanged32[8] = true;
        }
    }

    /// <summary>机制三本轮史莱姆雨的玩家索引加一，复用已结束的停靠/拍击状态槽。</summary>
    private int Mechanic3RainTargetIndex
    {
        get => AnomalyNPC.AnomalyAI32[8].I;
        set
        {
            AnomalyNPC.AnomalyAI32[8].I = value;
            AnomalyNPC.AIChanged32[8] = true;
        }
    }

    // 不触碰竞技场引用、化身、音乐时钟和生存时钟。
    private void ResetPhase2SharedState()
    {
        ArenaApproachState = 0;
        WritePhase2Vector(2, Vector2.Zero);
        WritePhase2Vector(3, Vector2.Zero);
        WritePhase2Vector(4, Vector2.Zero);
    }

    /// <summary>从复用的 64 位同步槽读取完整二维向量。</summary>
    private Vector2 ReadPhase2Vector(int slot) => AnomalyNPC.AnomalyAI64[slot].GetValue<Vector2>();

    /// <summary>写入完整二维向量并标记对应 64 位同步槽。</summary>
    private void WritePhase2Vector(int slot, Vector2 value)
    {
        AnomalyNPC.AnomalyAI64[slot].SetValue(value);
        AnomalyNPC.AIChanged64[slot] = true;
    }

    #endregion

    #endregion
}