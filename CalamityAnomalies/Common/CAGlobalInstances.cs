// Developed by ColdsUx

using CalamityAnomalies.DataStructures;
using CalamityMod.Items.Potions.Alcohol;
using Transoceanic.Framework.Helpers.Utilities;

namespace CalamityAnomalies.Common;

public sealed class CAPlayer : ModPlayer
{
    //数据变量按字母顺序排列
    public int Coldheart_Phase;
    public int Coldheart_SubPhase;

    public bool Debuff_DimensionalRend;

    public PlayerDownedBossCalamity DownedBossCalamity = new();
    public PlayerDownedBossCalamity DownedBossAnomaly = new();

    public HysteresisBoolean YharimsGift;
    //public YharimsGift_CurrentBlessing YharimsGift_Blessing = YharimsGift_CurrentBlessing.None;
    //public readonly SmoothInt[] YharimsGift_Change = new SmoothInt[YharimsGift_Handler._totalBlessings];
    public Item YharimsGift_Last;

    public int ImmaculateWhite_Timer
    {
        get;
        set => field = Math.Max(0, value);
    }

    public override ModPlayer Clone(Player newEntity)
    {
        CAPlayer clone = (CAPlayer)base.Clone(newEntity);

        clone.Coldheart_Phase = Coldheart_Phase;
        clone.Coldheart_SubPhase = Coldheart_SubPhase;

        clone.Debuff_DimensionalRend = Debuff_DimensionalRend;
        clone.DownedBossCalamity = DownedBossCalamity;
        clone.DownedBossAnomaly = DownedBossAnomaly;

        clone.YharimsGift = YharimsGift;
        //clone.YharimsGift_Blessing = YharimsGift_Blessing;
        //Array.Copy(YharimsGift_Change, clone.YharimsGift_Change, YharimsGift_Change.Length);
        clone.YharimsGift_Last = YharimsGift_Last;

        clone.ImmaculateWhite_Timer = ImmaculateWhite_Timer;

        return clone;
    }

    public override void ResetEffects()
    {
        Debuff_DimensionalRend = false;
        ImmaculateWhite_Timer--;
    }
}

