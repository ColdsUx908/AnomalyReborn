namespace Transoceanic.Core.Loading;

/// <summary>
/// 登记当前模组加载周期的自有资源和静态引用清理；逆序执行，单项失败不阻止其余清理。
/// 不负责 tModLoader 管理的 On/IL 钩子；仅在加载与卸载主线程使用。
/// </summary>
public static class TOCleanup
{
    private static readonly List<Action> _Actions = [];
    private static bool _Cleaning;

    /// <summary>
    /// 在分配资源前登记清理，允许早于 Mod.Load 的内容自动加载阶段调用。每次登记在本轮卸载执行一次，清理期间禁止追加。
    /// </summary>
    /// <param name="cleanup">释放自有资源或清空引用的操作。</param>
    public static void Register(Action cleanup)
    {
        ArgumentNullException.ThrowIfNull(cleanup);
        if (_Cleaning)
            throw new InvalidOperationException("Cannot register cleanup while unloading.");
        _Actions.Add(cleanup);
    }

    internal static void Run()
    {
        if (_Cleaning)
            throw new InvalidOperationException("Recursive cleanup is forbidden.");
        _Cleaning = true;
        Action[] actions = _Actions.ToArray();
        _Actions.Clear();
        try { Execute(Enumerable.Reverse(actions)); }
        finally { _Cleaning = false; }
    }

    internal static void Execute(IEnumerable<Action> actions)
    {
        List<Exception> failures = [];
        foreach (Action action in actions)
        {
            try { action(); }
            catch (Exception exception) { failures.Add(exception); }
        }
        if (failures.Count > 0)
            throw new AggregateException("Transoceanic cleanup failed.", failures).Flatten();
    }
}
