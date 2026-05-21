// Developed by ColdsUx

namespace TransoceanicCodeAssist;

/// <summary>
/// 符号信息基类，封装了 <typeparamref name="TSymbol"/> 类型符号的公共属性，
/// 并在构造时进行基本的有效性校验。
/// </summary>
/// <typeparam name="TSymbol">具体的符号类型，必须实现 <see cref="ISymbol"/>。</typeparam>
internal abstract class SymbolInfoBase<TSymbol> where TSymbol : ISymbol
{
    /// <summary>
    /// 获取与此信息对象关联的原始符号。
    /// </summary>
    public TSymbol Symbol { get; private set; }

    /// <summary>
    /// 获取符号所在的命名空间。如果符号没有对应的命名空间（例如全局命名空间），则可能为 <see langword="null"/> 或空字符串。
    /// </summary>
    public string Namespace { get; protected set; }

    /// <summary>
    /// 获取符号的声明可访问性。
    /// </summary>
    public Accessibility Accessibility { get; protected set; }

    /// <summary>
    /// 获取包含该符号的直接类型的完全限定名。对于顶级类型为 <see langword="null"/>。
    /// </summary>
    public string ContainingType { get; protected set; }

    /// <summary>
    /// 获取成员类型的完全限定名字符串。对于类型符号，此属性存储其自身完全限定名；
    /// 对于字段、属性、方法等成员，存储其声明类型的完全限定名。
    /// </summary>
    public string MemberType { get; protected set; }

    /// <summary>
    /// 获取符号的名称。
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// 获取应用到此符号上的所有特性的数据数组。
    /// </summary>
    public AttributeData[] Attributes { get; protected set; }

    /// <summary>
    /// 获取一个值，该值指示当前信息对象是否有效。
    /// 无效的信息对象不应被用于生成代码。
    /// </summary>
    public bool Valid { get; protected set; }

    /// <summary>
    /// 获取可访问性的小写字符串表示（例如 "public"、"internal"），便于直接插入到生成的代码中。
    /// </summary>
    public string AccessibilityString => Accessibility.ToString().ToLowerInvariant();

    /// <summary>
    /// 使用指定的符号初始化 <see cref="SymbolInfoBase{TSymbol}"/> 的新实例。
    /// </summary>
    /// <param name="symbol">要包装的符号。</param>
    protected SymbolInfoBase(TSymbol symbol) => Symbol = symbol;

    /// <summary>
    /// 在符号的特性列表中查找指定完全限定名的特性。
    /// </summary>
    /// <param name="attributeFullName">要查找的特性的完全限定名。</param>
    /// <param name="data">如果找到，则返回匹配的 <see cref="AttributeData"/>；否则为 <see langword="null"/>。</param>
    /// <returns>如果找到匹配的特性，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool TryGetAttribute(string attributeFullName, out AttributeData data) => SourceGeneratorHelper.TryGetAttribute(attributeFullName, Attributes, out data);

    /// <summary>
    /// 初始化核心属性，设置 <see cref="Valid"/> 标志，并填充命名空间、包含类型、成员类型、
    /// 可访问性、名称以及特性列表。若 <paramref name="symbolNamespace"/> 为 <see langword="null"/>，
    /// 则将 <see cref="Valid"/> 置为 <see langword="false"/>。
    /// </summary>
    /// <param name="symbolNamespace">符号所在的命名空间（完全限定名字符串）。</param>
    /// <param name="containingType">包含类型的完全限定名（若无则为 <see langword="null"/>）。</param>
    /// <param name="memberType">成员类型的完全限定名。</param>
    protected void InitializeCore(string symbolNamespace, string containingType, string memberType)
    {
        Valid = true;
        Namespace = symbolNamespace;
        if (Namespace is null)
        {
            Valid = false;
            return;
        }

        Accessibility = Symbol.DeclaredAccessibility;
        ContainingType = containingType;

        MemberType = memberType;
        Name = Symbol.Name;
        Attributes = [.. Symbol.GetAttributes()];
    }
}

