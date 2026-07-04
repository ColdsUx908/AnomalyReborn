// Developed by ColdsUx

using CalamityAnomalies.GameContents.Contributor.Mocangran_ImmaculateWhite;

namespace CalamityAnomalies.GameContents.Contributor.Mocangran_VacuousBlack;

public sealed class VacuousBlack : CALegendaryItem, ILocalizationPrefix
{
    #region 传奇
    public override void LegendaryUpdate() => ImmaculateWhite.GetPhase(out Phase, out SubPhase);

    public override void LegendaryUpdate(Player player)
    {
        LegendaryUpdate();
    }
    #endregion 传奇

    public override string LocalizationCategory => "GameContents.Contributor";

    public string LocalizationPrefix => CASharedData.ModLocalizationPrefix + LocalizationCategory + ".VacuousBlack";

    public override void SetStaticDefaults()
    {
        Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 15));
        ItemID.Sets.AnimatesAsSoul[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.width = 42;
        Item.height = 42;
        Item.damage = 4;
        Item.DamageType = DamageClass.Summon;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.autoReuse = true;
        Item.channel = true;
        Item.UseSound = SoundID.Item4;
        Item.useTurn = true;
        Item.knockBack = 9f;
        Item.shoot = ModContent.ProjectileType<VacuousBlackMinion>();
        Item.shootSpeed = 0f;
        Item.buffType = ModContent.BuffType<VacuousBlackBuff>();
        Item.rare = ModContent.RarityType<Celestial>();
        Item.value = Celestial.CelestialPrice;
        Item.noUseGraphic = true;
    }

    public override bool CanUseItem(Player player)
    {
        return player.AvailableMinionSlots >= 1;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (Projectile.ActiveProjectiles.TryGetFirst(p => p.type == type && p.owner == player.whoAmI, out Projectile existingProjectile))
        {
            existingProjectile.minionSlots++;
            existingProjectile.netUpdate = true;
            return false;
        }
        player.AddBuff(Item.buffType, 2);
        return true;
    }

    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        damage *= ImmaculateWhite.GetDamageMultiplier(Phase, SubPhase);
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        CAItemTooltipModifier modifier = new(Item, tooltips);
        if (!modifier.TryGet(null, "Tooltip0", out int index, out _))
            return;

        AddContributorItemIdentifier(tooltips, ++index);
        AddLegendaryItemIdentifier(tooltips, ++index);

        modifier.Update();

        if (Main.keyState.PressingShift())
        {

        }
        else
            modifier.AddExpendedDisplayLine();
    }

    public override void Update(ref float gravity, ref float maxFallSpeed)
    {
        gravity = 0f;
        maxFallSpeed = 0f;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Star)
            .AddIngredient(ItemID.AngelStatue)
            .AddIngredient(ItemID.AngelHalo)
            .AddCondition(Condition.NearShimmer)
            .Register();
    }
}
