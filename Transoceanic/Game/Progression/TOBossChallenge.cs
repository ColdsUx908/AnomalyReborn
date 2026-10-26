namespace Transoceanic.Game.Progression;

public sealed class TOBossChallenge : ModSystem, ITOLoader
{
    /// <summary>
    /// 当前世界中存活的 Boss 实体列表。
    /// 在 <see cref="PostUpdateNPCs"/> 中更新。
    /// </summary>
    public static List<NPC> BossList { get; internal set; } = [];

    /// <summary>
    /// 当前世界中是否有 Boss 存活。
    /// </summary>
    public static bool BossActive { get; internal set; }

    /// <summary>
    /// Boss 挑战计时器，从第一个 Boss 出现开始累计，每帧递增，直到所有 Boss 都被击败后重置为 0。
    /// </summary>
    public static TerrariaTimer BossTimer { get; internal set; }

    /// <summary>
    /// Boss 挑战开始事件，在第一个 Boss 出现的那一帧触发。
    /// </summary>
    public static event Action OnBossChallengeBegin;

    /// <summary>
    /// Boss 挑战结束事件，在最后一个 Boss 消失的那一帧触发。
    /// </summary>
    public static event Action OnBossChallengeEnd;

    public override void PostUpdateNPCs()
    {
        BossList = NPC.Bosses.ToList();
        BossActive = BossList.Count > 0;

        if (BossActive)
        {
            BossTimer++;
            if (BossTimer == 1)
                OnBossChallengeBegin?.Invoke();
        }
        else if (BossTimer > 0)
        {
            OnBossChallengeEnd?.Invoke();
            BossTimer = 0;
        }
    }

    public override void OnWorldLoad() => BossTimer = 0;

    public override void OnWorldUnload() => BossTimer = 0;

    void ITOLoader.Load()
    {
        OnBossChallengeBegin = null;
        OnBossChallengeEnd = null;
    }

    void ITOLoader.Unload()
    {
        BossList = [];
        BossActive = false;

        OnBossChallengeBegin = null;
        OnBossChallengeEnd = null;
    }
}
