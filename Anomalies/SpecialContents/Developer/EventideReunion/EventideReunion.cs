using Anomalies.Assets.Effects;
using Anomalies.DataStructures;
using Anomalies.GameContents;
using Anomalies.SpecialContents.Developer;

namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed class EventideReunion : AnomalyModItem, IDeveloperItem, ILocalizationPrefix
{
    public override string LocalizationCategory => "SpecialContents.Developer";
    public string LocalizationPrefix => AnomalySharedData.ModLocalizationPrefix + LocalizationCategory + ".EventideReunion";

    public static EnchantedParticleSet EnchantmentEnergyParticles = new(-1, 0.7f,
        () => Main.rand.NextFloat() switch
        {
            < 0.5f => Color.Red,
            _ => AnomalySharedData.RebornColor,
        },
        () => Color.White, 0.1f, 50f);

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
        Item.UseSound = SoundID.Item1;
        Item.useTurn = true;
        Item.knockBack = 3f;
        Item.shoot = ModContent.ProjectileType<EventideReunionHoldout>();
        Item.rare = ModContent.RarityType<Celestial>();
        Item.value = Celestial.CelestialPrice;
        Item.noUseGraphic = true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        StatModifier meleeModifier = player.GetTotalDamage(DamageClass.Melee);
        CombinedHooks.ModifyWeaponDamage(player, Item, ref meleeModifier);
        int newDamage = Math.Max(0, (int)(meleeModifier.ApplyTo(Item.damage) + 5E-06f));
        Projectile.NewProjectileAction(source, position, velocity, type, newDamage, knockback, player.whoAmI, p =>
        {
        });
        return false;
    }

    public override bool? CanHitNPC(Player player, NPC target) => false;

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = Item.Texture;
        Vector2 particleDrawCenter = position;

        EnchantmentEnergyParticles.Update();
        EnchantmentEnergyParticles.DrawSet(particleDrawCenter + Main.screenPosition);
        TODrawUtils.DrawInInventoryWithCustomSize(spriteBatch, position, frame, drawColor, origin, texture, 0.12f);
        return false;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        AnomalyItemTooltipModifier modifier = new(Item, tooltips);
        bool shift = Main.keyState.PressingShift();

        int index = modifier._LastTooltipIndex;

        IDeveloperItem.AddDeveloperItemIdentifier(Mod, tooltips, ++index);
        if (shift)
            IDeveloperItem.AddDeveloperItemOwnerIdentifier(Mod, tooltips, ++index, "VectorChaos");
        ILegendaryItem.AddLegendaryItemIdentifier(Mod, tooltips, ++index);

        modifier.Update();

        if (!shift)
            modifier.AddExpendedDisplayLine();
    }

    public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
    {
        if (TODrawUtils.TryDrawItemNameWithCustomShader(line, l => AnomalyEffects.Texts.EventideReunion.Data
            .SetCustomParameter("uScreenResolution", Main.ScreenSize.ToVector2() * Math.Max(Main.UIScale, 1f) / 2f)
            .SetCustomParameter("uScreenRatio", TODrawUtils.ScreenRatio)
            .SetCustomParameter("uPosition", new Vector2(l.X, l.Y))
            .Apply()))
        {
            return false;
        }

        return true;
    }
}