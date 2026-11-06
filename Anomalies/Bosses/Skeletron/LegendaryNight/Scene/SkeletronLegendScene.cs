using Anomalies.GameContents.Base;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>传说之夜音乐场景，沿用音乐距离与资源回退，共用场景主体选择规则。</summary>
public abstract class LegendaryNightMusicScene : BaseMusicSceneEffect
{
    protected abstract bool Phase2Music { get; }

    public override bool CheckSingleNPC(NPC npc) => LegendaryNightHandler.TryGetSceneCandidate(npc, out SkeletronLegendHead head)
        && head.Phase2MusicActive == Phase2Music;

    public override bool IsSceneEffectActive(Player player) => PreCheckActive()
        && (MusicModMusic != -1 || VanillaMusic != -1)
        && LegendaryNightHandler.TryGetSceneHead(out SkeletronLegendHead head)
        && head.Phase2MusicActive == Phase2Music
        && new Circle(head.NPC.Center, MusicDistance).Collides(player.Hitbox);
}

public sealed class SkeletronLegendScene : LegendaryNightMusicScene
{
    protected override bool Phase2Music => false;
    public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

    public override int MusicModMusic => AnomaliesMusicHandler.CurseoftheDungeon;
    public override int VanillaMusic => MusicID.Boss1;
}

public sealed class SkeletronLegendScene2 : LegendaryNightMusicScene
{
    protected override bool Phase2Music => true;
    public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

    public override int MusicModMusic => AnomaliesMusicHandler.TornfromtheHeavens;
    public override int VanillaMusic => MusicID.EmpressOfLight;

    public override bool IsSceneEffectActive(Player player)
    {
        bool active = base.IsSceneEffectActive(player);
        // 直接使用完整淡入系数，实际音量仍遵循玩家的音乐音量设置。

        int music = Music;
        if (active && !Main.dedServ && music > 0 && music < Main.musicFade.Length)
            Main.musicFade[music] = 1f;
        return active;
    }
}