using Anomalies.Bosses.BrainofCthulhu;
using Anomalies.Bosses.EaterofWorlds;
using Anomalies.Bosses.Skeletron.LegendaryNight;
using Anomalies.Visuals.BossBar;
using Transoceanic.Framework.Helpers.Utilities;

namespace Anomalies.Common;

public sealed class AnomalyPlayer : ModPlayer
{
    // 同时覆盖荆棘、冲撞等经由 Player.ApplyDamageToNPC 结算的特殊攻击。
    public override bool CanHitNPC(NPC target) => AnomalyDepthDrawSystem.IsVisible(target.Anomaly.Z);

    //数据变量按字母顺序排列
    public int Coldheart_Phase;
    public int Coldheart_SubPhase;

    public bool Debuff_DimensionalRend;

    public PlayerDownedBoss DownedBoss = new();

    public float DarknessIntensity
    {
        get;
        set => field = MathHelper.Clamp(value, 0f, 1f);
    }

    // 落日遗逢的本地输入与飘带视觉状态，不参与网络快照。
    public bool EventideReunionRightWasDown;
    public int EventideReunionSunCooldown;
    public int EventideReunionDeathCooldown;
    public int EventideReunionDeathProtection;
    public int EventideReunionSunFallProtection;

    internal Vector2[][] EventideReunionStreamerPositions
    {
        get => field ??= [new Vector2[33], new Vector2[33]];
        private set;
    }

    internal Vector2[][] EventideReunionStreamerPrevious
    {
        get => field ??= [new Vector2[33], new Vector2[33]];
        private set;
    }

    internal Vector2[] EventideReunionStreamerAnchors
    {
        get => field ??= new Vector2[2];
        private set;
    }

    internal ulong EventideReunionStreamerLastUpdate;
    internal bool EventideReunionStreamerInitialized;

    public Item LegendaryNightVirtualBoneWings;

    public bool Minion_VacuousBlack;

    public bool StarGodBlessingActive;
    public float StarGodBlessingOpacity;

    public override ModPlayer Clone(Player newEntity)
    {
        AnomalyPlayer clone = (AnomalyPlayer)base.Clone(newEntity);

        clone.Coldheart_Phase = Coldheart_Phase;
        clone.Coldheart_SubPhase = Coldheart_SubPhase;

        clone.Debuff_DimensionalRend = Debuff_DimensionalRend;
        clone.DownedBoss = DownedBoss;

        clone.DarknessIntensity = DarknessIntensity;
        clone.EventideReunionRightWasDown = EventideReunionRightWasDown;
        clone.EventideReunionSunCooldown = EventideReunionSunCooldown;
        clone.EventideReunionDeathCooldown = EventideReunionDeathCooldown;
        clone.EventideReunionDeathProtection = EventideReunionDeathProtection;
        clone.EventideReunionSunFallProtection = EventideReunionSunFallProtection;
        clone.EventideReunionStreamerPositions = null;
        clone.EventideReunionStreamerPrevious = null;
        clone.EventideReunionStreamerAnchors = null;
        clone.EventideReunionStreamerLastUpdate = 0;
        clone.EventideReunionStreamerInitialized = false;
        clone.LegendaryNightVirtualBoneWings = LegendaryNightVirtualBoneWings?.Clone();
        clone.StarGodBlessingActive = StarGodBlessingActive;
        clone.StarGodBlessingOpacity = StarGodBlessingOpacity;

        return clone;
    }

    public override void ResetEffects()
    {
        Debuff_DimensionalRend = false;
        Minion_VacuousBlack = false;
        StarGodBlessingActive = false;
        StarGodBlessingOpacity = 0f;
        DarknessIntensity = MathHelper.Max(DarknessIntensity - 0.05f, 0);
    }
}

public sealed class AnomalyGlobalNPC : GlobalNPC, IContentLoader
{
    public int EventideReunionDeathOwner = -1;
    // 仅在回斩/波的伤害结算期间有效，供致死回调使用，不参与网络快照。
    public int EventideReunionDeathHitOwner = -1;
    public override bool InstancePerEntity => true;

#if DEBUG
    /// <summary>
    /// 调试用数据。
    /// <br/>不同实体可能会有不同的用途。
    /// </summary>
    public readonly Union64[] DebugData = new Union64[4];
#endif

    private const int AISlot = 66;
    private const int AISlot2 = 33;
    private const int AISlot3 = 33;
    private const int AISlot4 = 17;

    public Union32[] AnomalyAI32 { get; private set; } = new Union32[AISlot];
    public Union64[] AnomalyAI64 { get; private set; } = new Union64[AISlot2];

