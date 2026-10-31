using Transoceanic.Game.Networking;
using Transoceanic.Graphics.Particles.BuiltIn;

namespace Transoceanic.GlobalInstances;

/// <summary>
/// 保存每个弹幕的框架 AI 与客户端残影，并处理可选 ExtraAI 同步。
/// </summary>
public sealed class TOGlobalProjectile : GlobalProjectile
{
    /// <inheritdoc/>
    /// <remarks>
    /// 返回 true，为每个实体分配独立的框架实例数据。
    /// </remarks>
    public override bool InstancePerEntity => true;

    /// <summary>
    /// 实体拥有的客户端残影列表；克隆时清空，不参与网络同步。
    /// </summary>
    public List<AfterimageParticle> Afterimages => ActiveAfterimages ??= [];

    /// <summary>
    /// 已启用的客户端残影；查询不分配列表。
    /// </summary>
    internal List<AfterimageParticle> ActiveAfterimages { get; private set; }

    /// <summary>
    /// 清空并关闭本地残影，释放列表引用。
    /// </summary>
    public void DisableAfterimages()
    {
        ActiveAfterimages?.Clear();
        ActiveAfterimages = null;
    }

    private const int AISlot = 8;
    internal const int Timer1Slot = 2;
    internal const int Timer2Slot = 3;
    internal const int Timer3Slot = 4;
    internal const int Timer4Slot = 5;
    internal const int Timer5Slot = 6;


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
    /// 独立克隆 AI32 数组并清空残影列表，避免实体间共享可变容器。
    /// </remarks>
    public override GlobalProjectile Clone(Projectile from, Projectile to)
    {
        TOGlobalProjectile clone = (TOGlobalProjectile)base.Clone(from, to);
        clone.ActiveAfterimages = null;

        clone.OceanAI32 = (Union32[])OceanAI32.Clone();

        return clone;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 同步启用时依次写出 OceanAI32 完整稀疏快照；不清除变化标记，不发送残影等绘制数据。
    /// </remarks>
    public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        if (!TONetworkEnvironment.SyncEnabled)
            return;

        TONetUtils.WriteAI32Snapshot(binaryWriter, OceanAI32, 1);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 同步启用时依次读取 AI32 快照；每组成功读取后覆盖数组，未发送的数据槽及尾部标记归零。
    /// </remarks>
    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
    {
        if (!TONetworkEnvironment.SyncEnabled)
            return;

        TONetUtils.ReadAI32Snapshot(binaryReader, OceanAI32, 1);
    }
}