public sealed class CAGlobalNPC : GlobalNPC, IContentLoader
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
    private const int AISlot3 = 132;
    private const int AISlot4 = 33;

    public readonly Union32[] AnomalyAI32 = new Union32[AISlot];
    public readonly Union64[] AnomalyAI64 = new Union64[AISlot2];

    public ref BitArray32 AIChanged32 => ref AnomalyAI32[^1].bits;
    public ref BitArray64 AIChanged64 => ref AnomalyAI64[^1].bits;

    private readonly Union32[] InternalAnomalyAI32 = new Union32[AISlot3];
    private readonly Union64[] InternalAnomalyAI64 = new Union64[AISlot4];

    private ref BitArray32 InternalAIChanged32 => ref InternalAnomalyAI32[^4].bits;
    private ref BitArray32 InternalAIChanged32_2 => ref InternalAnomalyAI32[^3].bits;
    private ref BitArray32 InternalAIChanged32_3 => ref InternalAnomalyAI32[^2].bits;
    private ref BitArray32 InternalAIChanged32_4 => ref InternalAnomalyAI32[^1].bits;
    private ref BitArray64 InternalAIChanged64 => ref InternalAnomalyAI64[^1].bits;

    public override GlobalNPC Clone(NPC from, NPC to)
    {
        CAGlobalNPC clone = (CAGlobalNPC)base.Clone(from, to);

        Array.Copy(AnomalyAI32, clone.AnomalyAI32, AISlot);
        Array.Copy(AnomalyAI64, clone.AnomalyAI64, AISlot2);
        Array.Copy(InternalAnomalyAI32, clone.InternalAnomalyAI32, AISlot3);
        Array.Copy(InternalAnomalyAI32, clone.InternalAnomalyAI32, AISlot4);

        return clone;
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        TONetUtils.WriteChangedAI32(binaryWriter, AnomalyAI32, 1);
        TONetUtils.WriteChangedAI64(binaryWriter, AnomalyAI64, 1);
        TONetUtils.WriteChangedAI32(binaryWriter, InternalAnomalyAI32, 4);
        TONetUtils.WriteChangedAI64(binaryWriter, InternalAnomalyAI64, 1);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        TONetUtils.ReadChangedAI32(binaryReader, AnomalyAI32);
        TONetUtils.ReadChangedAI64(binaryReader, AnomalyAI64);
        TONetUtils.ReadChangedAI32(binaryReader, InternalAnomalyAI32);
        TONetUtils.ReadChangedAI64(binaryReader, InternalAnomalyAI64);
    }

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

    public bool Debuff_DimensionalRend
    {
        get => InternalAnomalyAI32[0].bits[1];
        set
        {
            if (InternalAnomalyAI32[0].bits[1] != value)
            {
                InternalAnomalyAI32[0].bits[1] = value;
                InternalAIChanged32[0] = true;
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

    public int AnomalyKilltime;

    public int AnomalyAITimer;

    public bool IsRunningAnomalyAI => AnomalyAITimer > 0;

    public int AnomalyUltraAITimer;
    public int AnomalyUltraBarTimer;

    public List<HPThresholdIndicator> HPThresholdIndicators = [];

    public DynamicDamageReductionHandler DynamicDRHandler;
    #endregion 额外数据
}

public sealed class CAGlobalProjectile : GlobalProjectile
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

    public readonly Union32[] AnomalyAI32 = new Union32[AISlot];
    public readonly Union64[] AnomalyAI64 = new Union64[AISlot2];

    public ref BitArray32 AIChanged32 => ref AnomalyAI32[^1].bits;
    public ref BitArray64 AIChanged64 => ref AnomalyAI64[^1].bits;

    private readonly Union32[] InternalAnomalyAI32 = new Union32[AISlot];
    private readonly Union64[] InternalAnomalyAI64 = new Union64[AISlot2];

    private ref BitArray32 InternalAIChanged32 => ref InternalAnomalyAI32[^1].bits;
    private ref BitArray64 InternalAIChanged64 => ref InternalAnomalyAI64[^1].bits;

    public override GlobalProjectile Clone(Projectile from, Projectile to)
    {
        CAGlobalProjectile clone = (CAGlobalProjectile)base.Clone(from, to);

        Array.Copy(AnomalyAI32, clone.AnomalyAI32, AISlot);
        Array.Copy(AnomalyAI64, clone.AnomalyAI64, AISlot2);
        Array.Copy(InternalAnomalyAI32, clone.InternalAnomalyAI32, AISlot);
        Array.Copy(InternalAnomalyAI32, clone.InternalAnomalyAI32, AISlot2);

        return clone;
    }

    public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        TONetUtils.WriteChangedAI32(binaryWriter, AnomalyAI32, 1);
        TONetUtils.WriteChangedAI64(binaryWriter, AnomalyAI64, 1);
        TONetUtils.WriteChangedAI32(binaryWriter, InternalAnomalyAI32, 1);
        TONetUtils.WriteChangedAI64(binaryWriter, InternalAnomalyAI64, 1);
    }

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
    {
        TONetUtils.ReadChangedAI32(binaryReader, AnomalyAI32);
        TONetUtils.ReadChangedAI64(binaryReader, AnomalyAI64);
        TONetUtils.ReadChangedAI32(binaryReader, InternalAnomalyAI32);
        TONetUtils.ReadChangedAI64(binaryReader, InternalAnomalyAI64);
    }

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
    #endregion 额外数据
}

public sealed class CAGlobalItem : GlobalItem
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

    public readonly Union32[] Data = new Union32[dataSlot];
    public readonly Union64[] Data2 = new Union64[dataSlot2];

    public override GlobalItem Clone(Item from, Item to)
    {
        CAGlobalItem clone = (CAGlobalItem)base.Clone(from, to);

        Array.Copy(Data, clone.Data, dataSlot);
        Array.Copy(Data2, clone.Data2, dataSlot2);

        return clone;
    }
}
