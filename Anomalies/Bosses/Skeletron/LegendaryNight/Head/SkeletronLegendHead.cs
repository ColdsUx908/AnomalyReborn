namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 传说之夜主体 Behavior；所有实体状态由 NPC 数据容器持有。
/// </summary>
/// <remarks>
/// <para>ai[0]（Union32）：Byte0阶段、Byte1行为、Byte2当前符卡；ai[1]：局部步骤；ai[2]（Union32）：Byte0剩余符卡位掩码、Byte2首轮完成标志；ai[3]：未使用。</para>
/// <para>localAI：本行为未使用；Timer1（OceanAI32[4].I）：行为/局部步骤累计帧；Timer2（OceanAI32[5].I）：转场回血初始生命；Timer5（OceanAI32[8].I）：转场起累计音乐帧；Timer3/4未使用。</para>
/// <para>AnomalyAI32[0]：未使用；[1].Byte0：化身；[2].I：化身帧；[3].F：化身旋转；[4].I：二阶段生存帧；[5].Bits[0]：正在召手。</para>
/// <para>AnomalyAI32[6].I：拍击左右位图（bit3表示初始化）；[7].I：机制二音乐起点。</para>
/// <para>AnomalyAI32[8].I：停靠锁定状态、已锁定拍击序号或机制三雨目标玩家索引 + 1，按机制复用。</para>
/// <para>AnomalyAI64[1]：竞技场 owner + identity；[2]：停靠/拍击/跳跃/回中起点；[3]：终点、落点或冲刺速度；[4]：停靠初速度。[0]未使用。</para>
/// <para>Skeletron_LegendaryNightHalfFieldHits：本机已消费的半场拍击位图，不加入网络快照，实体初始化及克隆时重置。</para>
/// </remarks>
public sealed partial class SkeletronLegendHead : AnomalyNPCBehavior<SkeletronLegendHead>
{
    // AI32[0..8] 为标量状态；AI64[1] 为竞技场引用，[2..4] 为复用向量。
    protected override int AI32Capacity => 9;
    protected override int AI64Capacity => 5;

    /// <summary>目标有效性的最大距离，单位为世界像素。</summary>
    public const float DespawnDistance = 10000f;
    /// <summary>进入二阶段的剩余生命比例阈值。</summary>
    public const float Phase2LifeRatio = 0.1f;
    /// <summary>二阶段动态减伤采用的预期生存秒数。</summary>
    public const int Phase2ExpectedTimeInSeconds = 180;
    /// <summary>二阶段动态减伤的累计生存帧上限。</summary>
    public const int Phase2MinimumDuration = Phase2ExpectedTimeInSeconds * 60;

    public override int ApplyingType => NPCID.SkeletronHead;

    public override bool CanHitPlayer(Player target, ref int cooldownSlot) => CurrentPhase != Phase.PhaseChange_1To2 && !Mechanic2InBackground;

    public override decimal Priority => 10m;

    public override bool ShouldProcess
    {
        get
        {
            if (!base.ShouldProcess)
                return false;

            if (AnomalyNPC?.CustomAIType == 1)
                return true;

            //保险机制：在SetDefaults等环节使用，检测当前是否有召唤仪式处于召唤状态
            foreach (Projectile p in Projectile.ActiveProjectiles)
            {
                if (p.ModProjectile is LegendaryNightRitual modP && modP.IsSummoningSkeletron)
                    return true;
            }

            return false;
        }
    }

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };

    public override void SetDefaults()
    {
        AnomalyNPC.Skeletron_LegendaryNightHalfFieldHits = 0;
        ArenaProjectile = Projectile.DummyProjectile;
        CurrentAvatar = Avatar.None;
        AvatarTimer = 0;
        AvatarRotation = 0f;
        Phase2ElapsedTime = 0;
        Timer5 = 0;
        IsSummoningHands = false;

        NPC.AddAnomalyHPIndicator(Phase2LifeRatio, Phase2LifeRatio);
        AnomalyNPC.DamageReduction_DynamicHandler = new TimedDDRHandler(
            new TimedDDRHandler.SingleDDRHandler(1f, 0f,
                n => GetInstance(n).Phase2 && GetInstance(n).Phase2ElapsedTime > 0,
                n => GetInstance(n).Phase2ElapsedTime >= Phase2MinimumDuration,
                Phase2ExpectedTimeInSeconds, storyModeOnly: false));
    }

    public override void SetDefaultsFinal()
    {
        NPC.lifeMax = CalamityEnabled ? 9000 : 7500;
        BridgeUtils.ApplyCalamityHealthBoost(NPC);
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) =>
        AnomalyUtils.ApplyAnomalyBossDifficultyAndPlayerScaling(NPC, bossAdjustment);

    public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
    {
        if (!Phase2)
            modifiers.SetMaxDamage(Math.Max(1, (int)(NPC.life - NPC.lifeMax * Phase2LifeRatio + 1)));
        else if (Phase2DeathLocked)
            modifiers.SetMaxDamage(Math.Max(1, NPC.life - 1));
    }

    public override void Load() => BossDeathEffectRegistry.Register(NPCID.SkeletronHead,
        npc => GetInstance(npc) is { ShouldProcess: true, DeathLocked: true });

    public override bool CheckDead()
    {
        if (DeathLocked)
        {
            NPC.life = 1;
            NPC.active = true;
            NPC.netUpdate = true;
            return false;
        }
        return true;
    }
}