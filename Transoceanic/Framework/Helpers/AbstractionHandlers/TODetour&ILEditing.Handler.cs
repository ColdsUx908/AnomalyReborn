// Developed by ColdsUx

using System.Collections;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Transoceanic.Framework.RuntimeEditing;

/// <summary>
/// 处理 Detour（方法钩子）的全局管理器，支持通过特性或命名约定自动应用 Detour。
/// </summary>
/// <remarks>
/// <para>该类作为 <see cref="IContentLoader"/> 实现，在模组加载后自动扫描并应用带有相关特性的 Detour 方法，
/// 同时提供了一系列用于手动创建和管理 Detour 钩子的静态方法。</para>
/// <para>Detour 机制基于 MonoMod.RuntimeDetour，允许在运行时替换目标方法的实现，
/// 常用于模组开发中对原版代码的非侵入式修改。</para>
/// <para>所有通过本管理器创建的钩子均会被记录在内部的 <see cref="Detours"/> 集合中，
/// 并在模组卸载时自动清理。</para>
/// </remarks>
public sealed partial class TODetourHandler : IContentLoader
{
    /// <summary>
    /// 管理一组 <see cref="Hook"/> 的集合，支持按目标类型和方法检索。
    /// </summary>
    /// <remarks>
    /// <para>该集合采用两层字典结构进行索引：外层字典的键为目标方法所属的类型 <see cref="Type"/>，
    /// 内层字典的键为目标方法 <see cref="MethodBase"/>，值为该目标方法上所有已应用的 <see cref="Hook"/> 列表。</para>
    /// <para>所有添加、移除操作都会同步执行钩子的 <see cref="Hook.Undo"/> 操作，确保钩子状态的正确性。</para>
    /// <para>该类实现了 <see cref="IEnumerable{Hook}"/>，可遍历集合中所有的钩子。</para>
    /// </remarks>
    public sealed class DetourSet : IEnumerable<Hook>
    {
        private readonly Dictionary<Type, Dictionary<MethodBase, List<Hook>>> _data = [];

        /// <summary>
        /// 向集合中添加一个钩子。
        /// </summary>
        /// <param name="hook">要添加的 <see cref="Hook"/> 实例，不能为 <c>null</c>。</param>
        /// <exception cref="ArgumentNullException"><paramref name="hook"/> 为 <c>null</c>。</exception>
        public void Add(Hook hook)
        {
            ArgumentNullException.ThrowIfNull(hook);
            Type targetType = hook.Source.DeclaringType;
            if (!_data.ContainsKey(targetType))
                _data[targetType] = [];
            if (_data[targetType].TryGetValue(hook.Source, out List<Hook> value))
                value.Add(hook);
            else
                _data[targetType][hook.Source] = [hook];
        }

