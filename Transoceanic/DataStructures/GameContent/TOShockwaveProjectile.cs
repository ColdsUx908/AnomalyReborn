using Transoceanic.DataStructures.Geometry;
using Transoceanic.DataStructures.Particles;

namespace Transoceanic.DataStructures.GameContent;

/// <summary>
/// 表示一个冲击波类型的弹幕基类。该类继承自 <see cref="TOModProjectile"/>，
/// 提供了一套基于生命周期的缩放与透明度插值逻辑，并通过圆形碰撞检测实现非穿透性的范围效果。
/// </summary>
public abstract class TOShockwaveProjectile : TOModProjectile
{
    /// <summary>
    /// 获取一个值，该值指示此冲击波是否对玩家具有敌意。
    /// </summary>
    /// <value>
    /// 若为 <see langword="true"/>，则弹幕对玩家造成伤害（<see cref="Projectile.hostile"/> 为 <see langword="true"/>）；
    /// 否则，弹幕对敌怪友善（<see cref="Projectile.friendly"/> 为 <see langword="true"/>）。
    /// </value>
    public abstract bool Hostile { get; }

    /// <summary>
    /// 获取此冲击波能够命中的 NPC 类型 ID 列表。
    /// </summary>
    /// <value>
    /// 一个 <see cref="List{Int32}"/>，包含允许被此弹幕命中的 NPC 类型。
    /// 若为 <c>null</c> 或空列表，则 <see cref="CanHitNPC"/> 将默认返回 <see langword="false"/>。
    /// </value>
    public abstract List<int> NPCTypesToHit { get; }

    /// <summary>
    /// 获取此冲击波的完整生命周期长度，以游戏刻（ticks）为单位。
    /// </summary>
    /// <value>
    /// 一个正整数值，决定了弹幕从生成到消失所经历的帧数。
    /// 该值将用于计算 <see cref="LifeCompletion"/> 并驱动缩放与透明度动画。
    /// </value>
    public abstract int LifeTime { get; }

    /// <summary>
    /// 获取此冲击波在生命周期结束时应达到的最终缩放比例。
    /// </summary>
    /// <value>
    /// 一个浮点数，表示弹幕在消失瞬间的 <see cref="Projectile.scale"/> 目标值。
    /// 缩放动画使用指数缓出函数进行插值。
    /// </value>
    public abstract float FinalScale { get; }

    /// <summary>
    /// 获取或设置一个值，该值指示是否应使用高清晰度版本的纹理。
    /// </summary>
    /// <value>
    /// 默认值为 <see langword="false"/>，使用标准分辨率的圆形硬边纹理；
    /// 若为 <see langword="true"/>，则纹理路径将附加 "HD" 后缀，用于更高细节的渲染。
    /// </value>
    public virtual bool UseHDTexture => false;

    /// <summary>
    /// 获取当前冲击波生命周期的完成比例，范围在 0.0 到 1.0 之间。
    /// </summary>
    /// <value>
    /// 一个介于 0 和 1 之间的浮点数，表示 <see cref="Timer1"/> 与 <see cref="LifeTime"/> 的比值。
    /// 此值用于插值计算缩放、透明度等视觉参数。
    /// </value>
    public float LifeCompletion => (float)Timer1 / LifeTime;

    /// <summary>
    /// 获取此弹幕所使用的纹理路径。根据 <see cref="UseHDTexture"/> 的值，
    /// 返回标准或高清版本的圆形硬边纹理。
    /// </summary>
    public override string Texture => ParticleHandler.BaseParticleTexturePath + "HollowCircleHardEdge" + (UseHDTexture ? "HD" : "");

    /// <summary>
    /// 设置此弹幕类型的静态默认值。此处将 <see cref="ProjectileID.Sets.DrawScreenCheckFluff"/> 设置为 10000，
    /// 以确保弹幕在屏幕边缘外一定距离内仍能被正确绘制。
    /// </summary>
    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;

    /// <summary>
    /// 基本实现，设置冲击波的默认属性。包括尺寸、穿透性、敌友标记、生命周期、碰撞与液体行为。
    /// </summary>
    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.penetrate = -1;
        Projectile.friendly = !Hostile;
        Projectile.hostile = Hostile;
        Projectile.timeLeft = LifeTime;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    /// <summary>
    /// 基本实现，递增 <see cref="TOModProjectile.Timer1"/> 计数器，并根据生命周期进度计算缩放与透明度动画。
    /// </summary>
    public override void AI()
    {
        Timer1++;
        Projectile.scale = MathHelper.Lerp(0f, FinalScale, TOMathUtils.Interpolation.ExponentialEaseOut(LifeCompletion, 4f));
        Projectile.Opacity = TOMathUtils.Interpolation.QuadraticEaseInOut((1f - LifeCompletion) / 0.1f);
    }

    /// <summary>
    /// 基本实现，确定此弹幕是否可以命中指定的 NPC。
    /// </summary>
    /// <param name="target">待检测的 NPC 实例。</param>
    /// <returns>
    /// 若弹幕非敌对且目标 NPC 的类型包含在 <see cref="NPCTypesToHit"/> 列表中，则返回 <see langword="true"/>；
    /// 否则返回 <c>null</c> 以采用默认命中逻辑（对于敌对弹幕，此方法将阻止命中任何 NPC）。
    /// </returns>
    public override bool? CanHitNPC(NPC target) => !Hostile && NPCTypesToHit is not null && NPCTypesToHit.Contains(target.type);

    /// <summary>
    /// 基本实现，执行精确的碰撞检测，使用圆形区域与目标矩形进行判断。
    /// </summary>
    /// <param name="projHitbox">弹幕自身的矩形碰撞箱（此参数未直接使用）。</param>
    /// <param name="targetHitbox">目标实体的矩形碰撞箱。</param>
    /// <returns>
    /// 若以弹幕中心为圆心、根据纹理半径与当前缩放计算出的圆形与目标矩形相交，则返回 <see langword="true"/>；否则返回 <see langword="false"/>。
    /// </returns>
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => new Circle(Projectile.Center, (UseHDTexture ? PulseRing.TextureRadiusHD : PulseRing.TextureRadius) * Projectile.scale).Collides(targetHitbox);

    /// <summary>
    /// 基本实现，在弹幕绘制前调用，以弹幕中心绘制带透明度与缩放的纹理。
    /// </summary>
    /// <param name="lightColor">基础光照颜色，用于叠加透明度计算。</param>
    /// <returns>始终返回 <see langword="false"/>，以阻止默认绘制流程。</returns>
    public override bool PreDraw(ref Color lightColor)
    {
        SpriteBatch spriteBatch = Main.spriteBatch;
        ParticleHandler.EnterDrawRegion_Additive(spriteBatch);
        spriteBatch.DrawFromCenter(Projectile.Texture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor) * Projectile.Opacity, 0f, Projectile.scale);
        ParticleHandler.ExitParticleDrawRegion(spriteBatch);
        return false;
    }
}