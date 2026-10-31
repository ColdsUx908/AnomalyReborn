using Transoceanic.GlobalInstances;
namespace Transoceanic.Core.ModIntegration;

/// <summary>
/// 标记采用 Transoceanic 实体集成约定的模组内容类型。
/// </summary>
public interface ITOModNPC
{
}

/// <summary>
/// 集成 Transoceanic 数据访问和常用状态映射的 ModNPC 基类。
/// </summary>
public abstract class TOModNPC : ModNPC, ITOModNPC
{
    /// <summary>
    /// 获取此模组实体对应的 OceanNPC 数据容器。
    /// </summary>
    public TOGlobalNPC OceanNPC { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => NPC.Ocean; }

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
}

/// <summary>
/// 标记采用 Transoceanic 实体集成约定的模组内容类型。
/// </summary>
public interface ITOModProjectile
{
}

/// <summary>
/// 集成 Transoceanic 数据访问和常用状态映射的 ModProjectile 基类。
/// </summary>
public abstract class TOModProjectile : ModProjectile, ITOModProjectile
{
    /// <summary>
    /// 获取此模组实体对应的 OceanProjectile 数据容器。
    /// </summary>
    public TOGlobalProjectile OceanProjectile { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Projectile.Ocean; }

    /// <summary>
    /// 弹幕所属玩家引用，由 Projectile.owner 对应的玩家槽位取得。
    /// </summary>
    public Player Owner => Projectile.Owner;

    /// <summary>
    /// 读写实体的 Timer1 计时槽；不会自动递增，槽位和同步行为与实体同名扩展属性一致。
    /// </summary>
    public int Timer1
    {
        get => Projectile.Timer1;
        set => Projectile.Timer1 = value;
    }
    /// <summary>
    /// 读写实体的 Timer2 计时槽；不会自动递增，槽位和同步行为与实体同名扩展属性一致。
    /// </summary>
    public int Timer2
    {
        get => Projectile.Timer2;
        set => Projectile.Timer2 = value;
    }
    /// <summary>
    /// 读写实体的 Timer3 计时槽；不会自动递增，槽位和同步行为与实体同名扩展属性一致。
    /// </summary>
    public int Timer3
    {
        get => Projectile.Timer3;
        set => Projectile.Timer3 = value;
    }
    /// <summary>
    /// 读写实体的 Timer4 计时槽；不会自动递增，槽位和同步行为与实体同名扩展属性一致。
    /// </summary>
    public float Timer4
    {
        get => Projectile.Timer4;
        set => Projectile.Timer4 = value;
    }
    /// <summary>
    /// 读写实体的 Timer5 计时槽；不会自动递增，槽位和同步行为与实体同名扩展属性一致。
    /// </summary>
    public float Timer5
    {
        get => Projectile.Timer5;
        set => Projectile.Timer5 = value;
    }

    /// <summary>
    /// 以 Union32 位型读写实体 ai[0]，修改副本后须写回。写入不会自动请求网络同步，调用方按实体权威规则设置 netUpdate。
    /// </summary>
    public Union32 AI_Union_0
    {
        get => (Union32)Projectile.ai[0];
        set => Projectile.ai[0] = value.F;
    }
    /// <summary>
    /// 以 Union32 位型读写实体 ai[1]，修改副本后须写回。写入不会自动请求网络同步，调用方按实体权威规则设置 netUpdate。
    /// </summary>
    public Union32 AI_Union_1
    {
        get => (Union32)Projectile.ai[1];
        set => Projectile.ai[1] = value.F;
    }
    /// <summary>
    /// 以 Union32 位型读写实体 ai[2]，修改副本后须写回。写入不会自动请求网络同步，调用方按实体权威规则设置 netUpdate。
    /// </summary>
    public Union32 AI_Union_2
    {
        get => (Union32)Projectile.ai[2];
        set => Projectile.ai[2] = value.F;
    }
    /// <summary>
    /// 以 Union32 位型读写实体 localAI[0]，修改副本后须写回。该槽仅在本地使用，不自动同步。
    /// </summary>
    public Union32 LocalAI_Union_0
    {
        get => (Union32)Projectile.localAI[0];
        set => Projectile.localAI[0] = value.F;
    }
    /// <summary>
    /// 以 Union32 位型读写实体 localAI[1]，修改副本后须写回。该槽仅在本地使用，不自动同步。
    /// </summary>
    public Union32 LocalAI_Union_1
    {
        get => (Union32)Projectile.localAI[1];
        set => Projectile.localAI[1] = value.F;
    }
    /// <summary>
    /// 以 Union32 位型读写实体 localAI[2]，修改副本后须写回。该槽仅在本地使用，不自动同步。
    /// </summary>
    public Union32 LocalAI_Union_2
    {
        get => (Union32)Projectile.localAI[2];
        set => Projectile.localAI[2] = value.F;
    }

    /// <summary>
    /// 创建归属于当前实体 AI 的生成来源。
    /// </summary>
    public IEntitySource SourceAI => Projectile.GetSource_FromAI();
}

/// <summary>
/// 标记采用 Transoceanic 实体集成约定的模组内容类型。
/// </summary>
public interface ITOModItem
{
}

/// <summary>
/// 集成 Transoceanic 数据访问和常用状态映射的 ModItem 基类。
/// </summary>
public abstract class TOModItem : ModItem, ITOModItem
{
    /// <summary>
    /// 获取此模组实体对应的 OceanItem 数据容器。
    /// </summary>
    public TOGlobalItem OceanItem { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Item.Ocean; }
}