/// <summary>
/// 类型符号信息基类，提供生成类型声明（包括嵌套类型和命名空间）的功能。
/// </summary>
internal class TypeSymbolInfoBase : SymbolInfoBase<INamedTypeSymbol>
{
    /// <summary>
    /// 使用指定的命名类型符号初始化 <see cref="TypeSymbolInfoBase"/> 的新实例。
    /// 初始化时填充命名空间、包含类型以及类型的完全限定名作为 <see cref="SymbolInfoBase{TSymbol}.MemberType"/>。
    /// </summary>
    /// <param name="symbol">要包装的命名类型符号。</param>
    public TypeSymbolInfoBase(INamedTypeSymbol symbol) : base(symbol)
    {
        InitializeCore(
            symbolNamespace: symbol.ContainingNamespace?.ToDisplayString(),
            containingType: symbol.ContainingType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            memberType: symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
    }

    /// <summary>
    /// 生成完整的类型声明文本，将指定的内部成员文本嵌套在适当的位置，
    /// 并自动处理外层类型和命名空间的包裹。
    /// </summary>
    /// <param name="innerText">要放置在类型声明内部的成员文本（可能为空）。</param>
    /// <param name="indentLevel">当前代码的缩进级别。</param>
    /// <param name="includeNamespace">如果为 <see langword="true"/>，则在必要时包裹命名空间声明。</param>
    /// <returns>生成的类型声明字符串。</returns>
    public string GenerateDeclarationText(string innerText, int indentLevel, bool includeNamespace = false)
    {
        (string prefix, string suffix, int innerIndent) = GenerateDeclaration(indentLevel, includeNamespace);
        string indentedInner = innerText.AddIndent(innerIndent);
        // 移除 inner 文本末尾的换行，避免与 suffix 前的换行重复
        indentedInner = indentedInner.TrimEnd();
        if (string.IsNullOrEmpty(indentedInner))
        {
            // 无成员时，仅用一个换行分隔 prefix 和 suffix（如 { 和 } 各占一行）
            return $"""
                {prefix}
                {suffix}
                """;
        }
        // 有成员时，prefix 后换行，成员文本后换行，再拼接 suffix
        return $"""
            {prefix}
            {indentedInner.TrimEnd()}
            {suffix}
            """;
    }

    /// <summary>
    /// 生成类型声明的前缀、后缀以及内部成员的缩进级别。
    /// 如果类型包含在另一个类型中，将递归生成外层类型的声明，并把命名空间包装在最外层（如果 <paramref name="includeNamespace"/> 为 <see langword="true"/>）。
    /// </summary>
    /// <param name="indentLevel">缩进级别。</param>
    /// <param name="includeNamespace">是否包含命名空间声明。</param>
    /// <returns>一个元组，包含声明前缀、声明后缀以及内部缩进级别。</returns>
    public (string prefix, string suffix, int innerIndent) GenerateDeclaration(int indentLevel, bool includeNamespace = false)
    {
        const int indentSize = 4;

        if (!Valid)
            return (string.Empty, string.Empty, indentLevel);

        // 处理外层类型（如果存在）
        if (Symbol.ContainingType is not null)
        {
            TypeSymbolInfoBase outer = new(Symbol.ContainingType);
            (string outerPrefix, string outerSuffix, int outerInnerIndent) = outer.GenerateDeclaration(indentLevel, includeNamespace: false);
            string indent = new(' ', outerInnerIndent * indentSize);
            (string prefixSelf, string suffixSelf, int innerIndentSelf) = BuildTypeDeclaration(indent, this);

            string finalPrefix = $"""
                {outerPrefix}
                {prefixSelf}
                """;
            string finalSuffix = $"""
                {suffixSelf}
                {outerSuffix}
                """;

            int finalInnerIndent = innerIndentSelf;

            // 是否需要包装命名空间
            if (includeNamespace && !string.IsNullOrEmpty(Namespace))
            {
                string indentedPrefix = finalPrefix.AddIndent(1);
                string indentedSuffix = finalSuffix.AddIndent(1);
                string nsPrefix = $$"""
                    namespace {{Namespace}}
                    {
                    {{indentedPrefix}}
                    """;
                string nsSuffix = $$"""
                    {{indentedSuffix}}
                    }
                    """;
                return (nsPrefix, nsSuffix, finalInnerIndent + 1);
            }

            return (finalPrefix, finalSuffix, finalInnerIndent);
        }
        else
        {
            string indent = new(' ', indentLevel * indentSize);
            (string prefixSelf, string suffixSelf, int innerIndentSelf) = BuildTypeDeclaration(indent, this);

            if (includeNamespace && !string.IsNullOrEmpty(Namespace))
            {
                string indentedPrefix = prefixSelf.AddIndent(1);
                string indentedSuffix = suffixSelf.AddIndent(1);
                string nsPrefix = $$"""
                    namespace {{Namespace}}
                    {
                    {{indentedPrefix}}
                    """;
                string nsSuffix = $$"""
                    {{indentedSuffix}}
                    }
                    """;
                return (nsPrefix, nsSuffix, innerIndentSelf + 1);
            }

            return (prefixSelf, suffixSelf, innerIndentSelf);
        }
    }

    /// <summary>
    /// 构建单个类型声明的行（不包括外层类型和命名空间）。
    /// 处理委托与其他类型（类、结构等）的不同语法结构。
    /// </summary>
    /// <param name="indent">当前缩进字符串（由空格组成）。</param>
    /// <param name="type">类型符号信息对象。</param>
    /// <returns>包含声明前缀、声明后缀和内部成员缩进级别的元组。</returns>
    private static (string prefix, string suffix, int innerIndent) BuildTypeDeclaration(string indent, TypeSymbolInfoBase type)
    {
        List<string> modifiers = [type.AccessibilityString];
        if (type.Symbol.IsStatic)
            modifiers.Add("static");
        else
        {
            if (type.Symbol.IsAbstract)
                modifiers.Add("abstract");
            if (type.Symbol.IsSealed)
                modifiers.Add("sealed");
        }
        modifiers.Add("partial");
        string keyword = GetTypeKeyword(type.Symbol);
        string typeParams = type.Symbol.TypeParameters.Length > 0
            ? "<" + string.Join(", ", type.Symbol.TypeParameters.Select(p => p.Name)) + ">"
            : "";
        string nameWithGenerics = type.Name + typeParams;
        string declarationLine = $"{indent}{string.Join(" ", modifiers)} {keyword} {nameWithGenerics}";

        if (type.Symbol.TypeKind == TypeKind.Delegate)
        {
            // 委托：单行声明，末尾不加换行
            string prefix = declarationLine + ";";
            string suffix = string.Empty;
            int innerIndent = indent.Length / 4;
            return (prefix, suffix, innerIndent);
        }
        else
        {
            // 其他类型：声明行 + 换行 + 左花括号（末尾无换行），右花括号单独一行（末尾无换行）
            string prefix = declarationLine + """


                """ + indent + "{";
            string suffix = indent + "}";
            int innerIndent = (indent.Length / 4) + 1;
            return (prefix, suffix, innerIndent);
        }
    }

    /// <summary>
    /// 获取与指定命名类型符号相对应的 C# 关键字。
    /// </summary>
    /// <param name="symbol">命名类型符号。</param>
    /// <returns>类型关键字，如 "class"、"struct"、"record" 等。</returns>
    private static string GetTypeKeyword(INamedTypeSymbol symbol)
    {
        if (symbol.IsRecord)
            return symbol.TypeKind == TypeKind.Struct ? "record struct" : "record";
        return symbol.TypeKind switch
        {
            TypeKind.Class => "class",
            TypeKind.Interface => "interface",
            TypeKind.Struct => "struct",
            TypeKind.Enum => "enum",
            TypeKind.Delegate => "delegate",
            _ => "class"
        };
    }
}

/// <summary>
/// 封装字段符号的信息，并提供生成字段声明字符串的功能。
/// </summary>
internal class FieldSymbolInfoBase : SymbolInfoBase<IFieldSymbol>
{
    /// <summary>
    /// 使用指定的字段符号初始化 <see cref="FieldSymbolInfoBase"/> 的新实例。
    /// 会提取字段的命名空间、包含类型以及字段类型的完全限定名。
    /// </summary>
    /// <param name="symbol">要包装的字段符号。</param>
    public FieldSymbolInfoBase(IFieldSymbol symbol) : base(symbol)
    {
        InitializeCore(
            symbolNamespace: symbol.ContainingType.ContainingNamespace?.ToDisplayString(),
            containingType: symbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            memberType: symbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
    }

    /// <summary>
    /// 生成字段自身的声明字符串（不包含外层类型或命名空间）。
    /// 例如：<c>private int _field;</c>
    /// </summary>
    /// <returns>如果信息有效，则为字段声明字符串；否则为 <see cref="string.Empty"/>。</returns>
    public string GenerateDeclaration()
    {
        if (!Valid)
            return string.Empty;

        List<string> modifiers = [AccessibilityString];

        if (Symbol.IsStatic)
            modifiers.Add("static");
        if (Symbol.IsReadOnly && !Symbol.IsConst)
            modifiers.Add("readonly");
        if (Symbol.IsConst)
            modifiers.Add("const");

        string type = Symbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        string name = Symbol.Name;

        return $"{string.Join(" ", modifiers)} {type} {name};";
    }
}

/// <summary>
/// 封装属性符号的信息，提供属性声明字符串的生成能力，
/// 并暴露 <see cref="GetMethod"/> 和 <see cref="SetMethod"/> 以支持访问器处理。
/// </summary>
internal class PropertySymbolInfoBase : SymbolInfoBase<IPropertySymbol>
{
    /// <summary>获取属性的 get 访问器方法（可能为 null）。</summary>
    public IMethodSymbol GetMethod { get; }

