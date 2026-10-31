using Transoceanic.Game.Networking;
using Transoceanic.Game.Timing;
using Transoceanic.Graphics.Particles.BuiltIn;

namespace Transoceanic.GlobalInstances;

/// <summary>
/// 保存每个 NPC 的框架 AI、生成标识与客户端残影，并处理可选 ExtraAI 同步。
/// </summary>
public sealed class TOGlobalNPC : GlobalNPC
{
    /// <inheritdoc/>
    /// <remarks>
    /// 返回 true，为每个实体分配独立的框架实例数据。
    /// </remarks>
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

    /// <summary>
    /// 实体拥有的客户端残影列表；克隆时清空，不参与网络同步。
    /// </summary>
    public List<AfterimageParticle> Afterimages => ActiveAfterimages ??= [];

    /// <summary>
    /// 已启用的本地残影；查询不创建容器。
    /// </summary>
    internal List<AfterimageParticle> ActiveAfterimages { get; private set; }

    /// <summary>
    /// 清空并关闭本地残影。
    /// </summary>
    public void DisableAfterimages()
    {
        ActiveAfterimages?.Clear();
        ActiveAfterimages = null;
    }

    /// <summary>
    /// 帧计数器，用于自定义实现的 FindFrame 方法。
    /// <br/>不同步。
    /// </summary>
    public double FrameCounter;
    /// <inheritdoc cref="FrameCounter"/>
    public double FrameCounter2;
    /// <inheritdoc cref="FrameCounter"/>
    public int FrameCounter3;

    private const int AISlot = 13;
    internal const int Timer1Slot = 4;
    internal const int Timer2Slot = 5;
    internal const int Timer3Slot = 6;
    internal const int Timer4Slot = 7;
    internal const int Timer5Slot = 8;
    internal const int Timer6Slot = 9;
    internal const int Timer7Slot = 10;
    internal const int Timer8Slot = 11;


    /// <summary>
    /// 32 位 AI 数据数组，末尾槽存放变化标记；通过实体扩展访问数据并维护标记。
    /// </summary>
    internal Union32[] OceanAI32 { get; private set; } = new Union32[AISlot];

    /// <summary>
    /// 32 位 AI 数组尾部的变化位标记引用，每一位对应一个数据槽。
    /// </summary>
    internal ref BitArray32 AIChanged32 => ref OceanAI32[^1].Bits;

    /// <inheritdoc/>
    /// <remarks>
    /// 独立克隆 AI32 数组，清空残影列表并重置标识，保留生成时间。
    /// </remarks>
    public override GlobalNPC Clone(NPC from, NPC to)
    {
        TOGlobalNPC clone = (TOGlobalNPC)base.Clone(from, to);

        clone.SpawnTime = SpawnTime;
        clone._Identifier = null;
        clone.ActiveAfterimages = null;
        clone.OceanAI32 = (Union32[])OceanAI32.Clone();

        return clone;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 同步启用时依次写出 OceanAI32 完整稀疏快照；不清除变化标记，不发送残影等绘制数据。
    /// </remarks>
    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        if (!TONetworkEnvironment.SyncEnabled)
            return;

        TONetUtils.WriteAI32Snapshot(binaryWriter, OceanAI32, 1);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 同步启用时依次读取 AI32 快照；每组成功读取后覆盖数组，未发送的数据槽及尾部标记归零。
    /// </remarks>
    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        if (!TONetworkEnvironment.SyncEnabled)
            return;

        TONetUtils.ReadAI32Snapshot(binaryReader, OceanAI32, 1);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 在 tML 内容加载时重置标识符分配器，并登记本轮清理。
    /// </remarks>
    public override void Load()
    {
        TOCleanup.Register(() => _IdentifierAllocator = 0);
        _IdentifierAllocator = 0;
    }
}
