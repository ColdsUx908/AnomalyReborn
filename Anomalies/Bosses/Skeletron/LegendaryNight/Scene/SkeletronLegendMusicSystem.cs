namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed class SkeletronLegendMusicSystem : ModSystem
{
    public override void Load()
    {
        if (Main.dedServ)
            return;

        On_Main.UpdateAudio_DecideOnNewMusic += DecideOnNewMusic;
        On_Main.UpdateAudio_DecideOnTOWMusic += DecideOnTOWMusic;
    }
    private static void DecideOnNewMusic(On_Main.orig_UpdateAudio_DecideOnNewMusic orig, Main self)
    {
        orig(self);
        ApplyLegendaryNightMusic();
    }
    private static void DecideOnTOWMusic(On_Main.orig_UpdateAudio_DecideOnTOWMusic orig, Main self)
    {
        orig(self);
        ApplyLegendaryNightMusic();
    }
    private static void ApplyLegendaryNightMusic()
    {
        if (Main.dedServ || Main.gameMenu)
            return;

        // 场景优先级仍可能被其他骷髅王音乐覆盖，在音乐选择结束后应用当前阶段。
        // 复用场景检查，保留距离限制、转场时机和音乐包未加载时的回退。
        SkeletronLegendScene2 phase2Scene = ModContent.GetInstance<SkeletronLegendScene2>();
        if (phase2Scene.IsSceneEffectActive(Main.LocalPlayer))
        {
            Main.newMusic = phase2Scene.Music;
            return;
        }

        SkeletronLegendScene phase1Scene = ModContent.GetInstance<SkeletronLegendScene>();
        if (phase1Scene.IsSceneEffectActive(Main.LocalPlayer))
            Main.newMusic = phase1Scene.Music;
    }
}