        /// <summary>
        /// 从集合中移除指定的钩子并执行 <see cref="Hook.Undo"/>。
        /// </summary>
        /// <param name="hook">要移除的 <see cref="Hook"/> 实例。</param>
        /// <returns>
        /// 如果成功找到并移除钩子，则返回 <see langword="true"/>；
        /// 如果钩子不在集合中，则返回 <see langword="false"/>。
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="hook"/> 为 <c>null</c>。</exception>
        public bool Remove(Hook hook)
        {
            ArgumentNullException.ThrowIfNull(hook);
            Type targetType = hook.Source.DeclaringType;
            if (!_data.TryGetValue(targetType, out Dictionary<MethodBase, List<Hook>> methodHooks))
                return false;
            foreach ((MethodBase source, List<Hook> hooks) in methodHooks)
            {
                int index = hooks.IndexOf(hook);
                if (index >= 0)
                {
                    hooks[index].Undo();
                    hooks.RemoveAt(index);
                    if (hooks.Count == 0)
                        methodHooks.Remove(source);
                    if (methodHooks.Count == 0)
                        _data.Remove(targetType);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 移除所有匹配条件的钩子并执行撤销。
        /// </summary>
        /// <param name="match">用于筛选钩子的条件委托，不能为 <c>null</c>。</param>
        /// <exception cref="ArgumentNullException"><paramref name="match"/> 为 <c>null</c>。</exception>
        public void RemoveAll(Func<Hook, bool> match)
        {
            ArgumentNullException.ThrowIfNull(match);
            foreach ((Type sourceType, Dictionary<MethodBase, List<Hook>> methodHooks) in _data)
            {
                foreach ((MethodBase source, List<Hook> hooks) in methodHooks)
                {
                    foreach (Hook hook in hooks)
                    {
                        if (match(hook))
                        {
                            hook.Undo();
                            hooks.Remove(hook);
                        }
                    }
                    if (hooks.Count == 0)
                        methodHooks.Remove(source);
                    if (methodHooks.Count == 0)
                        _data.Remove(sourceType);
                }
            }
        }

        /// <summary>
        /// 清除所有钩子并执行撤销。
        /// </summary>
        /// <remarks>
        /// 该方法会遍历集合中所有钩子，依次调用 <see cref="Hook.Undo"/>，然后清空内部字典。
        /// </remarks>
        public void Clear()
        {
            foreach (Dictionary<MethodBase, List<Hook>> methodHooks in _data.Values)
            {
                foreach (List<Hook> hooks in methodHooks.Values)
                {
                    foreach (Hook hook in hooks)
                        hook.Undo();
                    hooks.Clear();
                }
                methodHooks.Clear();
            }
            _data.Clear();
        }

        /// <summary>
        /// 尝试获取针对特定方法的所有钩子。
        /// </summary>
        /// <param name="targetMethod">要查询的目标方法。</param>
        /// <param name="hooks">
        /// 当此方法返回时，如果找到了钩子，则包含对应的 <see cref="List{Hook}"/>；
        /// 否则为 <c>null</c>。此参数以未经初始化的形式传递。
        /// </param>
        /// <returns>如果找到至少一个钩子，则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="targetMethod"/> 为 <c>null</c>。</exception>
        public bool TryGetHooks(MethodBase targetMethod, out List<Hook> hooks)
        {
            ArgumentNullException.ThrowIfNull(targetMethod);
            foreach (Dictionary<MethodBase, List<Hook>> methodHooks in _data.Values)
            {
                if (methodHooks.TryGetValue(targetMethod, out hooks) && hooks.Count > 0)
                    return true;
            }
            hooks = null;
            return false;
        }

        /// <summary>
        /// 返回一个循环访问集合中所有钩子的枚举器。
        /// </summary>
        /// <returns>可用于循环访问集合的 <see cref="IEnumerator{Hook}"/>。</returns>
        public IEnumerator<Hook> GetEnumerator()
        {
            foreach (Dictionary<MethodBase, List<Hook>> methodHooks in _data.Values)
            {
                foreach (List<Hook> hooks in methodHooks.Values)
                {
                    foreach (Hook hook in hooks)
                        yield return hook;
                }
            }
        }

        /// <summary>
        /// 返回一个循环访问集合的非泛型枚举器。
        /// </summary>
        /// <returns>可用于循环访问集合的 <see cref="IEnumerator"/>。</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// 内部维护的全局 Detour 钩子集合。
    /// </summary>
    /// <remarks>
    /// 所有通过本管理器手动或自动创建的钩子均会被添加至此集合，
    /// 确保在模组卸载时能够统一清理。
    /// </remarks>
    internal static readonly DetourSet Detours = [];

    /// <summary>
    /// 在模组内容加载完成后自动扫描并应用所有符合条件的 Detour 方法。
    /// </summary>
    /// <remarks>
    /// <para>扫描过程按以下顺序进行：</para>
    /// <list type="number">
    /// <item><description>清空现有钩子集合。</description></item>
    /// <item><description>扫描所有标记了 <c>DetourClassToAttribute</c> 的类型，并应用其中的静态 Detour 方法。</description></item>
    /// <item><description>扫描所有标记了 <c>DetourClassTo_MultiSourceAttribute</c> 的类型，并应用其中的静态 Detour 方法。</description></item>
    /// <item><description>扫描所有标记了 <c>DetourMethodToAttribute</c> 的方法，并直接应用。</description></item>
    /// <item><description>扫描所有实现了 <see cref="ITODetourProvider"/> 接口的类型，按优先级降序调用其 <see cref="ITODetourProvider.ApplyDetour"/> 方法。</description></item>
    /// </list>
    /// </remarks>
    void IContentLoader.PostSetupContent()
    {
        Detours.Clear();

        foreach ((Type type, DetourClassToAttribute attribute) in TOReflectionUtils.GetTypesWithAttribute<DetourClassToAttribute>())
            ApplyAllStaticMethodDetoursOfType(type, attribute.SourceType);

        foreach ((Type type, DetourClassTo_MultiSourceAttribute attribute) in TOReflectionUtils.GetTypesWithAttribute<DetourClassTo_MultiSourceAttribute>())
        {
            Type[] sourceTypes = attribute.SourceTypes;
            foreach (MethodInfo detour in type.GetRealMethods(TOReflectionUtils.StaticBindingFlags))
                ApplyTypedStaticMethodDetour(detour, sourceTypes);
        }

        foreach ((MethodInfo detour, DetourMethodToAttribute attribute) in TOReflectionUtils.GetMethodsWithAttribute<DetourMethodToAttribute>())
            ApplyStaticMethodDetour(detour, attribute.SourceType, attribute.ParamOffset < 0 ? null : attribute.ParamOffset);

        foreach (ITODetourProvider detourProvider in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITODetourProvider>().OrderByDescending(d => d.LoadPriority))
            detourProvider.ApplyDetour();
    }

    /// <summary>
    /// <see cref="IContentLoader.OnModUnload"/> 的实现。
    /// 在模组卸载时清理所有已应用的 Detour 钩子。
    /// </summary>
    void IContentLoader.OnModUnload() => Detours.Clear();

    private const string DefaultPrefix = "Detour_";
    [StringSyntax(StringSyntaxAttribute.Regex)] private const string Pattern = """^{0}(?<methodName>[\S]*?)(?:__[\S]*)?$""";
    [StringSyntax(StringSyntaxAttribute.Regex)] private const string Pattern2 = """^{0}(?<typeName>[\S]*?)__(?<methodName>[\S]*?)(?:__[\S]*)?$""";

    /// <summary>
    /// 默认 Detour 方法名正则表达式。
    /// </summary>
    /// <remarks>
    /// 匹配模式：<c>Detour_{methodName}[__{paramName}]</c>。
    /// </remarks>
    private static readonly Regex _defaultDetourNameRegex = GetDefaultDetourNameRegex();
    [GeneratedRegex("""^Detour_(?<methodName>[\S]*?)(?:__[\S]*)?$""")]
    private static partial Regex GetDefaultDetourNameRegex();

    /// <summary>
    /// 默认 Detour 方法名（含类型）正则表达式。
    /// </summary>
    /// <remarks>
    /// 匹配模式：<c>Detour_{typeName}__{methodName}[__{paramName}]</c>。
    /// </remarks>
    private static readonly Regex _defaultDetourNameRegex2 = GetDefaultDetourNameRegex2();
    [GeneratedRegex("""^Detour_(?<typeName>[\S]*?)__(?<methodName>[\S]*?)(?:__[\S]*)?$""")]
    private static partial Regex GetDefaultDetourNameRegex2();

    /// <summary>
    /// 尝试解析提供的 Detour 方法名，获取其应用的源方法名。
    /// </summary>
    /// <param name="detour">Detour 方法，必须是具名方法。</param>
    /// <param name="sourceName">当解析成功时，输出源方法名称；否则为 <c>null</c>。</param>
    /// <returns>解析是否成功。</returns>
    /// <remarks>
    /// <para>解析逻辑：</para>
    /// <list type="number">
    /// <item><description>尝试获取 <paramref name="detour"/> 的 <see cref="CustomDetourPrefixAttribute"/> 特性，若存在则使用其指定的前缀，否则使用 <see cref="DefaultPrefix"/>。</description></item>
    /// <item><description>尝试将方法名与模式 <c>{prefix}{methodName}[__{paramName}]</c> 进行匹配。</description></item>
    /// <item><description>如果匹配成功，输出 <c>methodName</c> 捕获组的值。</description></item>
    /// </list>
    /// </remarks>
    public static bool EvaluateDetourName(MethodInfo detour, [NotNullWhen(true)] out string sourceName)
    {
        string prefix = detour.Attribute<CustomDetourPrefixAttribute>()?.Prefix;
        Match match = string.IsNullOrEmpty(prefix) ? _defaultDetourNameRegex.Match(detour.Name) : Regex.Match(detour.Name, string.Format(Pattern, prefix));
        if (match.Success)
        {
            sourceName = match.Groups["methodName"].Value;
            return true;
        }
        sourceName = null;
        return false;
    }

    /// <summary>
    /// 尝试解析提供的 Detour 方法名，获取其应用的源类型名和源方法名。
    /// </summary>
    /// <param name="detour">Detour 方法，必须是具名方法。</param>
    /// <param name="sourceTypeName">当解析成功时，输出源类型名称；否则为 <c>null</c>。</param>
    /// <param name="sourceMethodName">当解析成功时，输出源方法名称；否则为 <c>null</c>。</param>
    /// <returns>解析是否成功。</returns>
    /// <remarks>
    /// <para>解析逻辑：</para>
    /// <list type="number">
    /// <item><description>尝试获取 <paramref name="detour"/> 的 <see cref="CustomDetourPrefixAttribute"/> 特性，若存在则使用其指定的前缀，否则使用 <see cref="DefaultPrefix"/>。</description></item>
    /// <item><description>尝试将方法名与模式 <c>{prefix}{typeName}__{methodName}[__{paramName}]</c> 进行匹配。</description></item>
    /// <item><description>如果匹配成功，输出 <c>typeName</c> 和 <c>methodName</c> 捕获组的值。</description></item>
    /// </list>
    /// </remarks>
    public static bool EvaluateTypedDetourName(MethodInfo detour, [NotNullWhen(true)] out string sourceTypeName, [NotNullWhen(true)] out string sourceMethodName)
    {
        string prefix = detour.Attribute<CustomDetourPrefixAttribute>()?.Prefix ?? DefaultPrefix;
        Match match = string.IsNullOrEmpty(prefix) ? _defaultDetourNameRegex2.Match(detour.Name) : Regex.Match(detour.Name, string.Format(Pattern2, prefix));
        if (match.Success)
        {
            sourceTypeName = match.Groups["typeName"].Value;
            sourceMethodName = match.Groups["methodName"].Value;
            return true;
        }
        sourceTypeName = null;
        sourceMethodName = null;
        return false;
    }

    /// <summary>
    /// 尝试将 Detour 应用到指定源方法上，并返回创建的钩子。
    /// </summary>
    /// <param name="source">目标源方法。</param>
    /// <param name="detour">Detour 方法。</param>
    /// <returns>创建的 <see cref="Hook"/> 对象，已自动添加到 <see cref="Detours"/> 集合中。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> 或 <paramref name="detour"/> 为 <c>null</c>。</exception>
    /// <remarks>
    /// 如果 <paramref name="detour"/> 方法上标记了 <see cref="CustomDetourConfigAttribute"/>，
    /// 则使用特性中指定的配置创建钩子。
    /// </remarks>
    public static Hook Modify(MethodBase source, MethodInfo detour)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(detour);
        DetourConfig detourConfig = detour.Attribute<CustomDetourConfigAttribute>()?.DetourConfig;
        Hook hook = detourConfig is not null ? new(source, detour, detourConfig, true) : new(source, detour, true);
        Detours.Add(hook);
        return hook;
    }

    /// <summary>
    /// 尝试将 Detour 应用到指定源方法上，并返回创建的钩子。
    /// </summary>
    /// <typeparam name="TDelegate">委托类型，必须继承自 <see cref="Delegate"/>。</typeparam>
    /// <param name="source">目标源方法。</param>
    /// <param name="detour">表示 Detour 方法的委托实例。</param>
    /// <returns>创建的 <see cref="Hook"/> 对象，已自动添加到 <see cref="Detours"/> 集合中。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> 或 <paramref name="detour"/> 为 <c>null</c>。</exception>
    /// <remarks>
    /// 如果委托对应的方法上标记了 <see cref="CustomDetourConfigAttribute"/>，
    /// 则使用特性中指定的配置创建钩子。
    /// </remarks>
    public static Hook Modify<TDelegate>(MethodBase source, TDelegate detour) where TDelegate : Delegate
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(detour);
        DetourConfig detourConfig = detour.Method.Attribute<CustomDetourConfigAttribute>()?.DetourConfig;
        Hook hook = detourConfig is not null ? new(source, detour, detourConfig, true) : new(source, detour, true);
        Detours.Add(hook);
        return hook;
    }

    /// <summary>
    /// 尝试通过委托将 Detour 应用到指定源方法上。
    /// </summary>
    /// <typeparam name="TSource">源方法的委托类型。</typeparam>
    /// <typeparam name="TTarget">Detour 方法的委托类型。</typeparam>
    /// <param name="source">表示源方法的委托实例。</param>
    /// <param name="detour">表示 Detour 方法的委托实例。</param>
    /// <returns>创建的 <see cref="Hook"/> 对象。</returns>
    /// <inheritdoc cref="Modify{TDelegate}(MethodBase, TDelegate)"/>
    public static Hook Modify<TSource, TTarget>(TSource source, TTarget detour) where TSource : Delegate where TTarget : Delegate => Modify(source.Method, detour);

    /// <summary>
    /// 在指定类型中按名称查找方法，并将 Detour 应用到该方法上。
    /// </summary>
    /// <param name="sourceType">源方法所在类型。</param>
    /// <param name="sourceMethodName">源方法名称。</param>
    /// <param name="detour">Detour 方法。</param>
    /// <returns>创建的 <see cref="Hook"/> 对象，若未找到匹配的方法则可能抛出异常。</returns>
    /// <exception cref="ArgumentNullException">任一参数为 <c>null</c>。</exception>
    /// <exception cref="AmbiguousMatchException">找到多个同名方法且无法确定唯一匹配。</exception>
    public static Hook Modify(Type sourceType, string sourceMethodName, MethodInfo detour) => Modify(sourceType.GetMethod(sourceMethodName, TOReflectionUtils.UniversalBindingFlags), detour);

    /// <summary>
    /// 在指定类型中按名称查找方法，并将 Detour 应用到该方法上。
    /// </summary>
    /// <typeparam name="TDelegate">委托类型，必须继承自 <see cref="Delegate"/>。</typeparam>
    /// <param name="sourceType">源方法所在类型。</param>
    /// <param name="sourceMethodName">源方法名称。</param>
    /// <param name="detour">表示 Detour 方法的委托实例。</param>
    /// <returns>创建的 <see cref="Hook"/> 对象。</returns>
    /// <inheritdoc cref="Modify(Type, string, MethodInfo)"/>
    public static Hook Modify<TDelegate>(Type sourceType, string sourceMethodName, TDelegate detour) where TDelegate : Delegate => Modify(sourceType.GetMethod(sourceMethodName, TOReflectionUtils.UniversalBindingFlags), detour);

    /// <summary>
    /// 在指定类型中按名称查找方法，并将 Detour 应用到该方法上。
    /// </summary>
    /// <typeparam name="T">源方法所在类型。</typeparam>
    /// <param name="sourceMethodName">源方法名称。</param>
    /// <param name="detour">Detour 方法。</param>
    /// <returns>创建的 <see cref="Hook"/> 对象。</returns>
    /// <inheritdoc cref="Modify(Type, string, MethodInfo)"/>
    public static Hook Modify<T>(string sourceMethodName, MethodInfo detour) => Modify(typeof(T), sourceMethodName, detour);

    /// <summary>
    /// 在指定类型中按名称和参数类型查找方法，考虑 Detour 方法的参数偏移量，并应用 Detour。
    /// </summary>
    /// <param name="sourceType">源方法所在类型。</param>
    /// <param name="sourceMethodName">源方法名称。</param>
    /// <param name="paramOffset">
    /// <para>Detour 方法参数偏移量。</para>
    /// <para>应设置为目标方法第一个参数在 Detour 方法中的索引。</para>
    /// <para>例如，若 Detour 方法有 <c>orig</c> 和 <c>self</c> 参数，则应设置为 <c>2</c>；若都没有，则应设置为 <c>0</c>。</para>
    /// <para>只应设置为 <c>0</c>、<c>1</c> 或 <c>2</c>。</para>
    /// </param>
    /// <param name="detour">Detour 方法。</param>
    /// <returns>创建的 <see cref="Hook"/> 对象。</returns>
    /// <exception cref="ArgumentNullException">任一参数为 <c>null</c>。</exception>
    /// <exception cref="IndexOutOfRangeException"><paramref name="paramOffset"/> 超出 Detour 方法的参数索引范围。</exception>
    public static Hook Modify(Type sourceType, string sourceMethodName, int paramOffset, MethodInfo detour) => Modify(sourceType.GetMethod(sourceMethodName, TOReflectionUtils.UniversalBindingFlags, detour.ParameterTypes[paramOffset..]), detour);

    /// <summary>
    /// 在指定类型中按名称和参数类型查找方法，考虑 Detour 方法的参数偏移量，并应用 Detour。
    /// </summary>
    /// <typeparam name="TDelegate">委托类型，必须继承自 <see cref="Delegate"/>。</typeparam>
    /// <param name="sourceType">源方法所在类型。</param>
    /// <param name="sourceMethodName">源方法名称。</param>
    /// <param name="paramOffset">
    /// <para>Detour 方法参数偏移量。</para>
    /// <para>应设置为目标方法第一个参数在 Detour 方法中的索引。</para>
    /// <para>例如，若 Detour 方法有 <c>orig</c> 和 <c>self</c> 参数，则应设置为 <c>2</c>；若都没有，则应设置为 <c>0</c>。</para>
    /// <para>只应设置为 <c>0</c>、<c>1</c> 或 <c>2</c>。</para>
    /// </param>
    /// <param name="detour">表示 Detour 方法的委托实例。</param>
    /// <returns>创建的 <see cref="Hook"/> 对象。</returns>
    /// <inheritdoc cref="Modify(Type, string, int, MethodInfo)"/>
    public static Hook Modify<TDelegate>(Type sourceType, string sourceMethodName, int paramOffset, TDelegate detour) where TDelegate : Delegate => Modify(sourceType.GetMethod(sourceMethodName, TOReflectionUtils.UniversalBindingFlags, detour.Method.ParameterTypes[paramOffset..]), detour);

    /// <summary>
    /// 在指定类型中按名称和参数类型查找方法，考虑 Detour 方法的参数偏移量，并应用 Detour。
    /// </summary>
    /// <typeparam name="T">源方法所在类型。</typeparam>
    /// <param name="sourceMethodName">源方法名称。</param>
    /// <param name="paramOffset">
    /// <para>Detour 方法参数偏移量。</para>
    /// <para>应设置为目标方法第一个参数在 Detour 方法中的索引。</para>
    /// <para>例如，若 Detour 方法有 <c>orig</c> 和 <c>self</c> 参数，则应设置为 <c>2</c>；若都没有，则应设置为 <c>0</c>。</para>
    /// <para>只应设置为 <c>0</c>、<c>1</c> 或 <c>2</c>。</para>
    /// </param>
    /// <param name="detour">Detour 方法。</param>
    /// <returns>创建的 <see cref="Hook"/> 对象。</returns>
    /// <inheritdoc cref="Modify(Type, string, int, MethodInfo)"/>
    public static Hook Modify<T>(string sourceMethodName, int paramOffset, MethodInfo detour) => Modify(typeof(T), sourceMethodName, paramOffset, detour);

    /// <summary>
    /// 在指定类型中按名称查找方法，考虑目标方法是否为实例方法，并应用 Detour。
    /// </summary>
    /// <param name="sourceType">源方法所在类型。</param>
    /// <param name="sourceMethodName">源方法名称。</param>
    /// <param name="hasThis">
    /// <para>目标方法是否为实例方法（有 <see langword="this"/> 指针）。</para>
    /// <para>会影响获取方法时的参数偏移量（<see langword="true"/> 为 2，反之为 1）和 <c>bindingAttr</c> 实参。</para>
    /// </param>
    /// <param name="detour">Detour 方法。</param>
    /// <returns>创建的 <see cref="Hook"/> 对象。</returns>
    /// <exception cref="ArgumentNullException">任一参数为 <c>null</c>。</exception>
    public static Hook Modify(Type sourceType, string sourceMethodName, bool hasThis, MethodInfo detour) =>
        Modify(hasThis ? sourceType.GetMethod(sourceMethodName, TOReflectionUtils.InstanceBindingFlags, detour.ParameterTypes[2..])
            : sourceType.GetMethod(sourceMethodName, TOReflectionUtils.StaticBindingFlags, detour.ParameterTypes[1..]), detour);

    /// <summary>
    /// 在指定类型中按名称查找方法，考虑目标方法是否为实例方法，并应用 Detour。
    /// </summary>
    /// <typeparam name="TDelegate">委托类型，必须继承自 <see cref="Delegate"/>。</typeparam>
    /// <param name="sourceType">源方法所在类型。</param>
    /// <param name="sourceMethodName">源方法名称。</param>
    /// <param name="hasThis">
    /// <para>目标方法是否为实例方法（有 <see langword="this"/> 指针）。</para>
    /// <para>会影响获取方法时的参数偏移量（<see langword="true"/> 为 2，反之为 1）和 <c>bindingAttr</c> 实参。</para>
    /// </param>
    /// <param name="detour">表示 Detour 方法的委托实例。</param>
    /// <returns>创建的 <see cref="Hook"/> 对象。</returns>
    /// <inheritdoc cref="Modify(Type, string, bool, MethodInfo)"/>
    public static Hook Modify<TDelegate>(Type sourceType, string sourceMethodName, bool hasThis, TDelegate detour) where TDelegate : Delegate =>
        Modify(hasThis ? sourceType.GetMethod(sourceMethodName, TOReflectionUtils.InstanceBindingFlags, detour.Method.ParameterTypes[2..])
            : sourceType.GetMethod(sourceMethodName, TOReflectionUtils.StaticBindingFlags, detour.Method.ParameterTypes[1..]), detour);

    /// <summary>
    /// 在指定类型中按名称查找方法，考虑目标方法是否为实例方法，并应用 Detour。
    /// </summary>
    /// <typeparam name="T">源方法所在类型。</typeparam>
    /// <param name="sourceMethodName">源方法名称。</param>
    /// <param name="hasThis">
    /// <para>目标方法是否为实例方法（有 <see langword="this"/> 指针）。</para>
    /// <para>会影响获取方法时的参数偏移量（<see langword="true"/> 为 2，反之为 1）和 <c>bindingAttr</c> 实参。</para>
    /// </param>
    /// <param name="detour">Detour 方法。</param>
    /// <returns>创建的 <see cref="Hook"/> 对象。</returns>
    /// <inheritdoc cref="Modify(Type, string, bool, MethodInfo)"/>
    public static Hook Modify<T>(string sourceMethodName, bool hasThis, MethodInfo detour) => Modify(typeof(T), sourceMethodName, hasThis, detour);

    /// <summary>
    /// 尝试将一个静态 Detour 方法应用到其对应的源方法上。
    /// </summary>
    /// <param name="detour">要应用的 Detour 方法。</param>
    /// <param name="sourceType">默认的源类型，当通过命名约定解析失败时使用。</param>
    /// <param name="paramOffset">可选的参数偏移量，用于解析重载方法。</param>
    /// <returns>如果成功应用则返回创建的 <see cref="Hook"/> 对象；否则返回 <c>null</c>。</returns>
    /// <remarks>
    /// <para>该方法遵循以下解析顺序：</para>
    /// <list type="number">
    /// <item><description>若方法标记了 <see cref="NotDetourMethodAttribute"/>，则跳过。</description></item>
    /// <item><description>若方法标记了 <see cref="CustomDetourSourceAttribute"/>，则直接使用特性指定的源方法。</description></item>
    /// <item><description>尝试通过 <see cref="EvaluateDetourName(MethodInfo, out string)"/> 解析方法名，若成功则在 <paramref name="sourceType"/> 中查找匹配的方法。</description></item>
    /// </list>
    /// </remarks>
    public static Hook ApplyStaticMethodDetour(MethodInfo detour, Type sourceType, int? paramOffset = null)
    {
        if (detour.HasAttribute<NotDetourMethodAttribute>())
            return null;
        if (detour.TryGetAttribute(out CustomDetourSourceAttribute attribute))
            return Modify(attribute.Source, detour);
        if (EvaluateDetourName(detour, out string sourceName))
            return paramOffset is null ? Modify(sourceType, sourceName, detour)
                : Modify(sourceType, sourceName, paramOffset.Value, detour);
        return null;
    }

    /// <summary>
    /// 尝试将一个静态 Detour 方法应用到其对应的源方法上，支持在多个源类型中解析。
    /// </summary>
    /// <param name="detour">要应用的 Detour 方法。</param>
    /// <param name="sourceTypes">候选源类型数组。</param>
    /// <returns>如果成功应用则返回创建的 <see cref="Hook"/> 对象；否则返回 <c>null</c>。</returns>
    /// <remarks>
    /// <para>解析顺序：</para>
    /// <list type="number">
    /// <item><description>若方法标记了 <see cref="NotDetourMethodAttribute"/>，则跳过。</description></item>
    /// <item><description>若方法标记了 <see cref="CustomDetourSourceAttribute"/>，则直接使用特性指定的源方法。</description></item>
    /// <item><description>尝试通过 <see cref="EvaluateTypedDetourName(MethodInfo, out string, out string)"/> 解析出类型名和方法名，然后在 <paramref name="sourceTypes"/> 中查找名称匹配的类型，进而获取对应方法。</description></item>
    /// </list>
    /// </remarks>
    public static Hook ApplyTypedStaticMethodDetour(MethodInfo detour, Type[] sourceTypes)
    {
        if (detour.HasAttribute<NotDetourMethodAttribute>())
            return null;
        if (detour.TryGetAttribute(out CustomDetourSourceAttribute attribute))
            return Modify(attribute.Source, detour);
        if (EvaluateTypedDetourName(detour, out string sourceTypeName, out string sourceMethodName))
        {
            Type sourceType = sourceTypes.FirstOrDefault(t => t.Name == sourceTypeName);
            if (sourceType is not null)
                return Modify(sourceType.GetMethod(sourceMethodName, TOReflectionUtils.UniversalBindingFlags), detour);
        }
        return null;
    }

    /// <summary>
    /// 应用指定类型中的所有静态 Detour 方法到给定的源类型上。
    /// </summary>
    /// <param name="type">包含 Detour 方法的类型。</param>
    /// <param name="sourceType">默认的源类型。</param>
    /// <remarks>
    /// 该方法会遍历 <paramref name="type"/> 中所有公共/非公共静态方法，
    /// 并对每个方法调用 <see cref="ApplyStaticMethodDetour(MethodInfo, Type, int?)"/>。
    /// </remarks>
    public static void ApplyAllStaticMethodDetoursOfType(Type type, Type sourceType)
    {
        foreach (MethodInfo detour in type.GetRealMethods(TOReflectionUtils.StaticBindingFlags))
            ApplyStaticMethodDetour(detour, sourceType);
    }
}

/// <summary>
/// 处理 IL 编辑的全局管理器，支持通过特性自动应用 IL 钩子。
/// </summary>
/// <remarks>
/// <para>该类作为 <see cref="IContentLoader"/> 实现，在模组加载后自动扫描并应用带有相关特性的 IL 编辑方法，
/// 同时提供了一系列用于手动创建和管理 IL 钩子的静态方法。</para>
/// <para>IL 编辑基于 MonoMod，允许直接修改方法的中间语言指令，是实现更细粒度行为修改的常用手段。</para>
/// <para>所有通过本管理器创建的 IL 钩子均会被记录在内部的 <see cref="Manipulators"/> 列表中，
/// 并在模组卸载时自动清理。</para>
/// </remarks>
public sealed partial class TOILEditingHandler : IContentLoader
{
    /// <summary>
    /// 内部维护的全局 IL 钩子列表。
    /// </summary>
    /// <remarks>
    /// 所有通过本管理器手动或自动创建的 IL 钩子均会被添加至此列表，
    /// 确保在模组卸载时能够统一清理。
    /// </remarks>
    internal static readonly List<ILHook> Manipulators = [];

