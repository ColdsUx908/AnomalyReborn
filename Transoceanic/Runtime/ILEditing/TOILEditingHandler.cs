using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Transoceanic.Runtime.Detours;
using Transoceanic.Runtime.Reflection;

namespace Transoceanic.Runtime.ILEditing;

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
                Modify(sourceType, GetMarkedSourceName(manipulator, sourceName), manipulator);
        }
    }

    /// <summary>
    /// <see cref="IContentLoader.OnModUnload"/> 的实现。
    /// 在模组卸载时清理所有已应用的 IL 钩子。
    /// </summary>
    void IContentLoader.OnModUnload() => Manipulators.Clear();

    [StringSyntax(StringSyntaxAttribute.Regex)] private const string Pattern = """^{0}(?<methodName>[\S]+?)(?:__[\S]+)?$""";

    /// <summary>
    /// 默认 IL 编辑方法名正则表达式。
    /// </summary>
    /// <remarks>
    /// 匹配模式：<c>IL_{methodName}[__{parameterType}...]</c>，例如 IL_Update__Player。
    /// </remarks>
    private static readonly Regex _DefaultManipulatorNameRegex = GetDefaultManipulatorNameRegex();
    [GeneratedRegex("""^IL_(?<methodName>[\S]+?)(?:__[\S]+)?$""")]
    private static partial Regex GetDefaultManipulatorNameRegex();

    /// <summary>
    /// 尝试解析 IL 编辑方法名，获取其应用的源方法名。
    /// </summary>
    /// <param name="manipMethod">IL 编辑方法，必须是具名方法。</param>
    /// <param name="sourceName">当解析成功时，输出源方法名称；否则为 <see langword="null"/>。</param>
    /// <returns>解析是否成功。</returns>
    /// <remarks>
    /// <para>解析逻辑：</para>
    /// <list type="number">
    /// <item><description>尝试获取 <paramref name="manipMethod"/> 的 <see cref="CustomManipulatorPrefixAttribute"/> 特性，若存在则使用其指定的前缀，否则使用默认前缀 "IL_"。</description></item>
    /// <item><description>尝试将方法名与模式 <c>{prefix}{methodName}[__{parameterType}...]</c> 进行匹配。</description></item>
    /// <item><description>如果匹配成功，输出 <c>methodName</c> 捕获组的值。</description></item>
    /// </list>
    /// </remarks>
    internal static bool EvaluateManipulatorName(MethodInfo manipMethod, [NotNullWhen(true)] out string sourceName)
    {
        string prefix = manipMethod.Attribute<CustomManipulatorPrefixAttribute>()?.Prefix;
        Match match = string.IsNullOrEmpty(prefix) ? _DefaultManipulatorNameRegex.Match(manipMethod.Name) : Regex.Match(manipMethod.Name, string.Format(Pattern, Regex.Escape(prefix)));
        if (match.Success)
        {
            sourceName = match.Groups["methodName"].Value;
            return true;
        }
        sourceName = null;
        return false;
    }

    private static string GetMarkedSourceName(MethodInfo manipulator, string sourceName)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceName);
        if (sourceName.Contains("__", StringComparison.Ordinal))
            return sourceName;
        string prefix = manipulator.Attribute<CustomManipulatorPrefixAttribute>()?.Prefix;
        int marker = manipulator.Name.IndexOf("__", string.IsNullOrEmpty(prefix) ? 0 : prefix.Length, StringComparison.Ordinal);
        return marker < 0 ? sourceName : sourceName + manipulator.Name[marker..];
    }

    /// <summary>
    /// 按方法名及可选的 __ 参数类型标记定位唯一目标，如 Update__Player 或 Foo__Int32__String。
    /// 标记按参数顺序匹配类型短名或全名；可省略末尾参数，前提是剩余候选唯一。
    /// 自动扫描标记可用 Int32Array 表示一维数组、Int32ByRef 表示引用参数；显式名称也可使用反射类型名。
    /// </summary>
    /// <exception cref="ArgumentNullException">源类型或名称为空引用。</exception>
    /// <exception cref="MissingMethodException">未找到匹配方法。</exception>
    /// <exception cref="AmbiguousMatchException">标记不足以唯一定位重载。</exception>
    internal static MethodInfo ResolveSourceMethod(Type sourceType, string sourceName)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentException.ThrowIfNullOrEmpty(sourceName);
        string[] parts = sourceName.Split("__", StringSplitOptions.None);
        if (parts.Any(string.IsNullOrEmpty))
            throw new ArgumentException("Method and parameter markers cannot be empty.", nameof(sourceName));
        MethodInfo[] matches = sourceType.GetMethods(TOReflectionUtils.UniversalBindingFlags)
            .Where(method => method.Name == parts[0] && MatchesParameters(method.GetParameters()))
            .ToArray();
        return matches.Length switch
        {
            0 => throw new MissingMethodException(sourceType.FullName, sourceName),
            1 => matches[0],
            _ => throw new AmbiguousMatchException($"'{sourceType.FullName}.{sourceName}' matches multiple methods; append __ parameter type markers.")
        };

        bool MatchesParameters(ParameterInfo[] parameters)
        {
            if (parts.Length - 1 > parameters.Length)
                return false;
            for (int i = 1; i < parts.Length; i++)
            {
                Type type = parameters[i - 1].ParameterType;
                if (parts[i] != type.Name && parts[i] != type.FullName && parts[i] != GetTypeMarker(type))
                    return false;
            }
            return true;
        }
    }

    private static string GetTypeMarker(Type type) => type.IsByRef
        ? GetTypeMarker(type.GetElementType()) + "ByRef"
        : type.IsArray
            ? GetTypeMarker(type.GetElementType()) + (type.GetArrayRank() == 1 ? "Array" : $"Array{type.GetArrayRank()}")
            : type.Name;

    /// <summary>
    /// 创建并应用一个 IL 钩子到指定源方法上。
    /// </summary>
    /// <param name="source">目标源方法。</param>
    /// <param name="manip">IL 编辑委托。</param>
    /// <returns>创建的 <see cref="ILHook"/> 对象，已自动添加到 <see cref="Manipulators"/> 列表中。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> 或 <paramref name="manip"/> 为 <see langword="null"/>。</exception>
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
    /// <exception cref="ArgumentNullException"><paramref name="source"/> 或 <paramref name="manipMethod"/> 为 <see langword="null"/>。</exception>
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
    /// 在指定类型中按名称和可选的 __ 参数类型标记查找唯一方法，并应用 IL 钩子。
    /// </summary>
    /// <param name="sourceType">源方法所在类型。</param>
    /// <param name="sourceName">源方法名称。</param>
    /// <param name="manip">IL 编辑委托。</param>
    /// <returns>创建的 <see cref="ILHook"/> 对象。</returns>
    /// <exception cref="ArgumentNullException">任一参数为 <see langword="null"/>。</exception>
    /// <exception cref="AmbiguousMatchException">找到多个同名方法且无法确定唯一匹配。</exception>
    public static ILHook Modify(Type sourceType, string sourceName, ILContext.Manipulator manip)
    {
        ArgumentNullException.ThrowIfNull(manip);
        return Modify(ResolveSourceMethod(sourceType, GetMarkedSourceName(manip.Method, sourceName)), manip);
    }

    /// <summary>
    /// 在指定类型中按名称和可选的 __ 参数类型标记查找唯一方法，并应用 IL 钩子。
    /// </summary>
    /// <param name="sourceType">源方法所在类型。</param>
    /// <param name="sourceName">源方法名称。</param>
    /// <param name="manipMethod">IL 编辑方法。</param>
    /// <returns>创建的 <see cref="ILHook"/> 对象。</returns>
    /// <inheritdoc cref="Modify(Type, string, ILContext.Manipulator)"/>
    /// <exception cref="ArgumentException">
    /// <paramref name="manipMethod"/> 的签名与 <see cref="ILContext.Manipulator"/> 委托不兼容。
    /// </exception>
    public static ILHook Modify(Type sourceType, string sourceName, MethodInfo manipMethod)
    {
        ArgumentNullException.ThrowIfNull(manipMethod);
        return Modify(ResolveSourceMethod(sourceType, GetMarkedSourceName(manipMethod, sourceName)), manipMethod);
    }

    /// <summary>
    /// 在指定类型中按名称和可选的 __ 参数类型标记查找唯一方法，并应用 IL 钩子。
    /// </summary>
    /// <typeparam name="T">源方法所在类型。</typeparam>
    /// <param name="sourceName">源方法名称。</param>
    /// <param name="manip">IL 编辑委托。</param>
    /// <returns>创建的 <see cref="ILHook"/> 对象。</returns>
    /// <inheritdoc cref="Modify(Type, string, ILContext.Manipulator)"/>
    public static ILHook Modify<T>(string sourceName, ILContext.Manipulator manip) => Modify(typeof(T), sourceName, manip);

    /// <summary>
    /// 在指定类型中按名称和可选的 __ 参数类型标记查找唯一方法，并应用 IL 钩子。
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
