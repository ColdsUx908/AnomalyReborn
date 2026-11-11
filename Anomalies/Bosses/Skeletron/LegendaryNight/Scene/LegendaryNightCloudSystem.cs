namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed class LegendaryNightCloudSystem : ModSystem
{
    /// <summary>清空云层时采用的风速值。</summary>
    private const float ClearingWindSpeed = 24f;

    public override void Load()
    {
        On_Main.updateCloudLayer += ClearBackgroundClouds;
        if (Main.dedServ)
            return;
        On_Cloud.addCloud += PreventNewClouds;
        On_Cloud.UpdateClouds += ClearExistingClouds;
    }

    private static void ClearBackgroundClouds(On_Main.orig_updateCloudLayer orig, Main self)
    {
        if (Main.gameMenu || !LegendaryNightHandler.TryGetSceneHead(out _))
        {
            orig(self);
            return;
        }

        if (Main.cloudBGActive <= 0f)
            return;

        // 结束原版远景阴云天气，保留当前透明度，让原版绘制逐步淡出。
        Main.cloudBGActive = 0f;
        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.WorldData);
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
