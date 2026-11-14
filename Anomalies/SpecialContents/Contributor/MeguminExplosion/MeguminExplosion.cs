using Anomalies.Assets.Effects;
using Anomalies.GameContents;

namespace Anomalies.SpecialContents.Contributor.MeguminExplosion;

public sealed class MeguminExplosion : AnomalyModItem, IContributorItem, ILegendaryItem, ILocalizationPrefix
{
    public override bool IsLoadingEnabled(Mod mod) => AnomalySpecialContentToggler.MeguminExplosion;

    public const int ChargeDuration = 180;
    public const float CastRange = 1200f;

    public override string LocalizationCategory => "SpecialContents.Contributor";
    public string LocalizationPrefix => AnomalySharedData.ModLocalizationPrefix + LocalizationCategory + ".MeguminExplosion";

    public override void SetStaticDefaults() => Item.staff[Type] = true;

    public override void SetDefaults()
    {
        Item.width = 64;
        Item.height = 88;
        Item.damage = 4500;
        Item.DamageType = DamageClass.Magic;
        Item.mana = 160;
        Item.useTime = Item.useAnimation = 30;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.channel = true;
        Item.noMelee = true;
        Item.knockBack = 12f;
        Item.shoot = ModContent.ProjectileType<MeguminCasting>();
        Item.shootSpeed = 1f;
        Item.rare = ModContent.RarityType<MeguminRarity>();
        Item.value = Item.sellPrice(gold: 30);
        Item.UseSound = SoundID.Item117 with { Volume = 0.55f, Pitch = -0.35f };
    }

    public override bool CanUseItem(Player player) => !player.HasBuff<MeguminExhaustion>()
        && player.ownedProjectileCounts[Item.shoot] == 0
        && player.ownedProjectileCounts[ModContent.ProjectileType<MeguminBlast>()] == 0;

    public override void ModifyManaCost(Player player, ref float reduce, ref float mult)
    {
        // Vanilla can recheck a channeling staff's use cost; only the initial cast pays that cost.
        // The beam pays its separate fixed maintenance cost through CheckMana with an explicit amount.
        if (player.ownedProjectileCounts[Item.shoot] > 0
            || player.ownedProjectileCounts[ModContent.ProjectileType<MeguminBlast>()] > 0)
            mult = 0f;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity,
        int type, int damage, float knockback)
    {
        Vector2 target = ClampTarget(player, Main.MouseWorld);
        Projectile.NewProjectileAction(source, player.MountedCenter, velocity, type, damage, knockback, player.whoAmI,
            projectile =>
            {
                projectile.ai[1] = target.X;
                projectile.ai[2] = target.Y;
            });
        return false;
    }

    internal static Vector2 ClampTarget(Player player, Vector2 target)
    {
        Vector2 offset = target - player.MountedCenter;
        if (offset.LengthSquared() > CastRange * CastRange)
            target = player.MountedCenter + offset.SafeNormalize(Vector2.UnitX * player.direction) * CastRange;
        return Vector2.Clamp(target, new Vector2(32f), new Vector2(Main.maxTilesX * 16f - 32f, Main.maxTilesY * 16f - 32f));
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        AnomalyItemTooltipModifier modifier = new(Item, tooltips);
        bool shift = Main.keyState.PressingShift();
        int index = modifier._LastTooltipIndex;

        foreach (TooltipLine line in tooltips)
            if (IsLoreLine(line.Name))
                line.OverrideColor = MeguminRarity.LoreColor;
        IContributorItem.AddContributorItemIdentifier(Mod, tooltips, ++index);
        if (shift)
            IContributorItem.AddContributorItemOwnerIdentifier(Mod, tooltips, ++index, "Korvath");
        ILegendaryItem.AddLegendaryItemIdentifier(Mod, tooltips, ++index);

        modifier.Update();
        if (!shift)
            modifier.AddExpendedDisplayLine();
    }

    // This endgame legendary keeps its fixed 4500 base damage; equipment and prefixes still apply normally.
    public void LegendaryUpdate() { }
    public void LegendaryUpdate(Player player) => LegendaryUpdate();
    public override void UpdateInventory(Player player) => LegendaryUpdate(player);

    private static bool IsLoreLine(string name) => name is "Tooltip0" or "Tooltip1" or "Tooltip2";

    public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
    {
        if (!AnomalyClientConfig.Instance.EnableShaders || AnomalyEffects.Texts.MeguminStaff is null)
            return true;

        void ApplyGradient(DrawableTooltipLine text, float palette) => AnomalyEffects.Texts.MeguminStaff.Data
            .SetCustomParameter("uTextOrigin", new Vector2(text.X, text.Y) * Main.UIScale)
            .SetCustomParameter("uTextScale", Main.UIScale)
            .SetCustomParameter("uPalette", palette).Apply();

        if (TODrawUtils.TryDrawItemNameWithCustomShader(line, text => ApplyGradient(text, 0f)))
            return false;
        if (line.Mod != "Terraria" || !IsLoreLine(line.Name))
            return true;

        Vector2 position = new(line.X, line.Y);
        TODrawUtils.DrawBorderString(Main.spriteBatch, line.Font, line.Text, position, Color.Black,
            4, 1f, line.BaseScale, line.Rotation);
        using (Main.spriteBatch.Scope(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, RasterizerState.CullNone, transformMatrix: Main.UIScaleMatrix, effectIsNull: true))
        {
            ApplyGradient(line, 1f);
            Main.spriteBatch.DrawString(line.Font, line.Text, position, Color.White, line.Rotation,
                line.Origin, line.BaseScale, SpriteEffects.None, 0f);
        }
        return false;
    }

    public override void AddRecipes()
    {
        CreateRecipe().AddIngredient(ItemID.InfernoFork).AddIngredient(ItemID.Ruby, 999)
            .AddIngredient(ItemID.LunarBar, 40).AddIngredient(ItemID.FragmentSolar, 400)
            .AddTile(TileID.LunarCraftingStation).Register();
    }
}

public sealed class MeguminExhaustion : ModBuff
{
    public override bool IsLoadingEnabled(Mod mod) => AnomalySpecialContentToggler.MeguminExplosion;

    public override string Texture => $"Terraria/Images/Buff_{BuffID.ManaSickness}";
    public override string LocalizationCategory => "SpecialContents.Contributor";

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.moveSpeed -= 0.35f;
        player.manaRegenDelay = Math.Max(player.manaRegenDelay, 30);
    }
}
