using Transoceanic.Game.Networking;
using Transoceanic.Game.Timing;
using Transoceanic.Graphics.Particles.BuiltIn;

namespace Transoceanic.GlobalInstances;

public sealed class TOGlobalNPC : GlobalNPC, ITOLoader
{
    public override bool InstancePerEntity => true;

    /// <summary>
    /// 标识符分配器。
    /// </summary>
    private static long _IdentifierAllocator;

    private long? _Identifier;

    /// <summary>
    /// NPC的标识符。
    /// <br/>不同步。
    /// </summary>
    public long Identifier => _Identifier ??= ++_IdentifierAllocator;

    /// <summary>
    /// NPC生成时 <see cref="TOGameTime.GameTimer"/> 的值。
    /// <br/>不同步。
    /// </summary>
    internal long SpawnTime = -1;

    public readonly List<AfterimageParticle> Afterimages = [];

    /// <summary>
    /// 帧计数器，用于自定义实现的 FindFrame 方法。
    /// <br/>不同步。
    /// </summary>
    public double FrameCounter;
    /// <inheritdoc cref="FrameCounter"/>
    public double FrameCounter2;
    /// <inheritdoc cref="FrameCounter"/>
    public int FrameCounter3;

    private const int AISlot = 33;
    private const int AISlot2 = 17;

    internal Union32[] OceanAI32 { get; private set; } = new Union32[AISlot];
    internal Union64[] OceanAI64 { get; private set; } = new Union64[AISlot2];

    internal ref BitArray32 AIChanged32 => ref OceanAI32[^1].Bits;
    internal ref BitArray64 AIChanged64 => ref OceanAI64[^1].Bits;

    public override GlobalNPC Clone(NPC from, NPC to)
    {
        TOGlobalNPC clone = (TOGlobalNPC)base.Clone(from, to);

        clone.SpawnTime = SpawnTime;
        clone.OceanAI32 = (Union32[])OceanAI32.Clone();
        clone.OceanAI64 = (Union64[])OceanAI64.Clone();

        return clone;
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        if (!TONetworkEnvironment.SyncEnabled)
            return;

        TONetUtils.WriteAI32Snapshot(binaryWriter, OceanAI32, 1);
        TONetUtils.WriteAI64Snapshot(binaryWriter, OceanAI64, 1);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        if (!TONetworkEnvironment.SyncEnabled)
            return;

        TONetUtils.ReadAI32Snapshot(binaryReader, OceanAI32, 1);
        TONetUtils.ReadAI64Snapshot(binaryReader, OceanAI64, 1);
    }

    void ITOLoader.Load() => _IdentifierAllocator = 0;

    void ITOLoader.Unload() => _IdentifierAllocator = 0;
}
