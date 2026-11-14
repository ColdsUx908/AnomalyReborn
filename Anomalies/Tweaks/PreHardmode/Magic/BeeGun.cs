using Anomalies.Tweaks.PreHardmode.Projectiles;

namespace Anomalies.Tweaks.PreHardmode.Magic;

/// <summary>
/// 蜜蜂枪修改：
/// <list type="bullet">
/// <item/><description/>基础伤害由 9 降低至 7。
/// <item/><description/>另见 <see cref="BeeBehavior"/>。
/// </list>
/// </summary>
public sealed class BeeGun : AnomalyItemBehavior<BeeGun>
{
    public override int ApplyingType => ItemID.BeeGun;

    public override void SetDefaultsFinal()
    {
        Item.damage = 7;
    }
}
