// Designed by ColdsUx

namespace TransoceanicCodeAssist;

internal static class SourceGeneratorHelper
{
    public const string NeverBrowsableIdentifier = "[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]";

    public const string ExternalAttributePrefix = "global::Transoceanic.Framework.ExternalAttributes.";

    public static IncrementalValuesProvider<TypeDeclarationSyntax> GetTypeDeclarations(IncrementalGeneratorInitializationContext context) =>
        context.SyntaxProvider.CreateSyntaxProvider(
            predicate: (node, _) => node is TypeDeclarationSyntax,
            transform: (ctx, _) => (TypeDeclarationSyntax)ctx.Node
        );

    public static bool TryGetPartialTypeSymbolInfo((TypeDeclarationSyntax Left, Compilation Right) tuple, out INamedTypeSymbol symbol)
    {
        TypeDeclarationSyntax typeDecl = tuple.Left;
        Compilation compilation = tuple.Right;

        if (typeDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            SemanticModel model = compilation.GetSemanticModel(typeDecl.SyntaxTree);
            if (model.GetDeclaredSymbol(typeDecl) is INamedTypeSymbol typeSymbol)
            {
                symbol = typeSymbol;
                return true;
            }
        }

        symbol = null;
        return false;
    }

    public static IncrementalValuesProvider<MethodDeclarationSyntax> GetMethodDeclarations(IncrementalGeneratorInitializationContext context) =>
        context.SyntaxProvider.CreateSyntaxProvider(
            predicate: (node, _) => node is MethodDeclarationSyntax,
            transform: (ctx, _) => (MethodDeclarationSyntax)ctx.Node
        );

    public static bool TryGetPartialMethodSymbolInfo((MethodDeclarationSyntax Left, Compilation Right) tuple, out IMethodSymbol symbol)
    {
        MethodDeclarationSyntax methodDecl = tuple.Left;
        Compilation compilation = tuple.Right;

        if (methodDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            SemanticModel model = compilation.GetSemanticModel(methodDecl.SyntaxTree);
            if (model.GetDeclaredSymbol(methodDecl) is IMethodSymbol methodSymbol)
            {
                symbol = methodSymbol;
                return true;
            }
        }

        symbol = null;
        return false;
    }

    public static IncrementalValuesProvider<PropertyDeclarationSyntax> GetPropertyDeclarations(IncrementalGeneratorInitializationContext context) =>
    context.SyntaxProvider.CreateSyntaxProvider(
        predicate: (node, _) => node is PropertyDeclarationSyntax,
        transform: (ctx, _) => (PropertyDeclarationSyntax)ctx.Node
    );

    public static bool TryGetPartialPropertySymbolInfo((PropertyDeclarationSyntax Left, Compilation Right) tuple, out IPropertySymbol symbol)
    {
        PropertyDeclarationSyntax property = tuple.Left;
        Compilation compilation = tuple.Right;

        if (property.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            SemanticModel model = compilation.GetSemanticModel(property.SyntaxTree);
            if (model.GetDeclaredSymbol(property) is IPropertySymbol propertySymbol)
            {
                symbol = propertySymbol;
                return true;
            }
        }

        symbol = null;
        return false;
    }
    public static IncrementalValuesProvider<TInfo> GetTypeInfos<TInfo>(IncrementalGeneratorInitializationContext context, Func<INamedTypeSymbol, Compilation, TInfo> factory) where TInfo : TypeSymbolInfoBase
    {
        if (factory is null)
            return default;

        return GetTypeDeclarations(context).Combine(context.CompilationProvider)
            .Select((tuple, _) =>
            {
                if (!TryGetPartialTypeSymbolInfo(tuple, out INamedTypeSymbol symbol))
                    return null;
                TInfo info = factory(symbol, tuple.Right.WithAllMetadata());
                if (!info.Valid)
                    return null;
                return info;
            }).Where(info => info is not null);
    }

    public static IncrementalValueProvider<ImmutableArray<TInfo>> GetCollectedTypeInfos<TInfo>(IncrementalGeneratorInitializationContext context, Func<INamedTypeSymbol, Compilation, TInfo> factory) where TInfo : TypeSymbolInfoBase =>
        GetTypeInfos(context, factory).Collect();

    public static IncrementalValuesProvider<TInfo> GetMethodInfos<TInfo>(IncrementalGeneratorInitializationContext context, Func<IMethodSymbol, Compilation, TInfo> factory) where TInfo : MethodSymbolInfoBase
    {
        if (factory is null)
            return default;

        return GetMethodDeclarations(context).Combine(context.CompilationProvider)
            .Select((tuple, _) =>
            {
                if (!TryGetPartialMethodSymbolInfo(tuple, out IMethodSymbol symbol))
                    return null;
                TInfo info = factory(symbol, tuple.Right.WithAllMetadata());
                if (!info.Valid)
                    return null;
                return info;
            }).Where(info => info is not null);
    }