    /// <summary>
    /// 在模组内容加载完成后自动扫描并应用所有符合条件的 IL 编辑方法。
    /// </summary>
    /// <remarks>
    /// <para>扫描过程：清空现有钩子列表后，扫描所有标记了 <c>ILEditingMethodToAttribute</c> 的方法，
    /// 解析其目标源方法并应用 IL 钩子。</para>
    /// </remarks>
    void IContentLoader.PostSetupContent()
    {
        Manipulators.Clear();

        foreach ((MethodInfo manipulator, ILEditingMethodToAttribute attribute) in TOReflectionUtils.GetMethodsWithAttribute<ILEditingMethodToAttribute>())
        {
            Type sourceType = attribute.SourceType;
            if (EvaluateManipulatorName(manipulator, out string sourceName))
                Modify(sourceType, sourceName, manipulator);
        }
    }

    /// <summary>
    /// <see cref="IContentLoader.OnModUnload"/> 的实现。
    /// 在模组卸载时清理所有已应用的 IL 钩子。
    /// </summary>
    void IContentLoader.OnModUnload() => Manipulators.Clear();

    [StringSyntax(StringSyntaxAttribute.Regex)] private const string Pattern = """^{0}(?<methodName>[\S]*?)(?:__[\S]*)?$""";

    /// <summary>
    /// 默认 IL 编辑方法名正则表达式。
    /// </summary>
    /// <remarks>
    /// 匹配模式：<c>IL_{methodName}[__{paramName}]</c>。
    /// </remarks>
    private static readonly Regex _defaultManipulatorNameRegex = GetDefaultManipulatorNameRegex();
    [GeneratedRegex("""^IL_(?<methodName>[\S]*)$""")]
    private static partial Regex GetDefaultManipulatorNameRegex();