    /// <summary>
    /// 特殊标志位，供外部判定使用。
    /// <br/>每帧都会被重置为默认值，外部判定需要在每帧内使用。
    /// </summary>
    public BitArray64 SpecialFlags = default;

    public ref BitArray32 AIChanged32 => ref AnomalyAI32[^2].bits;
    public ref BitArray32 AIChanged32_2 => ref AnomalyAI32[^1].bits;
    public ref BitArray64 AIChanged64 => ref AnomalyAI64[^1].bits;

    // Per-NPC local lookup hints. The authoritative owner/identity remains in AnomalyAI64.
    private ProjectileNetReference[] _ProjectileReferenceCache = new ProjectileNetReference[AISlot2 - 1];

    public Projectile GetProjectileReference(int slot)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(slot, _ProjectileReferenceCache.Length);
        ref ProjectileNetReference reference = ref _ProjectileReferenceCache[slot];
        reference.SetPackedValue(AnomalyAI64[slot].l);
        return reference.Resolve();
    }

    public void SetProjectileReference(NPC npc, int slot, Projectile projectile)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(slot, _ProjectileReferenceCache.Length);
        ref ProjectileNetReference reference = ref _ProjectileReferenceCache[slot];
        reference.Set(projectile);
        long packed = reference.PackedValue;
        if (AnomalyAI64[slot].l == packed)
            return;
        AnomalyAI64[slot].l = packed;
        AIChanged64[slot] = true;
        if (TOSharedData.NotClient)
            npc.netUpdate = true;
    }

    // The worm's segment graph is server-local entity state, not singleton Behavior state.
    internal List<EaterofWorldsSegmentContainer> EaterOfWorldsSegments { get; private set; } = [];

    // 克脑的客户端绘制数据，不参与网络快照。
    public List<BrainOfCthulhuAfterImage> BrainAfterImages = [];
    public Rectangle BrainFrame = new(0, 0, 198, 180);

    // 仅客户端绘制使用，不同步，也不在单例 Behavior 上保存实体状态。
    internal SkeletronHandBloodDust HandBloodDust;

    /// <summary>本帧的视觉锚点：实体局部位置、未投影的世界目标和混合权重。仅影响绘制。</summary>
    internal (Vector2 LocalOffset, Vector2 Target, float Weight)? DepthDrawAnchor;

    private Union32[] InternalAnomalyAI32 { get; set; } = new Union32[AISlot3];
    private Union64[] InternalAnomalyAI64 { get; set; } = new Union64[AISlot4];

    private ref BitArray32 InternalAIChanged32 => ref InternalAnomalyAI32[^1].bits;
    private ref BitArray64 InternalAIChanged64 => ref InternalAnomalyAI64[^1].bits;

    public override void ResetEffects(NPC npc)
    {
        SpecialFlags = default;
        DepthDrawAnchor = null;
    }

    public override GlobalNPC Clone(NPC from, NPC to)
    {
        AnomalyGlobalNPC clone = (AnomalyGlobalNPC)base.Clone(from, to);

        clone.AnomalyAI32 = (Union32[])AnomalyAI32.Clone();
        clone.AnomalyAI64 = (Union64[])AnomalyAI64.Clone();
        clone.InternalAnomalyAI32 = (Union32[])InternalAnomalyAI32.Clone();
        clone.InternalAnomalyAI64 = (Union64[])InternalAnomalyAI64.Clone();
        clone._ProjectileReferenceCache = (ProjectileNetReference[])_ProjectileReferenceCache.Clone();
        clone.EaterOfWorldsSegments = [with(EaterOfWorldsSegments)];
        clone.BrainAfterImages = [];
        clone.BrainFrame = BrainFrame;
        clone.HandBloodDust = null;
        clone.DepthDrawAnchor = null;

        return clone;
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        TONetUtils.WriteAI32Snapshot(binaryWriter, AnomalyAI32, 2);
        TONetUtils.WriteAI64Snapshot(binaryWriter, AnomalyAI64, 1);
        TONetUtils.WriteAI32Snapshot(binaryWriter, InternalAnomalyAI32, 1);
        TONetUtils.WriteAI64Snapshot(binaryWriter, InternalAnomalyAI64, 1);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        TONetUtils.ReadAI32Snapshot(binaryReader, AnomalyAI32, 2);
        TONetUtils.ReadAI64Snapshot(binaryReader, AnomalyAI64, 1);
        TONetUtils.ReadAI32Snapshot(binaryReader, InternalAnomalyAI32, 1);
        TONetUtils.ReadAI64Snapshot(binaryReader, InternalAnomalyAI64, 1);
    }

    #region 额外数据
    /// <summary>
    /// 深度（世界像素）。0 为正常平面，正值向屏幕后方远离，负值向屏幕前方靠近。
    /// <br/>不改变世界位置；玩家武器和弹幕使用投影碰撞箱，越过观察者平面后不可被击中。
    /// <br/>NPC 之间的接触伤害仍要求 Z=0，避免使用某个客户端的镜头决定服务端碰撞。
    /// <br/>持续保留，回到正常战斗平面时应设为 0。
    /// <br/>随 NPC 额外 AI 同步；由服务端修改时按需设置 npc.netUpdate。
    /// </summary>
    public float Z
    {
        get => InternalAnomalyAI32[3].f;
        set
        {
            if (!float.IsFinite(value))
                throw new ArgumentOutOfRangeException(nameof(value));
            if (InternalAnomalyAI32[3].f != value)
            {
                InternalAnomalyAI32[3].f = value;
                InternalAIChanged32[3] = true;
            }
        }
    }

    public override bool? CanBeHitByItem(NPC npc, Player player, Item item) => AnomalyDepthDrawSystem.IsVisible(Z) ? null : false;

    public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile) => AnomalyDepthDrawSystem.IsVisible(Z) ? null : false;

    public override bool CanBeHitByNPC(NPC npc, NPC attacker) => Z == 0f;

    public bool ShouldRunAnomalyAI
    {
        get => InternalAnomalyAI32[0].bits[0];
        set
        {
            if (InternalAnomalyAI32[0].bits[0] != value)
            {
                InternalAnomalyAI32[0].bits[0] = value;
                InternalAIChanged32[0] = true;
            }
        }
    }

    public bool Debuff_DimensionalRend
    {
        get => InternalAnomalyAI32[1].bits[0];
        set
        {
            if (InternalAnomalyAI32[1].bits[0] != value)
            {
                InternalAnomalyAI32[1].bits[0] = value;
                InternalAIChanged32[1] = true;
            }
        }
    }

    /// <summary>
    /// 自定义AI类型，仅供NPC的变种AI使用。
    /// <br/>如果不使用变种AI，请不要修改此值。
    /// </summary>
    public int CustomAIType
    {
        get => InternalAnomalyAI32[2].i;
        set
        {
            if (InternalAnomalyAI32[2].i != value)
            {
                InternalAnomalyAI32[2].i = value;
                InternalAIChanged32[2] = true;
            }
        }
    }

    /// <summary>
    /// 伤害减免，在计算伤害时会被应用。
    /// </summary>
    /// <remarks>谨慎使用。</remarks>
    public float DR
    {
        get => InternalAnomalyAI32[4].f;
        set
        {
            if (InternalAnomalyAI32[4].f != value)
            {
                InternalAnomalyAI32[4].f = value;
                InternalAIChanged32[4] = true;
            }
        }
    }

    /// <summary>
    /// 额外DR，不受任何修改DR的机制影响。
    /// </summary>
    /// <remarks>谨慎使用。</remarks>
    public float ExtraDR
    {
        get => InternalAnomalyAI32[5].f;
        set
        {
            if (InternalAnomalyAI32[5].f != value)
            {
                InternalAnomalyAI32[5].f = value;
                InternalAIChanged32[5] = true;
            }
        }
    }

    #region 本地数据
    public int AnomalyAITimer;

    public bool IsRunningAnomalyAI => AnomalyAITimer > 0;

    public int AnomalyUltraAITimer;
    public int AnomalyUltraBarTimer;

    public bool CanHaveBossHealthBar;
    public bool CurrentlyIncreasingDefenseOrDR;
    public bool CurrentlyEnraged;
    public bool CurrentlyImmune;
    public bool BossHealthBarIsSmall;

    public List<HPThresholdIndicator> HPThresholdIndicators = [];

    public IDynamicDRHandler DynamicDRHandler;
    #endregion 本地数据
    #endregion 额外数据
}