    public static IncrementalValueProvider<ImmutableArray<TInfo>> GetCollectedMethodInfos<TInfo>(IncrementalGeneratorInitializationContext context, Func<IMethodSymbol, Compilation, TInfo> factory) where TInfo : MethodSymbolInfoBase =>
        GetMethodInfos(context, factory).Collect();

    public static IncrementalValuesProvider<TInfo> GetPropertyInfos<TInfo>(IncrementalGeneratorInitializationContext context, Func<IPropertySymbol, Compilation, TInfo> factory) where TInfo : PropertySymbolInfoBase
    {
        if (factory is null)
            return default;

        return GetPropertyDeclarations(context).Combine(context.CompilationProvider)
            .Select((tuple, _) =>
            {
                if (!TryGetPartialPropertySymbolInfo(tuple, out IPropertySymbol symbol))
                    return null;
                TInfo info = factory(symbol, tuple.Right.WithAllMetadata());
                if (!info.Valid)
                    return null;
                return info;
            }).Where(info => info is not null);
    }

    public static IncrementalValueProvider<ImmutableArray<TInfo>> GetCollectedPropertyInfos<TInfo>(IncrementalGeneratorInitializationContext context, Func<IPropertySymbol, Compilation, TInfo> factory) where TInfo : PropertySymbolInfoBase =>
        GetPropertyInfos(context, factory).Collect();

    public static bool TryGetAttribute(string attributeFullName, AttributeData[] datas, out AttributeData data)
    {
        // 将传入的完全限定名中的嵌套类型分隔符 '+' 替换为 '.'（与 Roslyn 的显示格式一致）
        string targetFullName = attributeFullName.Replace('+', '.');

        data = datas.FirstOrDefault(a =>
        {
            if (a.AttributeClass is null)
                return false;

            // 获取特性类的完全限定名（如 "global::Namespace.Outer.InnerAttribute"）
            string attributeFullName = a.AttributeClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return attributeFullName == targetFullName;
        });

        return data is not null;
    }

    #region 扩展
    extension(Compilation compilation)
    {
        public Compilation WithAllMetadata()
        {
            if (compilation.Options is CSharpCompilationOptions originalOptions)
            {
                CSharpCompilationOptions newOptions = originalOptions.WithMetadataImportOptions(MetadataImportOptions.All);
                return compilation.WithOptions(newOptions);
            }

            return compilation;
        }
    }

    extension(ITypeSymbol typeSymbol)
    {
        public string FullMetadataName
        {
            get
            {
                if (typeSymbol.ContainingType != null)
                    return typeSymbol.ContainingType.FullMetadataName + "+" + typeSymbol.MetadataName;

                string ns = typeSymbol.ContainingNamespace?.ToString();
                if (!string.IsNullOrEmpty(ns))
                    return ns + "." + typeSymbol.MetadataName;
                return typeSymbol.MetadataName;
            }
        }

        public string ToDisplayStringWithObjectForNonPublic(SymbolDisplayFormat format, out bool nonPublic)
        {
            nonPublic = false;
            format ??= SymbolDisplayFormat.FullyQualifiedFormat;

            // 类型参数（泛型参数）总是保留原名
            if (typeSymbol is ITypeParameterSymbol)
                return typeSymbol.ToDisplayString(format);

            // 检查访问性，非 public 替换为 object
            if (typeSymbol.DeclaredAccessibility != Accessibility.Public)
            {
                nonPublic = true;
                return "object";
            }

            return typeSymbol.ToDisplayString(format);
        }
    }

    extension(string text)
    {
        public string AddIndent(int indentLevel)
        {
            const int indentSize = 4;

            string[] lines = text.Split('\n');
            if (lines.Length == 0)
                return text;

            string indent = new(' ', indentLevel * indentSize);

            for (int i = 0; i < lines.Length; i++)
            {
                // 仅当该行不是空字符串时才添加缩进
                if (!string.IsNullOrEmpty(lines[i]))
                    lines[i] = indent + lines[i];
            }

            return string.Join("\n", lines);
        }

        public bool IsValidCSharpIdentifier
        {
            get
            {
                if (string.IsNullOrEmpty(text))
                    return false;

                // 第一个字符：字母或下划线
                if (text[0] != '_' && !char.IsLetter(text[0]))
                    return false;

                // 后续字符：字母、数字或下划线
                for (int i = 1; i < text.Length; i++)
                {
                    char c = text[i];
                    if (c == '_' || char.IsLetterOrDigit(c))
                        continue;
                    return false;
                }

                return true;
            }
        }
    }
    #endregion 扩展
}