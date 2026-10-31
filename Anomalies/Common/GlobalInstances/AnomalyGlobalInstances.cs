namespace Anomalies.Common;

public sealed partial class AnomalyPlayer : ModPlayer
{
#if DEBUG
    /// <summary>
    /// 调试用数据。
    /// <br/>不同实体可能会有不同的用途。
    /// </summary>
    public readonly Union64[] DebugData = new Union64[4];
#endif

    public override ModPlayer Clone(Player newEntity)
    {
        AnomalyPlayer clone = (AnomalyPlayer)base.Clone(newEntity);

        CloneExtraData(clone);
        return clone;
    }

    partial void CloneExtraData(AnomalyPlayer clone);

    public override void ResetEffects() => ResetExtraData();

    partial void ResetExtraData();

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) => SyncExtraData(toWho, fromWho, newPlayer);

    partial void SyncExtraData(int toWho, int fromWho, bool newPlayer);
}

public sealed partial class AnomalyGlobalNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

#if DEBUG
    /// <summary>
    /// 调试用数据。
    /// <br/>不同实体可能会有不同的用途。
    /// </summary>
    public readonly Union64[] DebugData = new Union64[4];
#endif

    private const int AISlot = 6;
    private const int AISlot2 = 2;
    private const int AISlot3 = 7;
    private const int AISlot4 = 2;

    public Union32[] AnomalyAI32 { get; private set; } = new Union32[AISlot];
    public Union64[] AnomalyAI64 { get; private set; } = new Union64[AISlot2];

    /// <summary>显式申请内容 AI 数据槽；只扩容，保留数据与尾部标记。不得持有旧数组的 ref 跨越此调用。</summary>
    public void EnsureAICapacity(int ai32Count, int ai64Count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(ai32Count);
        ArgumentOutOfRangeException.ThrowIfNegative(ai64Count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(ai32Count, 64);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(ai64Count, 32);
        AnomalyAI32 = AnomalyAICapacity.Grow(AnomalyAI32, ai32Count, 2);
        AnomalyAI64 = AnomalyAICapacity.Grow(AnomalyAI64, ai64Count, 1);
        if (_Network_ProjectileReferenceCache.Length < AnomalyAI64.Length - 1)
            Array.Resize(ref _Network_ProjectileReferenceCache, AnomalyAI64.Length - 1);
    }

    public ref BitArray32 AIChanged32 => ref AnomalyAI32[^2].Bits;
    public ref BitArray32 AIChanged32_2 => ref AnomalyAI32[^1].Bits;
    public ref BitArray64 AIChanged64 => ref AnomalyAI64[^1].Bits;

    public Projectile GetProjectileReference(int slot)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(slot, 32);
        EnsureAICapacity(0, slot + 1);
        ref ProjectileNetReference reference = ref _Network_ProjectileReferenceCache[slot];
        reference.SetPackedValue(AnomalyAI64[slot].L);
        return reference.Resolve();
    }

    public void SetProjectileReference(NPC npc, int slot, Projectile projectile)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(slot, 32);
        EnsureAICapacity(0, slot + 1);
        ref ProjectileNetReference reference = ref _Network_ProjectileReferenceCache[slot];
        reference.Set(projectile);
        long packed = reference.PackedValue;
        if (AnomalyAI64[slot].L == packed)
            return;
        AnomalyAI64[slot].L = packed;
        AIChanged64[slot] = true;
        if (TONetworkEnvironment.NotClient)
            npc.netUpdate = true;
    }

    private Union32[] InternalAnomalyAI32 { get; set; } = new Union32[AISlot3];
    private Union64[] InternalAnomalyAI64 { get; set; } = new Union64[AISlot4];

    private ref BitArray32 InternalAIChanged32 => ref InternalAnomalyAI32[^1].Bits;
    private ref BitArray64 InternalAIChanged64 => ref InternalAnomalyAI64[^1].Bits;

    public override void ResetEffects(NPC npc) => ResetExtraData();

    partial void ResetExtraData();

    public override GlobalNPC Clone(NPC from, NPC to)
    {
        AnomalyGlobalNPC clone = (AnomalyGlobalNPC)base.Clone(from, to);
        clone.AnomalyAI32 = (Union32[])AnomalyAI32.Clone();
        clone.AnomalyAI64 = (Union64[])AnomalyAI64.Clone();
        clone.InternalAnomalyAI32 = (Union32[])InternalAnomalyAI32.Clone();
        clone.InternalAnomalyAI64 = (Union64[])InternalAnomalyAI64.Clone();

        CloneExtraData(clone);
        return clone;
    }

    partial void CloneExtraData(AnomalyGlobalNPC clone);

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        TONetUtils.WriteAI32Snapshot(binaryWriter, AnomalyAI32, 2);
        TONetUtils.WriteAI64Snapshot(binaryWriter, AnomalyAI64, 1);
        TONetUtils.WriteAI32Snapshot(binaryWriter, InternalAnomalyAI32, 1);
        TONetUtils.WriteAI64Snapshot(binaryWriter, InternalAnomalyAI64, 1);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        AnomalyAI32 = AnomalyAICapacity.Read32(binaryReader, AnomalyAI32, 2, 64);
        AnomalyAI64 = AnomalyAICapacity.Read64(binaryReader, AnomalyAI64, 32);
        TONetUtils.ReadAI32Snapshot(binaryReader, InternalAnomalyAI32, 1);
        TONetUtils.ReadAI64Snapshot(binaryReader, InternalAnomalyAI64, 1);
    }
}