    /// <summary>
    /// 尝试解析 IL 编辑方法名，获取其应用的源方法名。
    /// </summary>
    /// <param name="manipMethod">IL 编辑方法，必须是具名方法。</param>
    /// <param name="sourceName">当解析成功时，输出源方法名称；否则为 <c>null</c>。</param>
    /// <returns>解析是否成功。</returns>
    /// <remarks>
    /// <para>解析逻辑：</para>
    /// <list type="number">
    /// <item><description>尝试获取 <paramref name="manipMethod"/> 的 <see cref="CustomManipulatorPrefixAttribute"/> 特性，若存在则使用其指定的前缀，否则使用默认前缀 "IL_"。</description></item>
    /// <item><description>尝试将方法名与模式 <c>{prefix}{methodName}[__{paramName}]</c> 进行匹配。</description></item>
    /// <item><description>如果匹配成功，输出 <c>methodName</c> 捕获组的值。</description></item>
    /// </list>
    /// </remarks>
    public static bool EvaluateManipulatorName(MethodInfo manipMethod, [NotNullWhen(true)] out string sourceName)
    {
        string prefix = manipMethod.Attribute<CustomManipulatorPrefixAttribute>()?.Prefix;
        Match match = string.IsNullOrEmpty(prefix) ? _defaultManipulatorNameRegex.Match(manipMethod.Name) : Regex.Match(manipMethod.Name, string.Format(Pattern, prefix));
        if (match.Success)
        {
            sourceName = match.Groups["methodName"].Value;
            return true;
        }
        sourceName = null;
        return false;
    }

