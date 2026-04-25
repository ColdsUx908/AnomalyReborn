// Designed by ColdsUx

#pragma warning disable IDE1006 //命名样式

namespace Transoceanic.Hooks.Framework.Helpers;

/// <summary>
/// 提供对 <see cref=" TOExtensions"/> 的动态扩展支持。
/// </summary>
public sealed class On_TOExtensions : IContentLoader
{
    /// <summary>
    /// 内部存储 <see cref="get_IsBossEnemy"/> 事件的所有处理程序委托的列表。
    /// 该字段在内容加载完成和模组卸载时会被自动重置为空列表。
    /// </summary>
    private static List<Func<NPC, bool>> _handler_get_IsBossEnemy = [];

    /// <summary>
    /// 当需要判断指定的 <see cref="NPC"/> 是否应被视为 Boss 时触发的事件。
    /// </summary>
    /// <remarks>
    /// <para>事件处理程序签名为 <c>Func&lt;NPC, bool&gt;</c>，接受一个 <see cref="NPC"/> 参数，返回一个 <see cref="bool"/> 值。</para>
    /// <para>返回 <see langword="true"/> 表示该 NPC 被视为 Boss；返回 <see langword="false"/> 表示不视为 Boss（此时会继续调用后续注册的处理程序）。</para>
    /// <para>多个处理程序按照注册顺序执行，第一个返回 <see langword="true"/> 的处理程序将决定最终结果。</para>
    /// <para>注意：事件的添加和移除直接操作内部列表，并非原生事件委托链，因此不支持 <see langword="null"/> 条件调用。</para>
    /// </remarks>
    public static event Func<NPC, bool> get_IsBossEnemy
    {
        add => _handler_get_IsBossEnemy.Add(value);
        remove => _handler_get_IsBossEnemy.Remove(value);
    }

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
        if (_handler_get_IsBossEnemy.Count == 0)
            return false;

        foreach (Func<NPC, bool> handler in _handler_get_IsBossEnemy)
        {
            if (handler(npc))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 通过反射清空当前类中所有以 <c>_handler_</c> 开头的静态字段，
    /// 将它们重置为对应泛型列表类型的新实例。
    /// </summary>
    /// <remarks>
    /// <para>该方法会查找所有静态字段（依据 <c>TOReflectionUtils.StaticBindingFlags</c>），
    /// 对名称前缀为 <c>_handler_</c> 且类型为 <see cref="List{T}"/> 的字段，
    /// 创建新的空列表实例并赋值，从而移除所有已注册的事件处理程序。</para>
    /// <para>若字段类型是某种具体的泛型列表（如 <c>List&lt;Func&lt;NPC, bool&gt;&gt;</c>），直接创建其实例；
    /// 若字段类型是泛型定义等非具体类型，则通过反射构造对应的 <see cref="List{T}"/> 实例。</para>
    /// <para>此方法在 <see cref="PostSetupContent"/> 和 <see cref="OnModUnload"/> 中被调用，
    /// 以确保每次模组加载后事件处理程序列表处于干净状态。</para>
    /// </remarks>
    private static void ResetHandlerFields()
    {
        foreach (FieldInfo field in typeof(On_TOExtensions).GetFields(TOReflectionUtils.StaticBindingFlags))
        {
            if (field.Name.StartsWith("_handler_"))
            {
                Type fieldType = field.FieldType;
                if (fieldType.IsGenericType)
                {
                    Type genericDef = fieldType.GetGenericTypeDefinition();
                    Type concreteType;
                    if (genericDef == typeof(List<>))
                        concreteType = fieldType;
                    else
                    {
                        Type elementType = fieldType.GetGenericArguments()[0];
                        concreteType = typeof(List<>).MakeGenericType(elementType);
                    }
                    field.SetValue(null, Activator.CreateInstance(concreteType));
                }
            }
        }
    }

    /// <summary>
    /// <see cref="IContentLoader.PostSetupContent"/> 的显式实现。
    /// </summary>
    /// <remarks>
    /// 应用了 <see cref="LoadPriorityAttribute"/> 特性，优先级设为 100000，确保在较前阶段执行。
    /// 方法内部直接调用 <see cref="ResetHandlerFields"/> 清空所有事件处理程序列表。
    /// </remarks>
    [LoadPriority(1e5)]
    void IContentLoader.PostSetupContent() => ResetHandlerFields();

    /// <summary>
    /// <see cref="IContentLoader.OnModUnload"/> 的显式实现。
    /// </summary>
    /// <remarks>
    /// 在模组卸载时调用 <see cref="ResetHandlerFields"/>，移除所有事件订阅，防止内存泄漏或状态残留。
    /// </remarks>
    void IContentLoader.OnModUnload() => ResetHandlerFields();
}