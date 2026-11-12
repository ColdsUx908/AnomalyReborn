namespace Anomalies.Tweaks.PreHardmode.Magic;

/// <summary>
/// 太空枪修改：
/// <list type="bullet">
/// <item/><description/>射速由 30 提升至 40。
/// <item/><description/>另见 <see cref="GreenLaser"/>。
/// </list>
/// </summary>
public sealed class SpaceGun : AnomalyItemBehavior<SpaceGun>
{
    public override int ApplyingType => ItemID.SpaceGun;

    public override void SetDefaultsFinal()
    {
        Item.shootSpeed = 40f;
    }
}

/// <summary>
/// 绿激光修改：
/// <list type="bullet">
/// <item/><description/>使用局部无敌帧，每个弹幕限命中特定 NPC 一次。
/// </list>
/// </summary>
public sealed class GreenLaser : AnomalyProjectileBehavior<GreenLaser>
{
    public override int ApplyingType => ProjectileID.GreenLaser;

    public override void SetDefaultsFinal()
    {
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }
}