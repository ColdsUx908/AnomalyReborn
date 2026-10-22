using Anomalies.GameContents;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed class FallenStarEssence : AnomalyModItem
{
    public override string Texture => LegendaryNightHandler.LegendaryNightPath + "Summoning/FallenStarEssence";

    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    public override void SetDefaults()
    {
        Item.width = 40;
        Item.height = 42;
        Item.maxStack = 5;
        Item.rare = ModContent.RarityType<AnomalyUltramundaneRarity>();
        Item.value = AnomalyRarity.AnomalyPrice;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) => itemGroup = ContentSamples.CreativeHelper.ItemGroup.Material;

    public override void Update(ref float gravity, ref float maxFallSpeed)
    {
        gravity = 0f;
        maxFallSpeed = 0f;
    }
}