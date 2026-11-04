#if DEBUG
using Anomalies.Bosses.Skeletron.LegendaryNight;
using Anomalies.GameContents;

namespace Anomalies.SpecialContents.Developer;

public sealed partial class TestItem
{
    partial void ConfigureBossTestItem()
    {
        Item.rare = ModContent.RarityType<Celestial>();
        Item.value = Celestial.CelestialPrice;
        Item.shoot = ModContent.ProjectileType<PlatinumCoin>();
    }

    partial void ConfigureBossTestAvailability(ref bool available) => available = true;

    partial void ShootBossTest(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.altFunctionUse == 2)
        {
            Projectile.NewProjectileAction(source, Main.MouseWorld, Vector2.Zero,
                ModContent.ProjectileType<DemonShadow>(), damage, 0f, player.whoAmI, action: p => p.ai[0] = player.whoAmI + 1);
            return;
        }

        Projectile.NewProjectileAction(source, position, velocity, type, damage, knockback, action: p =>
        {
            p.scale = 1f;
        });
    }
}
#endif