    /// <summary>获取属性的 set 访问器方法（可能为 null）。</summary>
    public IMethodSymbol SetMethod { get; }

    /// <summary>
    /// 使用指定的属性符号初始化 <see cref="PropertySymbolInfoBase"/> 的新实例。
    /// 同时提取 get/set 访问器方法。
    /// </summary>
    /// <param name="symbol">要包装的属性符号。</param>
    public PropertySymbolInfoBase(IPropertySymbol symbol) : base(symbol)
    {
        GetMethod = symbol.GetMethod;
        SetMethod = symbol.SetMethod;

        InitializeCore(
            symbolNamespace: symbol.ContainingType.ContainingNamespace?.ToDisplayString(),
            containingType: symbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            memberType: symbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
    }

    /// <summary>
    /// 生成属性自身的声明字符串（不包含外层类型）。
    /// 自动处理 static、abstract、virtual、override、sealed 等修饰符，
    /// 并始终添加 partial 关键字。访问器根据符号的 get/set 方法和 init 语义生成。
    /// 示例：<c>public int MyProperty { get; set; }</c>
    /// </summary>
    /// <returns>属性声明字符串；如果无效则返回 <see cref="string.Empty"/>。</returns>
    public string GenerateDeclaration()
    {
        if (!Valid)
            return string.Empty;

        List<string> modifiers = [AccessibilityString];

        if (Symbol.IsStatic)
            modifiers.Add("static");

        if (Symbol.IsAbstract)
            modifiers.Add("abstract");
        else if (Symbol.IsOverride)
            modifiers.Add("override");
        else if (Symbol.IsVirtual && !Symbol.IsSealed)
            modifiers.Add("virtual");

        modifiers.Add("partial"); //始终添加partial关键字，避免问题

        string type = Symbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        string name = Symbol.Name;

        string getter = Symbol.GetMethod is not null ? "get; " : "";
        string setter = "";
        if (Symbol.SetMethod is not null)
            setter = Symbol.SetMethod.IsInitOnly ? "init; " : "set; ";

        string accessors = (getter + setter).TrimEnd();
        return $"{string.Join(" ", modifiers)} {type} {name} {{ {accessors} }}";
    }
}

/// <summary>
/// 封装方法符号的信息，提供生成方法声明和对应委托声明的功能。
/// 同时预计算了参数相关的多种字符串表示，便于在源生成器中使用。
/// </summary>
internal class MethodSymbolInfoBase : SymbolInfoBase<IMethodSymbol>
{
    /// <summary>返回类型的完全限定名字符串表示。</summary>
    public string ReturnTypeString { get; private set; }

