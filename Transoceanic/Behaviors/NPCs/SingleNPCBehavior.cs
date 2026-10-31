using Transoceanic.GlobalInstances;

namespace Transoceanic.Behaviors;

/// <summary>
/// 按 NPC 类型匹配的行为基类，回调前绑定当前 NPC。
/// </summary>
public abstract partial class SingleNPCBehavior : SingleEntityBehavior<NPC>
{
    /// <summary>
    /// 获取当前绑定实体的 NPC 引用；行为共享此绑定，不应跨回调缓存。
    /// </summary>
    public NPC NPC { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Entity; }

    /// <summary>
    /// 获取当前绑定实体的 OceanNPC 引用；行为共享此绑定，不应跨回调缓存。
    /// </summary>
    public TOGlobalNPC OceanNPC { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Entity.Ocean; }

    /// <summary>
    /// NPC 当前目标玩家引用。
    /// </summary>
    public Player Target => NPC.PlayerTarget;

    /// <summary>
    /// 读写实体的 Timer1 计时槽；不会自动递增，槽位和同步行为与实体同名扩展属性一致。
    /// </summary>
    public int Timer1
    {
        get => NPC.Timer1;
        set => NPC.Timer1 = value;
    }

    /// <summary>
    /// 读写实体的 Timer2 计时槽；不会自动递增，槽位和同步行为与实体同名扩展属性一致。
    /// </summary>
    public int Timer2
    {
        get => NPC.Timer2;
        set => NPC.Timer2 = value;
    }

    /// <summary>
    /// 读写实体的 Timer3 计时槽；不会自动递增，槽位和同步行为与实体同名扩展属性一致。
    /// </summary>
    public int Timer3
    {
        get => NPC.Timer3;
        set => NPC.Timer3 = value;
    }

    /// <summary>
    /// 读写实体的 Timer4 计时槽；不会自动递增，槽位和同步行为与实体同名扩展属性一致。
    /// </summary>
    public int Timer4
    {
        get => NPC.Timer4;
        set => NPC.Timer4 = value;
    }

    /// <summary>
    /// 读写实体的 Timer5 计时槽；不会自动递增，槽位和同步行为与实体同名扩展属性一致。
    /// </summary>
    public int Timer5
    {
        get => NPC.Timer5;
        set => NPC.Timer5 = value;
    }

    /// <summary>
    /// 读写实体的 Timer6 计时槽；不会自动递增，槽位和同步行为与实体同名扩展属性一致。
    /// </summary>
    public float Timer6
    {
        get => NPC.Timer6;
        set => NPC.Timer6 = value;
    }

    /// <summary>
    /// 读写实体的 Timer7 计时槽；不会自动递增，槽位和同步行为与实体同名扩展属性一致。
    /// </summary>
    public float Timer7
    {
        get => NPC.Timer7;
        set => NPC.Timer7 = value;
    }

    /// <summary>
    /// 读写实体的 Timer8 计时槽；不会自动递增，槽位和同步行为与实体同名扩展属性一致。
    /// </summary>
    public float Timer8
    {
        get => NPC.Timer8;
        set => NPC.Timer8 = value;
    }

    /// <summary>
    /// 以 Union32 位型读写实体 ai[0]，修改副本后须写回。写入不会自动请求网络同步，调用方按实体权威规则设置 netUpdate。
    /// </summary>
    public Union32 AI_Union_0
    {
        get => (Union32)NPC.ai[0];
        set => NPC.ai[0] = value.F;
    }

    /// <summary>
    /// 以 Union32 位型读写实体 ai[1]，修改副本后须写回。写入不会自动请求网络同步，调用方按实体权威规则设置 netUpdate。
    /// </summary>
    public Union32 AI_Union_1
    {
        get => (Union32)NPC.ai[1];
        set => NPC.ai[1] = value.F;
    }

    /// <summary>
    /// 以 Union32 位型读写实体 ai[2]，修改副本后须写回。写入不会自动请求网络同步，调用方按实体权威规则设置 netUpdate。
    /// </summary>
    public Union32 AI_Union_2
    {
        get => (Union32)NPC.ai[2];
        set => NPC.ai[2] = value.F;
    }

    /// <summary>
    /// 以 Union32 位型读写实体 ai[3]，修改副本后须写回。写入不会自动请求网络同步，调用方按实体权威规则设置 netUpdate。
    /// </summary>
    public Union32 AI_Union_3
    {
        get => (Union32)NPC.ai[3];
        set => NPC.ai[3] = value.F;
    }

    /// <summary>
    /// 以 Union32 位型读写实体 localAI[0]，修改副本后须写回。该槽仅在本地使用，不自动同步。
    /// </summary>
    public Union32 LocalAI_Union_0
    {
        get => (Union32)NPC.localAI[0];
        set => NPC.localAI[0] = value.F;
    }

    /// <summary>
    /// 以 Union32 位型读写实体 localAI[1]，修改副本后须写回。该槽仅在本地使用，不自动同步。
    /// </summary>
    public Union32 LocalAI_Union_1
    {
        get => (Union32)NPC.localAI[1];
        set => NPC.localAI[1] = value.F;
    }

    /// <summary>
    /// 以 Union32 位型读写实体 localAI[2]，修改副本后须写回。该槽仅在本地使用，不自动同步。
    /// </summary>
    public Union32 LocalAI_Union_2
    {
        get => (Union32)NPC.localAI[2];
        set => NPC.localAI[2] = value.F;
    }

    /// <summary>
    /// 以 Union32 位型读写实体 localAI[3]，修改副本后须写回。该槽仅在本地使用，不自动同步。
    /// </summary>
    public Union32 LocalAI_Union_3
    {
        get => (Union32)NPC.localAI[3];
        set => NPC.localAI[3] = value.F;
    }

    /// <summary>
    /// 创建归属于当前实体 AI 的生成来源。
    /// </summary>
    public IEntitySource SourceAI => NPC.GetSource_FromAI();

    /// <inheritdoc cref="GlobalNPC.CanFallThroughPlatforms"/>
    public virtual bool? CanFallThroughPlatforms() => null;

    /// <inheritdoc cref="GlobalNPC.CanBeCaughtBy"/>
    public virtual bool? CanBeCaughtBy(Item item, Player player) => null;

    /// <inheritdoc cref="GlobalNPC.CanChat"/>
    public virtual bool? CanChat() => null;

    /// <inheritdoc cref="GlobalNPC.CanGoToStatue"/>
    public virtual bool? CanGoToStatue(bool toKingStatue) => null;
}
