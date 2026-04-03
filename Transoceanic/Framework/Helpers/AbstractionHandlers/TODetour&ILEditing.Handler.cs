using System.Collections;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Transoceanic.Framework.RuntimeEditing;

public sealed partial class TODetourHandler : IContentLoader
{
    public sealed class DetourSet : IEnumerable<Hook>
    {
        private readonly Dictionary<Type, Dictionary<MethodBase, List<Hook>>> _data = [];

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

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal static readonly DetourSet Detours = [];

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

    void IContentLoader.OnModUnload() => Detours.Clear();

    private const string DefaultPrefix = "Detour_";
    [StringSyntax(StringSyntaxAttribute.Regex)] private const string Pattern = """^{0}(?<methodName>[\S]*?)(?:__[\S]*)?$""";
    [StringSyntax(StringSyntaxAttribute.Regex)] private const string Pattern2 = """^{0}(?<typeName>[\S]*?)__(?<methodName>[\S]*?)(?:__[\S]*)?$""";

    /// <summary>
    /// 默认Detour方法名正则表达式。
    /// </summary>
    /// <inheritdoc cref = "GetDefaultDetourNameRegex" />
    private static readonly Regex _defaultDetourNameRegex = GetDefaultDetourNameRegex();
    [GeneratedRegex("""^Detour_(?<methodName>[\S]*?)(?:__[\S]*)?$""")]
    private static partial Regex GetDefaultDetourNameRegex();

    /// <summary>
    /// 默认Detour方法名（含类型）正则表达式。
    /// </summary>
    /// <inheritdoc cref = "GetDefaultDetourNameRegex2" />
    private static readonly Regex _defaultDetourNameRegex2 = GetDefaultDetourNameRegex2();
    [GeneratedRegex("""^Detour_(?<typeName>[\S]*?)__(?<methodName>[\S]*?)(?:__[\S]*)?$""")]
    private static partial Regex GetDefaultDetourNameRegex2();

    /// <summary>
    /// 尝试解析提供的Detour方法名，获取其应用的源方法名。
    /// <para/>逻辑：
    /// <br/> 1. 尝试获取detour参数的 <see cref="CustomDetourPrefixAttribute"/> 特性，如果存在，使用其提供的前缀，否则使用 <see cref="DefaultPrefix"/> 作为前缀，命名为prefix。
    /// <br/> 2. 尝试将方法名与 <c>{prefix}{methodName}[__{paramName}]</c> 进行匹配。
    /// <br/> 3. 如果匹配成功，输出methodName。
    /// </summary>
    /// <param name="detour">Detour方法，必须是具名方法。</param>
    /// <param name="sourceName">输出的源方法名。</param>
    /// <returns>解析是否成功。</returns>
    public static bool EvaluateDetourName(MethodInfo detour, [NotNullWhen(true)] out string sourceName)
    {
        string prefix = detour.Attribute<CustomDetourPrefixAttribute>()?.Prefix;
        Match match = prefix is null ? _defaultDetourNameRegex.Match(detour.Name) : Regex.Match(detour.Name, string.Format(Pattern, prefix));
        if (match.Success)
        {
            sourceName = match.Groups["methodName"].Value;
            return true;
        }
        sourceName = null;
        return false;
    }

