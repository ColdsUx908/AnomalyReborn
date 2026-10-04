using Anomalies.GameContents;

namespace Anomalies.SpecialContents.Contributor.VacuousBlack;

public sealed class VacuousBlack : AnomalyModItem, ILegendaryItem, ILocalizationPrefix
{
    #region 传奇
    public int Phase = 1;
    public int SubPhase = 1;

    public void LegendaryUpdate() => ImmaculateWhite.ImmaculateWhite.GetPhase(out Phase, out SubPhase);

    public void LegendaryUpdate(Player player) => LegendaryUpdate();
    #endregion 传奇

    public override string LocalizationCategory => "SpecialContents.Contributor";

    public string LocalizationPrefix => AnomalySharedData.ModLocalizationPrefix + LocalizationCategory + ".VacuousBlack";

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

    public override void UpdateInventory(Player player)
    {
        LegendaryUpdate(player);
    }

    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        damage *= ImmaculateWhite.ImmaculateWhite.GetDamageMultiplier(Phase, SubPhase);
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        AnomalyItemTooltipModifier modifier = new(Item, tooltips);
        bool shift = Main.keyState.PressingShift();
        if (!modifier.TryGet(null, "Tooltip0", out int index, out _))
            return;

        IContributorItem.AddContributorItemIdentifier(Mod, tooltips, ++index);
        if (shift)
            IContributorItem.AddContributorItemOwnerIdentifier(Mod, tooltips, ++index, "莫沧然");
        ILegendaryItem.AddLegendaryItemIdentifier(Mod, tooltips, ++index);

        modifier.Update();

        if (!shift)
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

