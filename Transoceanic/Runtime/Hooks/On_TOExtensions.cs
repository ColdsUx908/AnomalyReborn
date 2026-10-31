#pragma warning disable IDE1006

namespace Transoceanic.Runtime.Hooks;

/// <summary>
/// 提供对 <see cref="TOTerrariaExtensions"/> 的动态扩展支持。
/// </summary>
public sealed class On_TOExtensions : ILoadable
{
    /// <summary>
    /// 在框架自动加载时清空自身事件，早于消费方订阅，不再需要后置优先级。
    /// </summary>
    /// <param name="mod">由 tML 提供的所属框架模组。</param>
    void ILoadable.Load(Mod mod)
    {
        TOCleanup.Register(() => TOHookHelper.ResetHandlerFields(typeof(On_TOExtensions)));
        TOHookHelper.ResetHandlerFields(typeof(On_TOExtensions));
    }

    // 自有事件由 TOCleanup 清理，此入口只满足 tML 生命周期契约。
    void ILoadable.Unload() { }

    #region Handler
    /// <summary>
    /// 内部存储 <see cref="get_IsBossEnemy"/> 事件的所有处理程序委托的列表。
    /// 该字段在框架自动加载时重置，随后保留消费方订阅，卸载时由 TOCleanup 清空。
    /// </summary>
    private static List<Func<NPC, bool>> _HandlerGetIsBossEnemy = [];
    #endregion Handler

    #region Event
    /// <summary>
    /// 当需要判断指定的 <see cref="NPC"/> 是否应被视为 Boss 时触发的事件。
    /// </summary>
    /// <remarks>
    /// <para>事件处理程序签名为 <c>Func&lt;NPC, bool&gt;</c>，接受一个 <see cref="NPC"/> 参数，返回一个 <see cref="bool"/> 值。</para>
    /// <para>返回 <see langword="true"/> 表示该 NPC 被视为 Boss；返回 <see langword="false"/> 表示不视为 Boss（此时会继续调用后续注册的处理程序）。</para>
    /// <para>多个处理程序按照注册顺序执行，第一个返回 <see langword="true"/> 的处理程序将决定最终结果。</para>
    /// </remarks>
    public static event Func<NPC, bool> get_IsBossEnemy
    {
        add => _HandlerGetIsBossEnemy.Add(value);
        remove => _HandlerGetIsBossEnemy.Remove(value);
    }
    #endregion Event

    #region Implementation
    /// <summary>
    /// 执行实际的 Boss 判定逻辑。
    /// </summary>
    /// <param name="npc">待判定的 <see cref="NPC"/> 实例，不能为 <see langword="null"/>。</param>
    /// <returns>
    /// 如果至少一个已注册的 <see cref="get_IsBossEnemy"/> 事件处理程序返回 <see langword="true"/>，则为 <see langword="true"/>；
    /// 否则为 <see langword="false"/>。
    /// </returns>
    internal static bool Impl_get_IsBossEnemy(NPC npc)
    {
        if (_HandlerGetIsBossEnemy.Count == 0)
            return false;

        foreach (Func<NPC, bool> handler in _HandlerGetIsBossEnemy)
        {
            if (handler(npc))
                return true;
        }

        return false;
    }
    #endregion Implementation
}
