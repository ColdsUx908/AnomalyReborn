using Anomalies.Assets.Effects;
using Anomalies.GameContents;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public class MemoryofAstraeus : AnomalyModItem
{
    public override string Texture => LegendaryNightHandler.LegendaryNightPath + "Summoning/MemoryofAstraeus";

    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    public override void SetStaticDefaults()
    {
        ItemID.Sets.SortingPriorityBossSpawns[Type] = 19; // Celestial Sigil
        AnomalySharedData.DontApplyAnomalyRarityShaderItems[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.width = 40;
        Item.height = 60;
        Item.rare = ModContent.RarityType<AnomalyUltramundaneRarity>();
        Item.value = AnomalyRarity.AnomalyPrice;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) => itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossItem;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<FallenStarEssence>())
            .AddIngredient(ItemID.Bottle)
            .AddIngredient(ItemID.FallenStar, 5)
            .AddIngredient(ItemID.GoldBar, 5)
            .AddTile(TileID.DemonAltar)
            .Register();
    }

    public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
    {
        if (TODrawUtils.TryDrawItemNameWithCustomShader(line, l => AnomalyEffects.Texts.LegendaryNightGold.Data
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