    /// <summary>
    /// 尝试解析提供的Detour方法名，获取其应用的源类型名和源方法名。
    /// <para/>逻辑：
    /// <br/> 1. 尝试获取detour参数的 <see cref="CustomDetourPrefixAttribute"/> 特性，如果存在，使用其提供的前缀，否则使用 <see cref="DefaultPrefix"/> 作为前缀，命名为prefix。
    /// <br/> 2. 尝试将方法名与 <c>{prefix}{typeName}__{methodName}[__{paramName}]</c> 进行匹配。
    /// <br/> 3. 如果匹配成功，输出typeName和methodName。
    /// </summary>
    /// <param name="detour">Detour方法，必须是具名方法。</param>
    /// <param name="sourceTypeName">输出的源类型名</param>
    /// <param name="sourceMethodName">输出的源方法名。</param>
    /// <returns>解析是否成功。</returns>
    public static bool EvaluateTypedDetourName(MethodInfo detour, [NotNullWhen(true)] out string sourceTypeName, [NotNullWhen(true)] out string sourceMethodName)
    {
        string prefix = detour.Attribute<CustomDetourPrefixAttribute>()?.Prefix ?? DefaultPrefix;
        Match match = prefix is null ? _defaultDetourNameRegex2.Match(detour.Name) : Regex.Match(detour.Name, string.Format(Pattern2, prefix));
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
    /// 尝试将Detour应用到指定源方法上。
    /// </summary>
    /// <returns>创建的Hook对象。</returns>
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
    /// 尝试将Detour应用到指定源方法上。
    /// </summary>
    /// <returns>创建的Hook对象。</returns>
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
    /// 尝试在指定类中获取对应名称方法，并将Detour应用到获取的方法上。
    /// </summary>
    /// <returns>创建的Hook对象。</returns>
    public static Hook Modify<TSource, TTarget>(TSource source, TTarget detour) where TSource : Delegate where TTarget : Delegate => Modify(source.Method, detour);

    /// <summary>
    /// 尝试在指定类中获取对应名称方法，并将Detour应用到获取的方法上。
    /// </summary>
    /// <returns>创建的Hook对象。</returns>
    public static Hook Modify(Type sourceType, string sourceMethodName, MethodInfo detour) => Modify(sourceType.GetMethod(sourceMethodName, TOReflectionUtils.UniversalBindingFlags), detour);

    /// <summary>
    /// 尝试在指定类中获取对应名称方法，并将Detour应用到获取的方法上。
    /// </summary>
    /// <returns>创建的Hook对象。</returns>
    public static Hook Modify<TDelegate>(Type sourceType, string sourceMethodName, TDelegate detour) where TDelegate : Delegate => Modify(sourceType.GetMethod(sourceMethodName, TOReflectionUtils.UniversalBindingFlags), detour);

    /// <summary>
    /// 尝试在指定类中获取对应名称方法，并将Detour应用到获取的方法上。
    /// </summary>
    /// <returns>创建的Hook对象。</returns>
    public static Hook Modify<T>(string sourceMethodName, MethodInfo detour) => Modify(typeof(T), sourceMethodName, detour);

    /// <summary>
    /// 尝试在指定类中获取对应名称方法，并将Detour应用到获取的方法上。
    /// </summary>
    /// <param name="paramOffset">Detour方法参数偏移量。
    /// <br/>应设置为目标方法第一个参数在Detour方法中的索引。
    /// <br/>例如，若Detour方法有 <c>orig</c> 和 <c>self</c> 参数，则应设置为 <c>2</c>；若都没有，则应设置为 <c>0</c>。
    /// <br/>只应设置为 <c>0</c>、<c>1</c> 或 <c>2</c>。
    /// </param>
    /// <returns>创建的Hook对象。</returns>
    public static Hook Modify(Type sourceType, string sourceMethodName, int paramOffset, MethodInfo detour) => Modify(sourceType.GetMethod(sourceMethodName, TOReflectionUtils.UniversalBindingFlags, detour.ParameterTypes[paramOffset..]), detour);

    /// <summary>
    /// 尝试在指定类中获取对应名称方法，并将Detour应用到获取的方法上。
    /// </summary>
    /// <param name="paramOffset">Detour方法参数偏移量。
    /// <br/>应设置为目标方法第一个参数在Detour方法中的索引。
    /// <br/>例如，若Detour方法有 <c>orig</c> 和 <c>self</c> 参数，则应设置为 <c>2</c>；若都没有，则应设置为 <c>0</c>。
    /// <br/>只应设置为 <c>0</c>、<c>1</c> 或 <c>2</c>。
    /// </param>
    /// <returns>创建的Hook对象。</returns>
    public static Hook Modify<TDelegate>(Type sourceType, string sourceMethodName, int paramOffset, TDelegate detour) where TDelegate : Delegate => Modify(sourceType.GetMethod(sourceMethodName, TOReflectionUtils.UniversalBindingFlags, detour.Method.ParameterTypes[paramOffset..]), detour);

    /// <summary>
    /// 尝试在指定类中获取对应名称方法，并将Detour应用到获取的方法上。
    /// </summary>
    /// <param name="paramOffset">Detour方法参数偏移量。
    /// <br/>应设置为目标方法第一个参数在Detour方法中的索引。
    /// <br/>例如，若Detour方法有 <c>orig</c> 和 <c>self</c> 参数，则应设置为 <c>2</c>；若都没有，则应设置为 <c>0</c>。
    /// <br/>只应设置为 <c>0</c>、<c>1</c> 或 <c>2</c>。
    /// </param>
    /// <returns>创建的Hook对象。</returns>
    public static Hook Modify<T>(string sourceMethodName, int paramOffset, MethodInfo detour) => Modify(typeof(T), sourceMethodName, paramOffset, detour);

    /// <summary>
    /// 尝试在指定类中获取对应名称方法，并将Detour应用到获取的方法上。
    /// </summary>
    /// <param name="hasThis">目标方法是否为实例方法（有 <see langword="this"/> 指针）。
    /// <br/>会影响获取方法时的参数偏移量（<see langword="true"/> 为2，反之为1）和 <c>bindingAttr</c> 实参。
    /// </param>
    /// <returns>创建的Hook对象。</returns>
    public static Hook Modify(Type sourceType, string sourceMethodName, bool hasThis, MethodInfo detour) =>
        Modify(hasThis ? sourceType.GetMethod(sourceMethodName, TOReflectionUtils.InstanceBindingFlags, detour.ParameterTypes[2..])
            : sourceType.GetMethod(sourceMethodName, TOReflectionUtils.StaticBindingFlags, detour.ParameterTypes[1..]), detour);

    /// <summary>
    /// 尝试在指定类中获取对应名称方法，并将Detour应用到获取的方法上。
    /// </summary>
    /// <param name="hasThis">目标方法是否为实例方法（有 <see langword="this"/> 指针）。
    /// <br/>会影响获取方法时的参数偏移量（<see langword="true"/> 为2，反之为1）和 <c>bindingAttr</c> 实参。
    /// </param>
    /// <returns>创建的Hook对象。</returns>
    public static Hook Modify<TDelegate>(Type sourceType, string sourceMethodName, bool hasThis, TDelegate detour) where TDelegate : Delegate =>
        Modify(hasThis ? sourceType.GetMethod(sourceMethodName, TOReflectionUtils.InstanceBindingFlags, detour.Method.ParameterTypes[2..])
            : sourceType.GetMethod(sourceMethodName, TOReflectionUtils.StaticBindingFlags, detour.Method.ParameterTypes[1..]), detour);

    /// <summary>
    /// 尝试在指定类中获取对应名称方法，并将Detour应用到获取的方法上。
    /// </summary>
    /// <param name="hasThis">目标方法是否为实例方法（有 <see langword="this"/> 指针）。
    /// <br/>会影响获取方法时的参数偏移量（<see langword="true"/> 为2，反之为1）和 <c>bindingAttr</c> 实参。
    /// </param>
    /// <returns>创建的Hook对象。</returns>
    public static Hook Modify<T>(string sourceMethodName, bool hasThis, MethodInfo detour) => Modify(typeof(T), sourceMethodName, hasThis, detour);

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

    public static Hook ApplyTypedStaticMethodDetour(MethodInfo detour, Type[] sourceTypes)
    {
        if (detour.HasAttribute<NotDetourMethodAttribute>())
            return null;
        if (detour.TryGetAttribute(out CustomDetourSourceAttribute attribute))
            return Modify(attribute.Source, detour);
        if (EvaluateTypedDetourName(detour, out string sourceTypeName, out string sourceMethodName))
        {
            Type sourceType = sourceTypes.AsValueEnumerable().FirstOrDefault(t => t.Name == sourceTypeName);
            if (sourceType is not null)
                return Modify(sourceType.GetMethod(sourceMethodName, TOReflectionUtils.UniversalBindingFlags), detour);
        }
        return null;
    }

    public static void ApplyAllStaticMethodDetoursOfType(Type type, Type sourceType)
    {
        foreach (MethodInfo detour in type.GetRealMethods(TOReflectionUtils.StaticBindingFlags))
            ApplyStaticMethodDetour(detour, sourceType);
    }
}

public sealed partial class TOILEditingHandler : IContentLoader
{

