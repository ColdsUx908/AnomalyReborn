// Developed by ColdsUx

using Anomalies.DataStructures;
using Transoceanic.Framework.Helpers.Utilities;

namespace Anomalies.Common;

public sealed class AnomalyPlayer : ModPlayer
{
    //数据变量按字母顺序排列
    public int Coldheart_Phase;
    public int Coldheart_SubPhase;

    public bool Debuff_DimensionalRend;

    public PlayerDownedBoss DownedBoss = new();

    public int ImmaculateWhite_Timer
    {
        get;
        set => field = Math.Max(0, value);
    }

    public bool Minion_VacuousBlack;

    public override ModPlayer Clone(Player newEntity)
    {
        AnomalyPlayer clone = (AnomalyPlayer)base.Clone(newEntity);

        clone.Coldheart_Phase = Coldheart_Phase;
        clone.Coldheart_SubPhase = Coldheart_SubPhase;

        clone.Debuff_DimensionalRend = Debuff_DimensionalRend;
        clone.DownedBoss = DownedBoss;

        clone.ImmaculateWhite_Timer = ImmaculateWhite_Timer;

        return clone;
    }

    public override void ResetEffects()
    {
        Debuff_DimensionalRend = false;
        ImmaculateWhite_Timer--;
        Minion_VacuousBlack = false;
    }
}

public sealed class AnomalyGlobalNPC : GlobalNPC, IContentLoader
{
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

    public readonly Union32[] AnomalyAI32 = new Union32[AISlot];
    public readonly Union64[] AnomalyAI64 = new Union64[AISlot2];

    public ref BitArray32 AIChanged32 => ref AnomalyAI32[^2].bits;
    public ref BitArray32 AIChanged32_2 => ref AnomalyAI32[^1].bits;
    public ref BitArray64 AIChanged64 => ref AnomalyAI64[^1].bits;

    private readonly Union32[] InternalAnomalyAI32 = new Union32[AISlot3];
    private readonly Union64[] InternalAnomalyAI64 = new Union64[AISlot4];

    private ref BitArray32 InternalAIChanged32 => ref InternalAnomalyAI32[^1].bits;
    private ref BitArray64 InternalAIChanged64 => ref InternalAnomalyAI64[^1].bits;

    public override GlobalNPC Clone(NPC from, NPC to)
    {
        AnomalyGlobalNPC clone = (AnomalyGlobalNPC)base.Clone(from, to);

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

    public bool CanHaveBossHealthBar
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

    public bool CurrentlyIncreasingDefenseOrDR
    {
        get => InternalAnomalyAI32[0].bits[2];
        set
        {
            if (InternalAnomalyAI32[0].bits[2] != value)
            {
                InternalAnomalyAI32[0].bits[2] = value;
                InternalAIChanged32[0] = true;
            }
        }
    }

    public bool CurrentlyEnraged
    {
        get => InternalAnomalyAI32[0].bits[3];
        set
        {
            if (InternalAnomalyAI32[0].bits[3] != value)
            {
                InternalAnomalyAI32[0].bits[3] = value;
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
                InternalAIChanged32[0] = true;
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

    public int AnomalyKilltime;

    public int AnomalyAITimer;

    public bool IsRunningAnomalyAI => AnomalyAITimer > 0;

    public int AnomalyUltraAITimer;
    public int AnomalyUltraBarTimer;

    public List<HPThresholdIndicator> HPThresholdIndicators = [];

    public IDynamicDRHandler DynamicDRHandler;
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
        AnomalyGlobalProjectile clone = (AnomalyGlobalProjectile)base.Clone(from, to);

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

    public readonly Union32[] Data = new Union32[dataSlot];
    public readonly Union64[] Data2 = new Union64[dataSlot2];

    public override GlobalItem Clone(Item from, Item to)
    {
        AnomalyGlobalItem clone = (AnomalyGlobalItem)base.Clone(from, to);

        Array.Copy(Data, clone.Data, dataSlot);
        Array.Copy(Data2, clone.Data2, dataSlot2);

        return clone;
    }
}
