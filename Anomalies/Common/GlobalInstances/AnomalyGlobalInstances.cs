using Transoceanic.Framework.Helpers.Utilities;

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

public sealed partial class AnomalyGlobalNPC : GlobalNPC, IContentLoader
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

    public Union32[] AnomalyAI32 { get; private set; } = new Union32[AISlot];
    public Union64[] AnomalyAI64 { get; private set; } = new Union64[AISlot2];

    public ref BitArray32 AIChanged32 => ref AnomalyAI32[^2].bits;
    public ref BitArray32 AIChanged32_2 => ref AnomalyAI32[^1].bits;
    public ref BitArray64 AIChanged64 => ref AnomalyAI64[^1].bits;

    public Projectile GetProjectileReference(int slot)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(slot, _Network_ProjectileReferenceCache.Length);
        ref ProjectileNetReference reference = ref _Network_ProjectileReferenceCache[slot];
        reference.SetPackedValue(AnomalyAI64[slot].l);
        return reference.Resolve();
    }

    public void SetProjectileReference(NPC npc, int slot, Projectile projectile)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(slot, _Network_ProjectileReferenceCache.Length);
        ref ProjectileNetReference reference = ref _Network_ProjectileReferenceCache[slot];
        reference.Set(projectile);
        long packed = reference.PackedValue;
        if (AnomalyAI64[slot].l == packed)
            return;
        AnomalyAI64[slot].l = packed;
        AIChanged64[slot] = true;
        if (TOSharedData.NotClient)
            npc.netUpdate = true;
    }

    private Union32[] InternalAnomalyAI32 { get; set; } = new Union32[AISlot3];
    private Union64[] InternalAnomalyAI64 { get; set; } = new Union64[AISlot4];

    private ref BitArray32 InternalAIChanged32 => ref InternalAnomalyAI32[^1].bits;
    private ref BitArray64 InternalAIChanged64 => ref InternalAnomalyAI64[^1].bits;

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
        TONetUtils.ReadAI32Snapshot(binaryReader, AnomalyAI32, 2);
        TONetUtils.ReadAI64Snapshot(binaryReader, AnomalyAI64, 1);
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
        TONetUtils.ReadAI32Snapshot(binaryReader, AnomalyAI32, 1);
        TONetUtils.ReadAI64Snapshot(binaryReader, AnomalyAI64, 1);
        TONetUtils.ReadAI32Snapshot(binaryReader, InternalAnomalyAI32, 1);
        TONetUtils.ReadAI64Snapshot(binaryReader, InternalAnomalyAI64, 1);
    }
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