    internal static readonly List<ILHook> Manipulators = [];

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

    void IContentLoader.OnModUnload() => Manipulators.Clear();

    [StringSyntax(StringSyntaxAttribute.Regex)] private const string Pattern = """^{0}(?<methodName>[\S]*?)(?:__[\S]*)?$""";

    /// <summary>
    /// 默认IL编辑方法正则表达式。
    /// </summary>
    /// <inheritdoc cref = "GetDefaultManipulatorNameRegex" />
    private static readonly Regex _defaultManipulatorNameRegex = GetDefaultManipulatorNameRegex();
    [GeneratedRegex("""^IL_(?<methodName>[\S]*)$""")]
    private static partial Regex GetDefaultManipulatorNameRegex();

    public static bool EvaluateManipulatorName(MethodInfo detour, [NotNullWhen(true)] out string sourceName)
    {
        string prefix = detour.Attribute<CustomManipulatorPrefixAttribute>()?.Prefix;
        Match match = prefix is null ? _defaultManipulatorNameRegex.Match(detour.Name) : Regex.Match(detour.Name, string.Format(Pattern, prefix));
        if (match.Success)
        {
            sourceName = match.Groups["methodName"].Value;
            return true;
        }
        sourceName = null;
        return false;
    }

    public static ILHook Modify(MethodBase source, ILContext.Manipulator manip)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manip);
        return CreateILHook(source, manip, manip.Method);
    }

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

    public static ILHook Modify(Type sourceType, string sourceName, ILContext.Manipulator manip) => Modify(sourceType.GetMethod(sourceName, TOReflectionUtils.UniversalBindingFlags), manip);

    public static ILHook Modify(Type sourceType, string sourceName, MethodInfo manip) => Modify(sourceType.GetMethod(sourceName, TOReflectionUtils.UniversalBindingFlags), manip);

    public static ILHook Modify<T>(string sourceName, ILContext.Manipulator manip) => Modify(typeof(T), sourceName, manip);

    public static ILHook Modify<T>(string sourceName, MethodInfo manip) => Modify(typeof(T), sourceName, manip);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ILHook CreateILHook(MethodBase source, ILContext.Manipulator manip, MethodInfo manipMethod)
    {
        DetourConfig detourConfig = manipMethod.Attribute<CustomDetourConfigAttribute>()?.DetourConfig;
        ILHook hook = detourConfig is not null ? new ILHook(source, manip, detourConfig, true) : new ILHook(source, manip, true);
        Manipulators.Add(hook);
        return hook;
    }
}