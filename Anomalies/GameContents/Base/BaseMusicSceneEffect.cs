namespace Anomalies.GameContents.Base;

/// <summary>
/// 用于实现模组 NPC 的音乐切换。
/// </summary>
public abstract class BaseMusicSceneEffect : ModSceneEffect
{
    /// <summary>
    /// 获取此场景效果的优先级。
    /// <para/><list type="bullet">
    /// <item/><description/><see cref="SceneEffectPriority.BossLow"/>：模组 Boss 的默认优先级，但存在一个问题，即 Boss 1 的音乐会在 1 帧内被排队播放。
    /// <item/><description/><see cref="SceneEffectPriority.BossMedium"/>：覆盖所有原版 Boss 音乐，包括事件音乐。
    /// <item/><description/><see cref="SceneEffectPriority.BossHigh"/>：在 <see cref="SceneEffectPriority.BossMedium"/> 的基础上进一步提高优先级，甚至覆盖月亮领主召唤前的音乐渐消效果。
    /// </list>
    /// </summary>
    public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;

    /// <summary>
    /// 获取此场景效果对应的模组音乐 ID。若返回 -1，则表示不使用模组音乐。
    /// <br/>在音乐模组未加载时，该属性也应返回 -1，以避免尝试播放不存在的音乐。
    /// </summary>
    public abstract int MusicModMusic { get; }
    /// <summary>
    /// 获取此场景效果对应的原版音乐 ID。若返回 -1，则表示不使用原版音乐。
    /// </summary>
    public abstract int VanillaMusic { get; }
    public virtual float MusicDistance => 5000f;

    /// <summary>
    /// 用于在检查场景效果是否激活之前进行预检查。默认返回 <see langword="true"/>，表示无需额外条件即可进行激活检查。
    /// </summary>
    /// <returns>返回 <see langword="false"/> 以阻止场景效果激活。</returns>
    public virtual bool PreCheckActive() => true;

    /// <summary>
    /// 检测指定的 NPC 是否符合触发此场景效果的条件。子类必须实现此方法以定义具体的触发逻辑。
    /// </summary>
    /// <param name="npc">要检查的 NPC 实例。</param>
    /// <returns>如果 NPC 符合触发条件，则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    public abstract bool CheckSingleNPC(NPC npc);

    public override int Music
    {
        get
        {
            if (MusicModMusic != -1)
                return MusicModMusic;

            return VanillaMusic;
        }
    }

    public override bool IsSceneEffectActive(Player player)
    {
        if (!PreCheckActive())
            return false;

        if (MusicModMusic == -1 && VanillaMusic == -1)
            return false;

        /*
        // You can't even edit music if the game has Otherworld toggle on lol.
        // Theoretically, reflection would only occur if the music mod is disabled
        if (MusicModMusic == -1)
        {
            FieldInfo swapMusicField = typeof(Main).GetField("swapMusic", BindingFlags.Static | BindingFlags.NonPublic);
            bool musicSwapped = (bool)swapMusicField.GetValue(null);
            bool playingOtherworld = (!Main.drunkWorld && musicSwapped) || (Main.drunkWorld && !musicSwapped);
            if (VanillaMusic == -1 && !playingOtherworld)
                return false;
            if (OtherworldMusic == -1 && playingOtherworld)
                return false;
        }
        */

        foreach (NPC npc in NPC.ActiveNPCs)
        {
            if (CheckSingleNPC(npc) && new Circle(npc.Center, MusicDistance).Collides(player.Hitbox))
                return true;
        }

        return false;
    }
}