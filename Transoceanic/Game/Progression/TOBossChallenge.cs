namespace Transoceanic.Game.Progression;

/// <summary>
/// 跟踪当前世界的 Boss 列表、挑战计时及开始和结束事件。
/// </summary>
public sealed class TOBossChallenge : ModSystem
{
    private static readonly List<NPC> _BossList = [];

    /// <summary>
    /// 当前世界中存活的 Boss 实体列表。
    /// 在 <see cref="PostUpdateNPCs"/> 中更新。
    /// </summary>
    /// <remarks>只读包装限制列表修改，不会使其中的 NPC 对象不可变。</remarks>
    public static IReadOnlyList<NPC> BossList { get; } = _BossList.AsReadOnly();

    /// <summary>
    /// 当前世界中是否有 Boss 存活。
    /// </summary>
    public static bool BossActive { get; internal set; }

    /// <summary>
    /// Boss 挑战计时器，从第一个 Boss 出现开始累计，每帧递增，直到所有 Boss 消失或退出世界后重置为 0。
    /// </summary>
    public static TerrariaTimer BossTimer { get; internal set; }

    /// <summary>
    /// Boss 挑战开始事件，在第一个 Boss 出现的那一帧触发。
    /// </summary>
    public static event Action OnBossChallengeBegin;

    /// <summary>
    /// Boss 挑战结束事件，在最后一个 Boss 消失或退出仍在挑战的世界时触发。
    /// 触发前列表、活动标记和计时器均已清空；重复退出不会再次触发。结束不表示一定击败 Boss。
    /// </summary>
    public static event Action OnBossChallengeEnd;

    /// <inheritdoc/>
    /// <remarks>
    /// 每次 NPC 更新后刷新列表并累计挑战时间；首次发现 Boss 时开始，最后一个 Boss 消失时结束。
    /// </remarks>
    public override void PostUpdateNPCs()
    {
        _BossList.Clear();
        foreach (NPC boss in NPC.Bosses)
            _BossList.Add(boss);
        BossActive = _BossList.Count > 0;

        if (BossActive)
        {
            BossTimer++;
            if (BossTimer == 1)
                OnBossChallengeBegin?.Invoke();
        }
        else if (BossTimer > 0)
        {
            EndChallenge();
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 进入世界时清空旧世界的列表、活动标记与计时器，不触发挑战事件。
    /// </remarks>
    public override void OnWorldLoad()
    {
        _BossList.Clear();
        BossActive = false;
        BossTimer = 0;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 退出世界时清空挑战状态；仍有挑战时显式触发一次结束事件。
    /// </remarks>
    public override void OnWorldUnload() => EndChallenge();

    private static void EndChallenge()
    {
        bool wasActive = BossActive || BossTimer > 0;
        _BossList.Clear();
        BossActive = false;
        BossTimer = 0;
        if (wasActive)
            OnBossChallengeEnd?.Invoke();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 在框架内容加载时重置自身事件，早于消费模组订阅；本轮引用由 TOCleanup 清理。
    /// </remarks>
    public override void Load()
    {
        TOCleanup.Register(ClearReferences);
        OnBossChallengeBegin = null;
        OnBossChallengeEnd = null;
    }

    private static void ClearReferences()
    {
        _BossList.Clear();
        BossActive = false;
        BossTimer = 0;

        OnBossChallengeBegin = null;
        OnBossChallengeEnd = null;
    }
}
