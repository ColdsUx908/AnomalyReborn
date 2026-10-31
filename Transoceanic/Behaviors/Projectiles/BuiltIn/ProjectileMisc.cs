using Transoceanic.GlobalInstances;
using Transoceanic.Graphics.Particles;
using Transoceanic.Graphics.Particles.BuiltIn;

namespace Transoceanic.Behaviors.Projectiles.BuiltIn;

/// <summary>
/// 维护弹幕按速度旋转、残影更新与绘制。
/// </summary>
internal sealed class ProjectileMisc : GlobalProjectileBehavior
{
    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>500m</c>。</remarks>
    public override decimal Priority => 500m;

    /// <inheritdoc/>
    /// <remarks>
    /// 返回 true，允许继续执行弹幕 AI。
    /// </remarks>
    public override bool PreAI(Projectile projectile)
    {
        return true;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 按开关让弹幕朝速度旋转，并更新残影、移除超期残影。
    /// </remarks>
    public override void PostAI(Projectile projectile)
    {
        if (projectile.AlwaysRotating)
            projectile.VelocityToRotation(projectile.RotationOffset);

        TOGlobalProjectile ocean = projectile.Ocean;
        if (Main.dedServ || ocean.ActiveAfterimages is not { } afterimages)
            return;
        foreach (AfterimageParticle afterimage in afterimages)
            ParticleHandler.UpdateParticle(afterimage);
        afterimages.RemoveAll(a => a.Timer > a.Lifetime);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 绘制启用默认绘制的弹幕残影，再返回 true 允许绘制弹幕。
    /// </remarks>
    public override bool PreDraw(Projectile projectile, ref Color lightColor)
    {
        if (Main.dedServ || projectile.Ocean.ActiveAfterimages is not { } afterimages)
            return true;
        SpriteBatch spriteBatch = Main.spriteBatch;
        foreach (AfterimageParticle afterimage in afterimages)
        {
            if (afterimage.UseDefaultDraw)
                afterimage.Draw(spriteBatch);
        }

        return true;
    }
}
