namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 传说之夜手部 Behavior；状态由对应 NPC 数据容器持有。
/// </summary>
/// <remarks>
/// <para>ai[0]：左右侧（-1/+1）；ai[1]：主体 NPC 的0-based索引（NPC.Master 同时通过 OceanAI32[3].I 保存主人）；ai[2]：常态动作或机制二/五/六脚本动作；ai[3]：常态周期帧，机制五复用为已锁定攻击序号 + 1。</para>
/// <para>脚本 ai[2]：0待机、1蓄势、2前挥、3收手、4托球准备、6机制二入场（避开常态动作的0..5）；常态 ai[2]：0待机、1抬手、2拍击、3恢复、4横扫蓄势、5横扫。</para>
/// <para>AnomalyAI64[0]：常态/机制五挥击起点，机制五蓄势时复用为后撤方向，托球准备及机制二入场时复用为相对头部的起点；[1]：常态/机制五锁定终点，机制二入场时复用为初速度。完整向量以一个 64 位槽同步。</para>
/// <para>localAI、框架 Timer1..5、AnomalyAI32：本行为未使用；手部时序读取主体计时或音乐时钟。</para>
/// <para>Skeletron_HandBloodDust 为统一 GlobalNPC 中的客户端视觉集合，克隆重置、不参与网络同步。</para>
/// </remarks>
public sealed partial class SkeletronLegendHand : AnomalyNPCBehavior<SkeletronLegendHand>
{
    /// <summary>一阶段和常态二阶段共用的手部动作，映射原版 ai[2]。</summary>
    private enum NormalAction : byte
    {
        Idle = 0,
        Raise = 1,
        Slam = 2,
        Recover = 3,
        SweepWindup = 4,
        Sweep = 5,
    }

    /// <summary>机制二/五/六的脚本动作，与常态动作互斥复用 ai[2]。</summary>
    private enum ScriptedAction : byte
    {
        Idle = 0,
        Windup = 1,
        Strike = 2,
        Return = 3,
        PrepareSphere = 4,
        Mechanic2Entry = 6,
    }

    protected override int AI64Capacity => 2;

    /// <summary>攻击周期持续时间，单位为游戏帧。</summary>
    private const int AttackCycleDuration = 300;
    /// <summary>常态攻击周期内开始蓄势的相对帧。</summary>
    private const int WindupStart = 150;
    /// <summary>常态攻击周期内开始拍击或横扫的相对帧。</summary>
    private const int StrikeTime = 210;
    /// <summary>最小拍击持续时间，单位为游戏帧。</summary>
    private const int MinimumStrikeDuration = 20;
    /// <summary>最大拍击持续时间，单位为游戏帧。</summary>
    private const int MaximumStrikeDuration = 75;
    /// <summary>越过出手时玩家位置后继续挥动的帧数。</summary>
    private const int StrikeFollowThroughDuration = 8;
    private const float StrikeSpeed = 28f;

    #region AI 数组映射数据

    // ai[0/1] 保留左右侧和主人；ai[2] 为动作，ai[3] 按常态/脚本解释。
    // AI64[0/1] 在常态攻击和机制二/五/六之间复用，由服务器在动作入口重新锁定。
    /// <summary>常态/机制五挥击起点；机制六准备时复用为相对主体的位置。</summary>
    private Vector2 StrikeOrigin
    {
        get => AnomalyNPC.AnomalyAI64[0].GetValue<Vector2>();
        set
        {
            AnomalyNPC.AnomalyAI64[0].SetValue(value);
            AnomalyNPC.AIChanged64[0] = true;
        }
    }

    /// <summary>常态/机制五服务器锁定的挥击终点。</summary>
    private Vector2 StrikeDestination
    {
        get => AnomalyNPC.AnomalyAI64[1].GetValue<Vector2>();
        set
        {
            AnomalyNPC.AnomalyAI64[1].SetValue(value);
            AnomalyNPC.AIChanged64[1] = true;
        }
    }

    /// <summary>星爆开始时锁定的后撤方向，挥击前复用 AI64[0]。</summary>
    private Vector2 Mechanic5WindupDirection
    {
        get => StrikeOrigin;
        set => StrikeOrigin = value;
    }

    /// <summary>机制二入场的相对起点，复用 AI64[0]。</summary>
    private Vector2 Mechanic2EntryOffset
    {
        get => StrikeOrigin;
        set => StrikeOrigin = value;
    }

    /// <summary>机制二入场的初速度，复用 AI64[1]。</summary>
    private Vector2 Mechanic2EntryVelocity
    {
        get => StrikeDestination;
        set => StrikeDestination = value;
    }

    // ai[0]/ai[1] 继续保留原版左右侧与主人索引。
    private int HandSide => -Math.Sign(NPC.ai[0]);

    // 相同槽位在两套互斥行为中有不同含义，分别命名，不合并动作枚举。
    private NormalAction CurrentNormalAction
    {
        get => (NormalAction)(int)NPC.ai[2];
        set => NPC.ai[2] = (byte)value;
    }

    private ScriptedAction CurrentScriptedAction
    {
        get => (ScriptedAction)(int)NPC.ai[2];
        set => NPC.ai[2] = (byte)value;
    }

