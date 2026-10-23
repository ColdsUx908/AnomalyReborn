using Anomalies.Assets.Effects;
using Anomalies.GameContents;
using Terraria.Utilities;

namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed partial class EventideReunion : AnomalyModItem, IDeveloperItem, ILocalizationPrefix
{
    private static readonly UnifiedRandom _TooltipSparkleRandom = new(1);

    public const string LocalizationCategoryForEventideReunion = "SpecialContents.Developer.EventideReunion";
    // 物品名称自动追加 EventideReunion，正好对应公共分组下的 DisplayName / Tooltip。
    public override string LocalizationCategory => "SpecialContents.Developer";
    public string LocalizationPrefix => AnomalySharedData.ModLocalizationPrefix + LocalizationCategoryForEventideReunion;

    public static EventideReunionParticleSet EnchantmentEnergyParticles = new();

    public const int OriginalUseTime = 30;

    public override void SetDefaults()
    {
        Item.width = 312;
        Item.height = 312;
        Item.damage = 24;
        Item.DamageType = DamageClass.Melee;
        Item.useTime = OriginalUseTime;
        Item.useAnimation = OriginalUseTime;
        Item.useStyle = ItemUseStyleID.Rapier;
        Item.autoReuse = true;
        Item.channel = true;
        Item.useTurn = true;
        Item.knockBack = 3f;
        Item.shoot = ModContent.ProjectileType<EventideReunionHoldout>();
        Item.rare = ModContent.RarityType<Celestial>();
        Item.value = Celestial.CelestialPrice;
        Item.noUseGraphic = true;
        Item.noMelee = true;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) => itemGroup = ContentSamples.CreativeHelper.ItemGroup.MeleeWeapon;

    public override bool AltFunctionUse(Player ownerPlayer) => true;

    public override bool CanUseItem(Player ownerPlayer) => ownerPlayer.altFunctionUse != 2
        && ownerPlayer.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionSelection>()] == 0
        && ownerPlayer.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionSunSkill>()] == 0
        && !EventideReunionDeathSkill.IsBlockingActions(ownerPlayer)
        && ownerPlayer.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionSamsaraSkill>()] == 0
        && ownerPlayer.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionHoldout>()] == 0;

    // 仅由持有者客户端在松开右键且选中符文时调用一次。后续技能在这里分派并使用 Action 接口生成同步弹幕。
    public void ReleaseSkill(Player ownerPlayer, EventideReunionAspect aspect, Vector2 aimDirection)
    {
        switch (aspect)
        {
            case EventideReunionAspect.Sun:
                Item weapon = ownerPlayer.HeldItem;
                if (ownerPlayer.whoAmI != Main.myPlayer || ownerPlayer.Anomaly.EventideReunion_SunCooldown > 0
                    || weapon.type != ModContent.ItemType<EventideReunion>() || ownerPlayer.dead || ownerPlayer.noItems || ownerPlayer.CCed
                    || ownerPlayer.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionSunSkill>()] > 0)
                    return;
                Projectile.NewProjectileAction<EventideReunionSunSkill>(ownerPlayer.GetSource_ItemUse(weapon),
                    ownerPlayer.Center, Vector2.Zero, ownerPlayer.GetWeaponDamage(weapon),
                    ownerPlayer.GetWeaponKnockback(weapon, weapon.knockBack), ownerPlayer.whoAmI, p =>
                    {
                        p.ai[2] = aimDirection.X >= 0f ? 1f : -1f;
                        p.GetModProjectile<EventideReunionSunSkill>().Target = ownerPlayer.Center + aimDirection * 320f;
                        ownerPlayer.Anomaly.EventideReunion_SunCooldown = EventideReunionResonancePlayer.Release(ownerPlayer, aspect, EventideReunionSunSkill.Cooldown);
                        p.damage = p.originalDamage = ownerPlayer.GetWeaponDamage(weapon);
                    });
                break;
            case EventideReunionAspect.Samsara:
                Item samsaraWeapon = ownerPlayer.HeldItem;
                if (ownerPlayer.whoAmI != Main.myPlayer || ownerPlayer.Anomaly.EventideReunion_SamsaraCooldown > 0
                    || samsaraWeapon.type != ModContent.ItemType<EventideReunion>() || ownerPlayer.dead || ownerPlayer.noItems || ownerPlayer.CCed
                    || ownerPlayer.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionSamsaraSkill>()] > 0)
                    return;
                Projectile.NewProjectileAction<EventideReunionSamsaraSkill>(ownerPlayer.GetSource_ItemUse(samsaraWeapon),
                    ownerPlayer.Center, aimDirection.SafeNormalize(Vector2.UnitX * ownerPlayer.direction),
                    ownerPlayer.GetWeaponDamage(samsaraWeapon), ownerPlayer.GetWeaponKnockback(samsaraWeapon, samsaraWeapon.knockBack), ownerPlayer.whoAmI,
                    p =>
                    {
                        p.ai[2] = ownerPlayer.direction;
                        ownerPlayer.Anomaly.EventideReunion_SamsaraCooldown = EventideReunionResonancePlayer.Release(ownerPlayer, aspect, EventideReunionSamsaraRecord.Cooldown);
                        p.damage = p.originalDamage = ownerPlayer.GetWeaponDamage(samsaraWeapon);
                    });
                break;
            case EventideReunionAspect.Life:
                break; // 生命相·新芽赞歌
            case EventideReunionAspect.Death:
                Item deathWeapon = ownerPlayer.HeldItem;
                if (ownerPlayer.whoAmI != Main.myPlayer || ownerPlayer.Anomaly.EventideReunion_DeathCooldown > 0
                    || deathWeapon.type != ModContent.ItemType<EventideReunion>() || ownerPlayer.dead || ownerPlayer.noItems || ownerPlayer.CCed
                    || EventideReunionDeathSkill.IsBlockingActions(ownerPlayer))
                    return;
                Projectile.NewProjectileAction<EventideReunionDeathSkill>(ownerPlayer.GetSource_ItemUse(deathWeapon),
                    ownerPlayer.Center, aimDirection.SafeNormalize(Vector2.UnitX * ownerPlayer.direction),
                    ownerPlayer.GetWeaponDamage(deathWeapon), ownerPlayer.GetWeaponKnockback(deathWeapon, deathWeapon.knockBack), ownerPlayer.whoAmI,
                    p =>
                    {
                        ownerPlayer.Anomaly.EventideReunion_DeathCooldown = EventideReunionResonancePlayer.Release(ownerPlayer, aspect, EventideReunionDeathSkill.Cooldown);
                        p.damage = p.originalDamage = ownerPlayer.GetWeaponDamage(deathWeapon);
                    });
                break;
            case EventideReunionAspect.Sacredness:
                break; // 神圣相·圣裁贯日
            case EventideReunionAspect.Star:
                break; // 恒星相·星核坍缩
        }
    }

    public override bool Shoot(Player ownerPlayer, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int projectileType, int damage, float knockback)
    {
        StatModifier meleeModifier = ownerPlayer.GetTotalDamage(DamageClass.Melee);
        CombinedHooks.ModifyWeaponDamage(ownerPlayer, Item, ref meleeModifier);
        int newDamage = Math.Max(0, (int)(meleeModifier.ApplyTo(Item.damage) + 5E-06f));
        Projectile.NewProjectileAction(source, position, velocity, projectileType, newDamage, knockback, ownerPlayer.whoAmI, p =>
        {
        });
        return false;
    }

    public override bool? CanHitNPC(Player ownerPlayer, NPC target) => false;

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float currentScale)
    {
        Texture2D drawTexture = Item.Texture;
        Vector2 particleDrawCenter = position;

        EnchantmentEnergyParticles.Update();
        EnchantmentEnergyParticles.DrawSet(particleDrawCenter + Main.screenPosition);
        TODrawUtils.DrawInInventoryWithCustomSize(spriteBatch, position, frame, drawColor, origin, drawTexture, 0.12f);
        return false;
    }
}
