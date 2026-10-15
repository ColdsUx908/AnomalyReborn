namespace Anomalies.GameContents.Drinks;

public sealed class RaspberryBlackTea : AnomalyModItem
{
    public override string LocalizationCategory => "GameContents.Drinks";

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 10;
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
        ItemID.Sets.FoodParticleColors[Type] =
        [
            new Color(255, 198, 104),
            new Color(231, 101, 35),
            new Color(166, 39, 29)
        ];
        ItemID.Sets.IsFood[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.DefaultToFood(32, 46, BuffID.WellFed3, 108000, true, 30);
        Item.rare = ItemRarityID.Orange;
        Item.value = 200000;
    }
}