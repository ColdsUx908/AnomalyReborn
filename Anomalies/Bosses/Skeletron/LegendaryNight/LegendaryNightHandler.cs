namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed class LegendaryNightHandler
{
    public const string LegendaryNightPath = AnomalySharedData.ModPath + "Bosses/Skeletron/LegendaryNight/";
    public const string LegendaryNightEntityPath = AnomalySharedData.ModPath + "Bosses/Skeletron/LegendaryNight/Entities/";
    public const string LocalizationCategory = "Bosses.Skeletron.LegendaryNight";
    public static readonly Color BoneColor = new(233, 225, 199);
    public static readonly Color SlimeColor = new(105, 140, 255);
    public static readonly Color StarColor = new(255, 215, 0);
    public static readonly Color PlatinumColor = new(242, 243, 236);

    public static void SpawnBlessingBurst(Vector2 position)
    {
        if (Main.dedServ)
            return;
        for (int i = 0; i < 180; i++)
        {
            Vector2 velocity = Main.rand.NextPolarVector2(3f, 20f);
            Color color = Color.Lerp(StarColor, BoneColor, Main.rand.NextFloat());
            ParticleHandler.SpawnParticle(new OrbParticle(position, velocity, Main.rand.Next(30, 70),
                Main.rand.NextFloat(0.5f, 1.5f), color, lifeEndRatio: 0.2f));
        }
    }
}
