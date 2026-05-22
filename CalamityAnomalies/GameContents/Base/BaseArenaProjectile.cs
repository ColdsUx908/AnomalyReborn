// Developed by ColdsUx

namespace CalamityAnomalies.GameContents.Base;

/// <summary>
/// 环形竞技场弹幕的抽象基类，用于将玩家限制在以弹幕中心为圆心的环形区域内。
/// 当玩家试图越过边界时，会以递增的速度将其拉回，并在距离过远时剥夺控制权。
/// 子类需提供具体的半径 (<see cref="Radius"/>) 并可通过重写虚成员定制行为。
/// </summary>
public abstract class BaseArenaProjectile : CAModProjectile
{
    /// <summary>
    /// 竞技场环形边界的中心半径（从弹幕中心到环心中线的距离）。
    /// 子类必须实现此属性的读写访问器，可在运行时动态改变以调整边界大小。
    /// </summary>
    public abstract float Radius { get; set; }

    /// <summary>
    /// 环形边界的厚度（朝向圆心和背离圆心各扩展一半）。
    /// 默认值为 1，表示边界宽度为 1 像素。可用于调整伤害判定和约束的宽容度。
    /// </summary>
    public virtual float Thickness => 1f;

    /// <summary>
    /// 环形边界的内侧半径（圆心到环内侧边缘的距离）。
    /// 计算值为 <c>Radius - Thickness / 2f</c>。
    /// </summary>
    public float InnerRadius => Radius - Thickness / 2f;

    /// <summary>
    /// 环形边界的外侧半径（圆心到环外侧边缘的距离）。
    /// 计算值为 <c>Radius + Thickness / 2f</c>。
    /// </summary>
    public float OuterRadius => Radius + Thickness / 2f;

    /// <summary>
    /// 竞技场开始限制玩家操作的距离阈值。
    /// </summary>
    public float DistanceThreshold => Radius * 2f + 800f;

    /// <summary>
    /// 竞技场不再对玩家产生影响的距离阈值。
    /// </summary>
    public float DistanceThreshold2 => Radius * 4f + 4800f;

    /// <summary>
    /// 表示当前竞技场环形碰撞区域的几何结构。
    /// 使用 <see cref="Annulus"/> 定义以弹幕中心为圆心、内半径为 <see cref="InnerRadius"/>、外半径为 <see cref="OuterRadius"/> 的环形。
    /// 用于碰撞检测与伤害判定。
    /// </summary>
    public Annulus ArenaRing => new(Projectile.Center, InnerRadius, OuterRadius);

    /// <summary>
    /// 记录玩家持续超出环形边界的累计时间（帧数）。
    /// 每帧玩家越界时增加 1，进入范围内或离开干预范围时重置为 0。
    /// 用于计算逐步递增的拉回速度。
    /// </summary>
    public int OutofArenaTimer;

    /// <summary>
    /// 获取此竞技场当前锁定的目标玩家。
    /// 若无锁定玩家，应返回 <see langword="null"/>。
    /// </summary>
    public abstract Player Target { get; }

    /// <summary>
    /// 拉回玩家的基础速度（像素/帧）。
    /// 实际拉回速度会在此基础上根据 <see cref="OutofArenaTimer"/> 逐步增加，上限为 60。
    /// 默认值为 25。
    /// </summary>
    public virtual float BasePushSpeed => 25f;

    /// <summary>
    /// 是否启用移动限制。
    /// 默认值为 <see langword="true"/>。
    /// </summary>
    public virtual bool EnableMovementRestriction => true;

    /// <summary>
    /// 当玩家被拉回时产生的粒子效果类型 ID。
    /// 默认值为 -1，表示不生成粒子。子类可重写返回有效的 Dust ID 以提供视觉反馈。
    /// </summary>
    public virtual int DustType => -1;

    /// <summary>
    /// 竞技场弹幕对玩家移动的限制逻辑。
    /// 包含玩家越界检测、动态速度递增、位置修正、控制剥夺以及粒子生成。
    /// </summary>
    protected void MovementRestriction()
    {
        Player target = Target;
        if (target is not null && target.Alive)
        {
            float distance = target.Distance(Projectile.Center);

            if (distance > Radius && distance < DistanceThreshold2)
            {
                OutofArenaTimer++;
                int dustAmount = 10;

                float pushSpeed = Math.Min(BasePushSpeed + OutofArenaTimer * 0.05f, 60f);
                Vector2 movement = Projectile.Center - target.Center;
                float difference = movement.Length() - Radius;
                movement.Modulus = Math.Min(difference, pushSpeed);

                target.position += movement;

                if (distance > DistanceThreshold)
                {
                    target.Incapacitate();
                    dustAmount += 15;
                }

                int dustType = DustType;
                if (dustType != -1)
                {
                    for (int i = 0; i < dustAmount; i++)
                    {
                        Dust.NewDustAction(target.Center, target.width, target.height, dustType, default, d =>
                        {
                            d.noGravity = true;
                            d.velocity *= 5f;
                        });
                    }
                }
            }
            else //玩家处于安全范围内或超出干预距离，重置计时器
                OutofArenaTimer = 0;
        }
        else //目标玩家无效时也重置计时器
            OutofArenaTimer = 0;
    }

    /// <summary>
    /// 判断此弹幕是否能伤害指定玩家。
    /// 仅当玩家为当前锁定的 <see cref="Target"/> 时返回 <see langword="true"/>，否则为 <see langword="false"/>。
    /// </summary>
    /// <param name="target">待检测的玩家。</param>
    /// <returns>如果可以伤害则返回 <see langword="true"/>，否则 <see langword="false"/>。</returns>
    public override bool CanHitPlayer(Player target) => target == Target;

    /// <summary>
    /// 检测玩家的碰撞箱是否与竞技场环形边界发生碰撞。
    /// 通过 <see cref="ArenaRing"/> 进行环形碰撞测试。
    /// </summary>
    /// <param name="projHitbox">弹幕自身的碰撞矩形（未使用，因为使用环形判定）。</param>
    /// <param name="targetHitbox">目标玩家的碰撞矩形。</param>
    /// <returns>如果玩家碰撞箱与环形区域重叠则返回 <see langword="true"/>，否则返回 <see langword="false"/>。永不返回 <see langword="null"/>。</returns>
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => ArenaRing.Collides(targetHitbox);
}
