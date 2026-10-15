namespace Anomalies.GameContents.Drinks;

public sealed class BubbleTea : AnomalyModItem
{
    public override string LocalizationCategory => "GameContents.Drinks";

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 10;
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
        ItemID.Sets.FoodParticleColors[Type] =
        [
            new Color(232, 199, 158),
            new Color(190, 144, 100),
            new Color(70, 43, 28)
        ];
        ItemID.Sets.IsFood[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.DefaultToFood(56, 80, BuffID.WellFed3, 108000, true, 30);
        Item.rare = ItemRarityID.Orange;
        Item.value = 200000;
    }
}