using Transoceanic.GlobalInstances;

namespace Transoceanic.Behaviors;

/// <summary>
/// 按弹幕类型匹配的行为基类，回调前绑定当前弹幕。
/// </summary>
public abstract partial class SingleProjectileBehavior : SingleEntityBehavior<Projectile>
{
    /// <summary>
    /// 获取当前绑定实体的 Projectile 引用；行为共享此绑定，不应跨回调缓存。
    /// </summary>
    public Projectile Projectile { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Entity; }

    /// <summary>
    /// 获取当前绑定实体的 OceanProjectile 引用；行为共享此绑定，不应跨回调缓存。
    /// </summary>
    public TOGlobalProjectile OceanProjectile { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Entity.Ocean; }

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
        get => (Union32)Projectile.ai[0];
        set => Projectile.ai[0] = value.F;
    }

    /// <summary>
    /// 以 Union32 位型读写实体 ai[2]，修改副本后须写回。写入不会自动请求网络同步，调用方按实体权威规则设置 netUpdate。
    /// </summary>
    public Union32 AI_Union_2
    {
        get => (Union32)Projectile.ai[0];
        set => Projectile.ai[0] = value.F;
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

    /// <inheritdoc cref="GlobalProjectile.CanCutTiles"/>
    public virtual bool? CanCutTiles() => null;

    /// <inheritdoc cref="GlobalProjectile.CanDamage"/>
    public virtual bool? CanDamage() => null;
}