public sealed class AnomalyGlobalProjectile : GlobalProjectile
{
    public override bool InstancePerEntity => true;

#if DEBUG
    /// <summary>
    /// 调试用数据。
    /// <br/>不同实体可能会有不同的用途。
    /// </summary>
    public readonly Union64[] DebugData = new Union64[4];
#endif

    private const int AISlot = 33;
    private const int AISlot2 = 17;

    public Union32[] AnomalyAI32 { get; private set; } = new Union32[AISlot];
    public Union64[] AnomalyAI64 { get; private set; } = new Union64[AISlot2];

    public ref BitArray32 AIChanged32 => ref AnomalyAI32[^1].bits;
    public ref BitArray64 AIChanged64 => ref AnomalyAI64[^1].bits;

    private Union32[] InternalAnomalyAI32 { get; set; } = new Union32[AISlot];
    private Union64[] InternalAnomalyAI64 { get; set; } = new Union64[AISlot2];

    private ref BitArray32 InternalAIChanged32 => ref InternalAnomalyAI32[^1].bits;
    private ref BitArray64 InternalAIChanged64 => ref InternalAnomalyAI64[^1].bits;

    public override GlobalProjectile Clone(Projectile from, Projectile to)
    {
        AnomalyGlobalProjectile clone = (AnomalyGlobalProjectile)base.Clone(from, to);

        clone.AnomalyAI32 = (Union32[])AnomalyAI32.Clone();
        clone.AnomalyAI64 = (Union64[])AnomalyAI64.Clone();
        clone.InternalAnomalyAI32 = (Union32[])InternalAnomalyAI32.Clone();
        clone.InternalAnomalyAI64 = (Union64[])InternalAnomalyAI64.Clone();

        return clone;
    }

