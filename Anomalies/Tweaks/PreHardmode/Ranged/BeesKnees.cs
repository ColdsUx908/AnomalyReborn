using Anomalies.Tweaks.PreHardmode.Projectiles;

namespace Anomalies.Tweaks.PreHardmode.Ranged;

/// <summary>
/// 蜂膝弓修改：
/// <list type="bullet">
/// <item/><description/>基础伤害由 23 降低至 16。
/// <item/><description/>射速由 8 提升至 12。
/// <item/><description/>另见 <see cref="BeeBehavior"/>。
/// </list>
/// </summary>
public sealed class BeesKnees : AnomalyItemBehavior<BeesKnees>
{
    public override int ApplyingType => ItemID.BeesKnees;

    public override void SetDefaultsFinal()
    {
        Item.damage = 16;
        Item.shootSpeed = 12f;
    }
}