    private int Mechanic5LockedAttack
    {
        get => (int)NPC.ai[3];
        set => NPC.ai[3] = value;
    }

    // 机制6准备阶段复用起点槽位保存相对主人偏移，不另存实例状态。
    private Vector2 StellarPreparationOffset
    {
        get => StrikeOrigin;
        set => StrikeOrigin = value;
    }

    #endregion

    public NPC Master => NPC.Master;

    internal static bool IsOwnedBy(NPC hand, NPC head) => hand.type == NPCID.SkeletronHand && (int)hand.ai[1] == head.whoAmI;
    public override int ApplyingType => NPCID.SkeletronHand;

    public override decimal Priority => 110m;
    public override bool ShouldProcess
    {
        get
        {
            if (!AnomalyBossChangeToggler.LegendaryNight || !base.ShouldProcess)
                return false;
            if (AnomalyNPC.CustomAIType == 1)
                return true;

            // 参照召唤仪式，在 SetDefaults 等早期环节识别正在生成的手。
            foreach (NPC head in NPC.ActiveNPCs)
            {
                if (LegendaryNightHandler.IsLegendaryNightHead(head)
                    && SkeletronLegendHead.GetInstance(head).IsSummoningHands)
                    return true;
            }
            return false;
        }
    }

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI or CalamityLogicType_NPCBehavior.PreAI or CalamityLogicType_NPCBehavior.GetAlpha => false,
        _ => true
    };

    public override void SetDefaultsFinal()
    {
        NPC.lifeMax = 2970; //3960
        NPC.defense = 9999;
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) =>
        AnomalyUtils.ApplyAnomalyBossDifficultyAndPlayerScaling(NPC, bossAdjustment);

    public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
    {
        modifiers.SetMaxDamage(SkeletronLegendHead.HandIncomingDamage);
        modifiers.ModifyHitInfo += static (ref hit) => hit.Damage = SkeletronLegendHead.HandIncomingDamage;
    }

    public override bool CanHitPlayer(Player target, ref int cooldownSlot) =>
        NPC.TryGetMaster(NPCID.SkeletronHead, out NPC master)
        && LegendaryNightHandler.IsLegendaryNightHead(master) && SkeletronLegendHead.GetInstance(master) is SkeletronLegendHead { Phase2: true, Mechanic2InBackground: false, HandsHidden: false } head
        && !head.StellarMechanicsActive
        && (!head.Mechanic5Active || head.Phase2MusicElapsedTime >= SkeletronLegendHead.Mechanic5Timeline.StartMusicTime
            + SkeletronLegendHead.Mechanic5Parameters.HandReappearanceDuration);

    public override void Load() => BossDeathEffectRegistry.Register(NPCID.SkeletronHand,
        npc => GetInstance(npc).ShouldProcess);

    public override bool CheckDead()
    {
        int increasedLife = NPC.lifeMax - NPC.life;
        NPC.life = NPC.lifeMax;
        if (increasedLife > 0)
            NPC.HealEffect(increasedLife, true);
        NPC.active = true;
        NPC.netUpdate = true;
        return false;
    }

    internal static void HideOwnedHands(NPC master)
    {
        foreach (NPC hand in NPC.ActiveNPCs)
        {
            if (hand.type != NPCID.SkeletronHand || (int)hand.ai[1] != master.whoAmI)
                continue;
            hand.hide = true;
            hand.Anomaly.Skeletron_HandBloodDust = null;
        }
    }
    internal static void RevealOwnedHands(NPC master)
    {
        foreach (NPC hand in NPC.ActiveNPCs)
        {
            if (hand.type != NPCID.SkeletronHand || (int)hand.ai[1] != master.whoAmI)
                continue;
            hand.Center = master.Center;
            hand.velocity = Vector2.Zero;
            hand.ai[2] = hand.ai[3] = 0f;
            hand.target = master.target;
            hand.hide = false;
            hand.damage = 0;
            hand.Anomaly.Z = 0f;
            hand.netUpdate = true;
        }
    }

    /// <summary>复用普通骷髅王手部的待机位置。</summary>
    private Vector2 GetIdleOffset(int side) => SkeletronKinematics.GetIdleOffset(side, NPC.height, Master.height);

    /// <summary>手掌沿第一段骨节连接轴自然下垂。</summary>
    private static float GetRestingRotation(Vector2 handCenter, Vector2 headCenter, int side, int handHeight)
    {
        Vector2 wrist = handCenter + new Vector2(side * 5f, 20f - handHeight * 0.5f);
        Vector2 forearm = headCenter + new Vector2(side * 200f, 130f) - wrist;
        return MathHelper.WrapAngle(forearm.ToRotation() + MathHelper.PiOver2);
    }

    /// <summary>沿用原版 atan2(参考点-手部)+π/2；参考点取原版第二段骨节的 (±50,80)，使手掌顺接前臂。</summary>
    public static float GetPositionRotation(Vector2 handCenter, Vector2 headCenter, int side)
    {
        Vector2 direction = headCenter + new Vector2(side * 50f, 80f) - handCenter;
        return direction.LengthSquared() > 0.001f ? MathHelper.WrapAngle(direction.ToRotation() + MathHelper.PiOver2) : 0f;
    }

}