    public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        // SyncProjectile does not include these fields, even when netUpdate is true.
        binaryWriter.Write(projectile.scale);
        binaryWriter.Write(projectile.timeLeft);
        binaryWriter.Write(projectile.extraUpdates);
        bitWriter.WriteBit(projectile.tileCollide);
        TONetUtils.WriteAI32Snapshot(binaryWriter, AnomalyAI32, 1);
        TONetUtils.WriteAI64Snapshot(binaryWriter, AnomalyAI64, 1);
        TONetUtils.WriteAI32Snapshot(binaryWriter, InternalAnomalyAI32, 1);
        TONetUtils.WriteAI64Snapshot(binaryWriter, InternalAnomalyAI64, 1);
    }

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
    {
        projectile.scale = binaryReader.ReadSingle();
        projectile.timeLeft = binaryReader.ReadInt32();
        projectile.extraUpdates = binaryReader.ReadInt32();
        projectile.tileCollide = bitReader.ReadBit();
        TONetUtils.ReadAI32Snapshot(binaryReader, AnomalyAI32, 1);
        TONetUtils.ReadAI64Snapshot(binaryReader, AnomalyAI64, 1);
        TONetUtils.ReadAI32Snapshot(binaryReader, InternalAnomalyAI32, 1);
        TONetUtils.ReadAI64Snapshot(binaryReader, InternalAnomalyAI64, 1);
    }

    // 弹幕在克脑幻象阶段开始前已存在时，不再触发真假脑命中判定。
    // 本地实体标记，保留原有不参与额外 AI 快照的行为；克隆时按值复制。
    public bool IgnoreBrainIllusions;

    #region 额外数据
    public bool ShouldRunAnomalyAI
    {
        get => InternalAnomalyAI32[0].bits[0];
        set
        {
            if (InternalAnomalyAI32[0].bits[0] != value)
            {
                InternalAnomalyAI32[0].bits[0] = value;
                InternalAIChanged32[0] = true;
            }
        }
    }

    public int OverrideType
    {
        get => InternalAnomalyAI32[1].i;
        set
        {
            if (InternalAnomalyAI32[1].i != value)
            {
                InternalAnomalyAI32[1].i = value;
                InternalAIChanged32[1] = true;
            }
        }
    }

    /// <summary>
    /// 视觉深度（世界像素）。0 为正常平面，正值向屏幕后方远离，负值向屏幕前方靠近。
    /// <br/>仅影响绘制，不改变位置、碰撞或伤害；持续保留，回到正常平面时应设为 0。
    /// <br/>随弹幕额外 AI 同步；由弹幕的网络控制端修改时按需设置 projectile.netUpdate。
    /// </summary>
    public float Z
    {
        get => InternalAnomalyAI32[2].f;
        set
        {
            if (!float.IsFinite(value))
                throw new ArgumentOutOfRangeException(nameof(value));
            if (InternalAnomalyAI32[2].f != value)
            {
                InternalAnomalyAI32[2].f = value;
                InternalAIChanged32[2] = true;
            }
        }
    }
    #endregion 额外数据
}

public sealed class AnomalyGlobalItem : GlobalItem
{
    public override bool InstancePerEntity => true;

#if DEBUG
    /// <summary>
    /// 调试用数据。
    /// <br/>不同实体可能会有不同的用途。
    /// </summary>
    public readonly Union64[] DebugData = new Union64[4];
#endif

    private const int dataSlot = 64;
    private const int dataSlot2 = 32;

    public Union32[] Data { get; private set; } = new Union32[dataSlot];
    public Union64[] Data2 { get; private set; } = new Union64[dataSlot2];

    public override GlobalItem Clone(Item from, Item to)
    {
        AnomalyGlobalItem clone = (AnomalyGlobalItem)base.Clone(from, to);

        clone.Data = (Union32[])Data.Clone();
        clone.Data2 = (Union64[])Data2.Clone();

        return clone;
    }
}