public sealed partial class AnomalyGlobalProjectile : GlobalProjectile
{
    public override bool InstancePerEntity => true;

#if DEBUG
    /// <summary>
    /// 调试用数据。
    /// <br/>不同实体可能会有不同的用途。
    /// </summary>
    public readonly Union64[] DebugData = new Union64[4];
#endif

    private const int AISlot = 5;
    private const int AISlot2 = 2;

    public Union32[] AnomalyAI32 { get; private set; } = new Union32[AISlot];
    public Union64[] AnomalyAI64 { get; private set; } = new Union64[AISlot2];

    /// <summary>显式申请内容 AI 数据槽；只扩容，保留数据与尾部标记。不得持有旧数组的 ref 跨越此调用。</summary>
    public void EnsureAICapacity(int ai32Count, int ai64Count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(ai32Count);
        ArgumentOutOfRangeException.ThrowIfNegative(ai64Count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(ai32Count, 32);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(ai64Count, 16);
        AnomalyAI32 = AnomalyAICapacity.Grow(AnomalyAI32, ai32Count, 1);
        AnomalyAI64 = AnomalyAICapacity.Grow(AnomalyAI64, ai64Count, 1);
    }

    public ref BitArray32 AIChanged32 => ref AnomalyAI32[^1].Bits;
    public ref BitArray64 AIChanged64 => ref AnomalyAI64[^1].Bits;

    private Union32[] InternalAnomalyAI32 { get; set; } = new Union32[4];
    private Union64[] InternalAnomalyAI64 { get; set; } = new Union64[AISlot2];

    private ref BitArray32 InternalAIChanged32 => ref InternalAnomalyAI32[^1].Bits;
    private ref BitArray64 InternalAIChanged64 => ref InternalAnomalyAI64[^1].Bits;

    public override GlobalProjectile Clone(Projectile from, Projectile to)
    {
        AnomalyGlobalProjectile clone = (AnomalyGlobalProjectile)base.Clone(from, to);

        clone.AnomalyAI32 = (Union32[])AnomalyAI32.Clone();
        clone.AnomalyAI64 = (Union64[])AnomalyAI64.Clone();
        clone.InternalAnomalyAI32 = (Union32[])InternalAnomalyAI32.Clone();
        clone.InternalAnomalyAI64 = (Union64[])InternalAnomalyAI64.Clone();

        CloneExtraData(clone);
        return clone;
    }

    partial void CloneExtraData(AnomalyGlobalProjectile clone);

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
        AnomalyAI32 = AnomalyAICapacity.Read32(binaryReader, AnomalyAI32, 1, 32);
        AnomalyAI64 = AnomalyAICapacity.Read64(binaryReader, AnomalyAI64, 16);
        TONetUtils.ReadAI32Snapshot(binaryReader, InternalAnomalyAI32, 1);
        TONetUtils.ReadAI64Snapshot(binaryReader, InternalAnomalyAI64, 1);
    }
}

public sealed partial class AnomalyGlobalItem : GlobalItem
{
    public override bool InstancePerEntity => true;

#if DEBUG
    /// <summary>
    /// 调试用数据。
    /// <br/>不同实体可能会有不同的用途。
    /// </summary>
    public readonly Union64[] DebugData = new Union64[4];
#endif

    private const int DataSlot = 64;
    private const int DataSlot2 = 32;

    public Union32[] Data { get; private set; } = new Union32[DataSlot];
    public Union64[] Data2 { get; private set; } = new Union64[DataSlot2];

    public override GlobalItem Clone(Item from, Item to)
    {
        AnomalyGlobalItem clone = (AnomalyGlobalItem)base.Clone(from, to);

        clone.Data = (Union32[])Data.Clone();
        clone.Data2 = (Union64[])Data2.Clone();

        return clone;
    }
}

