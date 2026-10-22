namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed class LegendaryNightCloudSystem : ModSystem
{
    private const float ClearingWindSpeed = 24f;

    public override void Load()
    {
        if (Main.dedServ)
            return;
        On_Cloud.addCloud += PreventNewClouds;
        On_Cloud.UpdateClouds += ClearExistingClouds;
    }

    public override void Unload()
    {
        if (Main.dedServ)
            return;
        On_Cloud.addCloud -= PreventNewClouds;
        On_Cloud.UpdateClouds -= ClearExistingClouds;
    }

    private static void PreventNewClouds(On_Cloud.orig_addCloud orig)
    {
        if (LegendaryNightSky.Instance?.CloudSuppressionActive != true)
            orig();
    }

    private static void ClearExistingClouds(On_Cloud.orig_UpdateClouds orig)
    {
        if (LegendaryNightSky.Instance?.CloudSuppressionActive != true)
        {
            orig();
            return;
        }

        // 仅加速云层的原版移动，离开画面后由原版回收，不影响真实天气风速。
        float originalWind = Main.windSpeedCurrent;
        try
        {
            Main.windSpeedCurrent = originalWind < 0f ? -ClearingWindSpeed : ClearingWindSpeed;
            orig();
        }
        finally
        {
            Main.windSpeedCurrent = originalWind;
        }
    }
}