    /// <summary>类型参数列表的字符串表示（例如 "&lt;T&gt;"），无泛型时为空字符串。</summary>
    public string TypeParametersString { get; private set; }

    /// <summary>每个参数的完整声明字符串数组（例如 "int x", "ref string s"），使用原始类型完全限定名。</summary>
    public string[] ParameterDeclarations { get; private set; }

    /// <summary>每个参数的完整声明字符串数组，但在类型不可访问时使用 <see langword="object"/> 代替。</summary>
    public string[] ParameterDeclarationsWithObjectForNonPublic { get; private set; }

    /// <summary>每个参数的标识符字符串数组（例如 "x", "s"）。</summary>
    public string[] ParameterNames { get; private set; }

    /// <summary>每个参数为调用准备的标识符字符串数组（例如 "x", "ref s"），包含 ref/out 等修饰符。</summary>
    public string[] ParameterNamesForCall { get; private set; }

    /// <summary>
    /// 使用指定的方法符号初始化 <see cref="MethodSymbolInfoBase"/> 的新实例，
    /// 并预计算返回类型、泛型参数、参数声明等字符串。
    /// </summary>
    /// <param name="symbol">要包装的方法符号。</param>
    public MethodSymbolInfoBase(IMethodSymbol symbol) : base(symbol)
    {
        InitializeCore(
            symbolNamespace: symbol.ContainingType?.ContainingNamespace?.ToDisplayString(),
            containingType: symbol.ContainingType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            memberType: symbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

        if (!Valid)
        {
            ReturnTypeString = "";
            TypeParametersString = "";
            ParameterDeclarations = [];
            ParameterDeclarationsWithObjectForNonPublic = [];
            ParameterNames = [];
            ParameterNamesForCall = [];

            return;
        }

        ReturnTypeString = Symbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        if (Symbol.TypeParameters.Length > 0)
        {
            TypeParametersString = "<" + string.Join(", ", Symbol.TypeParameters.Select(tp => tp.Name)) + ">";
        }
        else
        {
            TypeParametersString = "";
        }

        // 计算参数相关数组
        List<string> paramDeclarations = [];
        List<string> paramDeclarationsWithObjectForNonPublic = [];
        List<string> paramNames = [];
        List<string> paramNamesForCall = [];

        foreach (IParameterSymbol param in Symbol.Parameters)
        {
            string modifier = param.RefKind switch
            {
                RefKind.Ref => "ref ",
                RefKind.Out => "out ",
                RefKind.In => "in ",
                _ => ""
            };
            string modifier2 = modifier;
            if (param.IsParams)
                modifier = "params ";

            // 原始类型（始终使用 FullyQualifiedFormat）
            string originalType = param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            paramDeclarations.Add($"{modifier}{originalType} {param.Name}");

            // 使用扩展方法：非 public 类型替换为 object，类型参数保留原名
            string fallbackType = param.Type.ToDisplayStringWithObjectForNonPublic(SymbolDisplayFormat.FullyQualifiedFormat, out _);
            paramDeclarationsWithObjectForNonPublic.Add($"{(fallbackType == "object" ? "" : modifier)}{fallbackType} {param.Name}");

            paramNames.Add(param.Name);
            paramNamesForCall.Add(modifier2 + param.Name);
        }

        ParameterDeclarations = [.. paramDeclarations];
        ParameterDeclarationsWithObjectForNonPublic = [.. paramDeclarationsWithObjectForNonPublic];
        ParameterNames = [.. paramNames];
        ParameterNamesForCall = [.. paramNamesForCall];
    }

    /// <summary>
    /// 生成方法自身的声明字符串（不包含外层类型）。
    /// 自动处理 static、abstract、virtual、override、sealed 等修饰符，
    /// 并始终添加 partial 关键字。参数列表使用 <see cref="ParameterDeclarationsWithObjectForNonPublic"/>。
    /// 示例：<c>public void MyMethod(int param);</c>
    /// </summary>
    /// <returns>方法声明字符串；如果无效则返回 <see cref="string.Empty"/>。</returns>
    public string GenerateDeclaration()
    {
        if (!Valid)
            return string.Empty;

        List<string> modifiers = [AccessibilityString];

        if (Symbol.IsStatic)
            modifiers.Add("static");

        if (Symbol.IsAbstract)
            modifiers.Add("abstract");
        else if (Symbol.IsOverride)
            modifiers.Add("override");
        else if (Symbol.IsVirtual && !Symbol.IsSealed)
            modifiers.Add("virtual");

        modifiers.Add("partial"); //始终添加partial关键字，避免问题

        string parametersString = string.Join(", ", ParameterDeclarationsWithObjectForNonPublic);

        return $"{string.Join(" ", modifiers)} {ReturnTypeString} {Symbol.Name}{TypeParametersString}({parametersString});";
    }

    /// <summary>
    /// 将方法转换为对应的委托声明。对于实例方法会自动添加一个 <c>self</c> 参数（类型为包含类型），
    /// 用于在生成的委托调用时传递实例引用。静态方法则直接使用原有参数。
    /// </summary>
    /// <param name="delegateTypeName">委托类型名称（可包含泛型占位符，但本方法会附加方法的类型参数）。</param>
    /// <param name="selfOverride">实例方法中 self 参数的名称，默认为 "self"。</param>
    /// <returns>委托声明字符串，如 <c>public delegate void MyDelegate(int x);</c></returns>
    public string GenerateDelegateDeclaration(string delegateTypeName, string selfOverride = null)
    {
        if (!Valid)
            return string.Empty;

        string delegateName = delegateTypeName + TypeParametersString; // 将类型参数附加到委托名称后

        List<string> parameters = [];

        // 实例方法：添加 self 参数
        if (!Symbol.IsStatic)
        {
            string selfType = Symbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            string selfName = selfOverride ?? "self";
            parameters.Add($"{selfType} {selfName}");
        }

        // 添加方法的原有参数
        parameters.AddRange(ParameterDeclarations);

        string parametersString = string.Join(", ", parameters);
        return $"public delegate {ReturnTypeString} {delegateName}({parametersString});";
    }
}