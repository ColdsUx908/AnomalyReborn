namespace Anomalies.GameContents.Drinks;

public sealed class PlumGreenTea : AnomalyModItem
{
    public override string LocalizationCategory => "GameContents.Drinks";

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 10;
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
        ItemID.Sets.FoodParticleColors[Type] =
        [
            new Color(180, 223, 113),
            new Color(92, 173, 48),
            new Color(39, 104, 32)
        ];
        ItemID.Sets.IsFood[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.DefaultToFood(36, 50, BuffID.WellFed3, 108000, true, 30);
        Item.rare = ItemRarityID.Orange;
        Item.value = 200000;
    }
}