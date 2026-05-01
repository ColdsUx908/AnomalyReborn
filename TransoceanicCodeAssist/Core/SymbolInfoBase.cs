// Developed by ColdsUx

namespace TransoceanicCodeAssist;

internal abstract class SymbolInfoBase<TSymbol> where TSymbol : ISymbol
{
    public TSymbol Symbol { get; private set; }

    public string Namespace { get; protected set; }
    public Accessibility Accessibility { get; protected set; }
    public string ContainingType { get; protected set; }
    public string MemberType { get; protected set; }
    public string Name { get; protected set; }
    public AttributeData[] Attributes { get; protected set; }

    public bool Valid { get; protected set; }

    public string AccessibilityString => Accessibility.ToString().ToLowerInvariant();

    protected SymbolInfoBase(TSymbol symbol) => Symbol = symbol;

    public bool TryGetAttribute(string attributeFullName, out AttributeData data) => SourceGeneratorHelper.TryGetAttribute(attributeFullName, Attributes, out data);

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

internal class TypeSymbolInfoBase : SymbolInfoBase<INamedTypeSymbol>
{
    public TypeSymbolInfoBase(INamedTypeSymbol symbol) : base(symbol)
    {
        InitializeCore(
            symbolNamespace: symbol.ContainingNamespace?.ToDisplayString(),
            containingType: symbol.ContainingType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            memberType: symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
    }

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
/// 封装字段符号的信息。
/// </summary>
internal class FieldSymbolInfoBase : SymbolInfoBase<IFieldSymbol>
{
    public FieldSymbolInfoBase(IFieldSymbol symbol) : base(symbol)
    {
        InitializeCore(
            symbolNamespace: symbol.ContainingType.ContainingNamespace?.ToDisplayString(),
            containingType: symbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            memberType: symbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
    }

    /// <summary>
    /// 生成字段自身的声明字符串（不考虑包含类型）。
    /// 示例：private int _field;
    /// </summary>
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

internal class PropertySymbolInfoBase : SymbolInfoBase<IPropertySymbol>
{
    /// <summary>获取属性的 get 访问器方法（可能为 null）。</summary>
    public IMethodSymbol GetMethod { get; }
    /// <summary>获取属性的 set 访问器方法（可能为 null）。</summary>
    public IMethodSymbol SetMethod { get; }

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
    /// 生成属性自身的声明字符串（不考虑包含类型）。
    /// 示例：public int MyProperty { get; set; }
    /// </summary>
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

internal class MethodSymbolInfoBase : SymbolInfoBase<IMethodSymbol>
{
    /// <summary>返回类型的字符串表示。</summary>
    public string ReturnTypeString { get; private set; }

    /// <summary>类型参数列表的字符串表示（例如 "&lt;T&gt;"），无泛型时为空字符串。</summary>
    public string TypeParametersString { get; private set; }

    /// <summary>每个参数的完整声明字符串数组（例如 ["int x", "ref string s"]）。</summary>
    public string[] ParameterDeclarations { get; private set; }
    /// <summary>每个参数的完整声明字符串数组，但在类型不可访问时使用 <see langword="object"/>。</summary>
    public string[] ParameterDeclarationsWithObjectForNonPublic { get; private set; }
    /// <summary>每个参数的标识符字符串数组（例如 ["x", "s"]）。</summary>
    public string[] ParameterNames { get; private set; }
    /// <summary>每个参数的为调用准备的标识符字符串数组（例如 ["x", "ref s"]）。</summary>
    public string[] ParameterNamesForCall { get; private set; }

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
            if (param.IsParams)
                modifier = "params ";

            // 原始类型（始终使用 FullyQualifiedFormat）
            string originalType = param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            paramDeclarations.Add($"{modifier}{originalType} {param.Name}");

            // 使用扩展方法：非 public 类型替换为 object，类型参数保留原名
            string fallbackType = param.Type.ToDisplayStringWithObjectForNonPublic(SymbolDisplayFormat.FullyQualifiedFormat, out _);
            paramDeclarationsWithObjectForNonPublic.Add($"{modifier}{fallbackType} {param.Name}");

            paramNames.Add(param.Name);
            paramNamesForCall.Add(modifier + param.Name);
        }

        ParameterDeclarations = [.. paramDeclarations];
        ParameterDeclarationsWithObjectForNonPublic = [.. paramDeclarationsWithObjectForNonPublic];
        ParameterNames = [.. paramNames];
        ParameterNamesForCall = [.. paramNamesForCall];
    }

    /// <summary>
    /// 生成方法自身的声明字符串（不考虑包含类型）。
    /// 示例：public void MyMethod(int param);
    /// </summary>
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
    /// 将方法转换为对应的委托声明。
    /// </summary>
    /// <param name="delegateTypeName">委托类型名称（可包含泛型占位符，但本方法会附加方法的类型参数）</param>
    /// <param name="selfOverride">实例方法中 self 参数的名称，默认 "self"</param>
    /// <returns>委托声明字符串，如 "delegate void MyDelegate(int x);"</returns>
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