    /// <summary>
    /// 创建并应用一个 IL 钩子到指定源方法上。
    /// </summary>
    /// <param name="source">目标源方法。</param>
    /// <param name="manip">IL 编辑委托。</param>
    /// <returns>创建的 <see cref="ILHook"/> 对象，已自动添加到 <see cref="Manipulators"/> 列表中。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> 或 <paramref name="manip"/> 为 <c>null</c>。</exception>
    public static ILHook Modify(MethodBase source, ILContext.Manipulator manip)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manip);
        return CreateILHook(source, manip, manip.Method);
    }

    /// <summary>
    /// 创建并应用一个 IL 钩子到指定源方法上，通过 <see cref="MethodInfo"/> 提供 IL 编辑方法。
    /// </summary>
    /// <param name="source">目标源方法。</param>
    /// <param name="manipMethod">IL 编辑方法，其签名必须与 <see cref="ILContext.Manipulator"/> 兼容。</param>
    /// <returns>创建的 <see cref="ILHook"/> 对象。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> 或 <paramref name="manipMethod"/> 为 <c>null</c>。</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="manipMethod"/> 的签名与 <see cref="ILContext.Manipulator"/> 委托不兼容。
    /// </exception>
    public static ILHook Modify(MethodBase source, MethodInfo manipMethod)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manipMethod);
        ILContext.Manipulator manipulator;
        try
        {
            manipulator = manipMethod.CreateDelegate<ILContext.Manipulator>();
        }
        catch (ArgumentException e)
        {
            throw new ArgumentException("The provided method's signature is not compatible with the delegate type ILContext.Manipulator.", nameof(manipMethod), e);
        }
        return CreateILHook(source, manipulator, manipMethod);
    }

    /// <summary>
    /// 在指定类型中按名称查找方法，并应用 IL 钩子。
    /// </summary>
    /// <param name="sourceType">源方法所在类型。</param>
    /// <param name="sourceName">源方法名称。</param>
    /// <param name="manip">IL 编辑委托。</param>
    /// <returns>创建的 <see cref="ILHook"/> 对象。</returns>
    /// <exception cref="ArgumentNullException">任一参数为 <c>null</c>。</exception>
    /// <exception cref="AmbiguousMatchException">找到多个同名方法且无法确定唯一匹配。</exception>
    public static ILHook Modify(Type sourceType, string sourceName, ILContext.Manipulator manip) => Modify(sourceType.GetMethod(sourceName, TOReflectionUtils.UniversalBindingFlags), manip);

    /// <summary>
    /// 在指定类型中按名称查找方法，并应用 IL 钩子。
    /// </summary>
    /// <param name="sourceType">源方法所在类型。</param>
    /// <param name="sourceName">源方法名称。</param>
    /// <param name="manipMethod">IL 编辑方法。</param>
    /// <returns>创建的 <see cref="ILHook"/> 对象。</returns>
    /// <inheritdoc cref="Modify(Type, string, ILContext.Manipulator)"/>    
    /// <exception cref="ArgumentException">
    /// <paramref name="manipMethod"/> 的签名与 <see cref="ILContext.Manipulator"/> 委托不兼容。
    /// </exception>
    public static ILHook Modify(Type sourceType, string sourceName, MethodInfo manipMethod) => Modify(sourceType.GetMethod(sourceName, TOReflectionUtils.UniversalBindingFlags), manipMethod);

    /// <summary>
    /// 在指定类型中按名称查找方法，并应用 IL 钩子。
    /// </summary>
    /// <typeparam name="T">源方法所在类型。</typeparam>
    /// <param name="sourceName">源方法名称。</param>
    /// <param name="manip">IL 编辑委托。</param>
    /// <returns>创建的 <see cref="ILHook"/> 对象。</returns>
    /// <inheritdoc cref="Modify(Type, string, ILContext.Manipulator)"/>
    public static ILHook Modify<T>(string sourceName, ILContext.Manipulator manip) => Modify(typeof(T), sourceName, manip);

    /// <summary>
    /// 在指定类型中按名称查找方法，并应用 IL 钩子。
    /// </summary>
    /// <typeparam name="T">源方法所在类型。</typeparam>
    /// <param name="sourceName">源方法名称。</param>
    /// <param name="manipMethod">IL 编辑方法。</param>
    /// <returns>创建的 <see cref="ILHook"/> 对象。</returns>
    /// <inheritdoc cref="Modify(Type, string, ILContext.Manipulator)"/>
    /// <exception cref="ArgumentException">
    /// <paramref name="manipMethod"/> 的签名与 <see cref="ILContext.Manipulator"/> 委托不兼容。
    /// </exception>
    public static ILHook Modify<T>(string sourceName, MethodInfo manipMethod) => Modify(typeof(T), sourceName, manipMethod);

    /// <summary>
    /// 创建 IL 钩子并添加到全局列表的内部实现。
    /// </summary>
    /// <param name="source">目标源方法。</param>
    /// <param name="manip">IL 编辑委托。</param>
    /// <param name="manipMethod">IL 编辑方法的元数据，用于检索附加特性。</param>
    /// <returns>创建的 <see cref="ILHook"/> 对象。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ILHook CreateILHook(MethodBase source, ILContext.Manipulator manip, MethodInfo manipMethod)
    {
        DetourConfig detourConfig = manipMethod.Attribute<CustomDetourConfigAttribute>()?.DetourConfig;
        ILHook hook = detourConfig is not null ? new ILHook(source, manip, detourConfig, true) : new ILHook(source, manip, true);
        Manipulators.Add(hook);
        return hook;
    }
}