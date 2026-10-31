namespace TransoceanicCodeAssist.Generators;

/// <summary>为指定目标类型生成 Detour 委托、默认转发及注册入口。</summary>
[Generator]
public class TypeDetourGenerator : IIncrementalGenerator
{
    private sealed class TypeDetourTargetSymbolInfo : TypeSymbolInfoBase
    {
        public const string AttributeFullName = "global::Transoceanic.Runtime.Detours.TypeDetourClassAttribute";

        public INamedTypeSymbol TargetTypeSymbol { get; }
        public List<(IMethodSymbol Symbol, bool FromProperty)> VirtualMethodSymbols { get; }
        public string[] TypeParameterNames { get; }

        public string FirstTypeParameterName => TypeParameterNames[0];

        public TypeDetourTargetSymbolInfo(INamedTypeSymbol symbol, Compilation compilation) : base(symbol)
        {
            if (!Valid || !GeneratorValidation.IsValidDeclaration(symbol, true))
            {
                Valid = false;
                return;
            }

            //原始类必须是泛型类
            if (!Symbol.IsGenericType)
            {
                Valid = false;
                return;
            }

            if (!TryGetAttribute(AttributeFullName, out AttributeData attributeData) || attributeData.ConstructorArguments.Length != 1)
            {
                Valid = false;
                return;
            }

            TypedConstant targetTypeArg = attributeData.ConstructorArguments[0];
            if (targetTypeArg.Kind != TypedConstantKind.Type)
            {
                Valid = false;
                return;
            }

            ITypeSymbol targetTypeFromAttr = (ITypeSymbol)targetTypeArg.Value;
            if (targetTypeFromAttr is null)
            {
                Valid = false;
                return;
            }

            //重新获取类型

            string fullName = targetTypeFromAttr.FullMetadataName;
            INamedTypeSymbol targetType = compilation.GetTypeByMetadataName(fullName);
            TargetTypeSymbol = targetType;

            // 目标类型无效则提前返回
            if (TargetTypeSymbol is null)
            {
                Valid = false;
                return;
            }

            //收集成员信息

            ImmutableArray<ISymbol> memberSymbols = TargetTypeSymbol.GetMembers();
            List<(IMethodSymbol Symbol, bool FromProperty)> instanceMethodSymbols = [];

            foreach (ISymbol memberSymbol in memberSymbols)
            {
                if (memberSymbol.IsImplicitlyDeclared
                    || memberSymbol.IsExtern
                    || memberSymbol.IsStatic
                    || !(memberSymbol.IsVirtual || memberSymbol.IsAbstract)
                    || memberSymbol.IsSealed
                    || !memberSymbol.Name.IsValidCSharpIdentifier
                    || memberSymbol.GetAttributes().Any(attr => attr.AttributeClass?.Name == nameof(ObsoleteAttribute)))
                {
                    continue;
                }

                switch (memberSymbol)
                {
                    case IPropertySymbol propertySymbol:
                        IMethodSymbol getter = propertySymbol.GetMethod;
                        if (getter is not null && GeneratorValidation.IsSupported(getter))
                            instanceMethodSymbols.Add((getter, true));
                        IMethodSymbol setter = propertySymbol.SetMethod;
                        if (setter is not null && GeneratorValidation.IsSupported(setter))
                            instanceMethodSymbols.Add((setter, true));
                        break;
                    case IMethodSymbol methodSymbol when methodSymbol.MethodKind == MethodKind.Ordinary && GeneratorValidation.IsSupported(methodSymbol):
                        instanceMethodSymbols.Add((methodSymbol, false));
                        break;
                    default:
                        break;
                }
            }

            VirtualMethodSymbols = instanceMethodSymbols;
            TypeParameterNames = [.. Symbol.TypeParameters.Select(p => p.Name)];
        }
    }

    /// <summary>获取写入生成源码的 GeneratedCode 特性标记。</summary>
    public static string GeneratedCodeMarker => SourceGeneratorHelper.GetGeneratedCodeMarker("TypeDetourGenerator");

    /// <summary>注册增量生成管线；从当前编译中的目标类型生成源码。</summary>
    /// <param name="context">Roslyn 提供的增量生成器初始化上下文。</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        GeneratorValidation.Register(context, "Transoceanic.Runtime.Detours.TypeDetourClassAttribute", true);
        IncrementalValueProvider<ImmutableArray<TypeDetourTargetSymbolInfo>> c = SourceGeneratorHelper.GetCollectedTypeInfos(context, (s, complition) => new TypeDetourTargetSymbolInfo(s, complition));
        context.RegisterSourceOutput(c, RegisterAction);
    }

    private static void RegisterAction(SourceProductionContext spc, ImmutableArray<TypeDetourTargetSymbolInfo> source)
    {
        SourceGeneratorHelper.AddTypeSources(spc, "TypeDetour", source, RegisterType);

    }

    private static string RegisterType(TypeDetourTargetSymbolInfo typeInfo)
    {
        StringBuilder builder = new();

        builder.AppendLine(HandleMethods(typeInfo));

        return typeInfo.GenerateDeclarationText(builder.ToString(), 0, true);
    }

    private static string HandleMethods(TypeDetourTargetSymbolInfo typeInfo)
    {
        if (typeInfo.VirtualMethodSymbols.Count == 0)
            return "// NO Methods";

        StringBuilder typeBuilder = new();
        StringBuilder applyMethodBuilder = new();

        bool first = true;

        foreach ((IMethodSymbol methodSymbol, bool fromProperty) in typeInfo.VirtualMethodSymbols)
        {
            StringBuilder localBuilder = new();

            if (first)
                first = false;
            else
                localBuilder.AppendLine();

            string name = methodSymbol.Name;

            bool overloaded = typeInfo.VirtualMethodSymbols.Count(candidate => candidate.Symbol.Name == name) > 1;
            string suffix = overloaded ? "__" + SourceGeneratorHelper.GetSignatureIdentifier(methodSymbol) : "";
            string delegateTypeName = $"Orig_{name}{suffix}";
            string methodName = $"Detour_{name}{suffix}";

            MethodSymbolInfoBase methodSymbolInfo = new(methodSymbol);

            string selfName = SourceGeneratorHelper.GetExtraParameterName(methodSymbol, "self");
            string origName = SourceGeneratorHelper.GetExtraParameterName(methodSymbol, "orig");
            string parameterDeclarationString = string.Join(", ", [$"{typeInfo.FirstTypeParameterName} {selfName}", .. methodSymbolInfo.ParameterDeclarationsWithObjectForNonPublic]);
            string parameterNameString = string.Join(", ", [selfName, .. methodSymbolInfo.ParameterNamesForCall]);
            localBuilder.Append($$"""
                // {{name}}
                {{GeneratedCodeMarker}}
                {{SourceGeneratorHelper.NeverBrowsableIdentifier}}
                public unsafe delegate {{methodSymbolInfo.ReturnTypeString}} {{delegateTypeName}}({{parameterDeclarationString}});
                /// <inheritdoc cref="{{(fromProperty ? methodSymbol.AssociatedSymbol : methodSymbol).GetDocumentationCommentId()}}"/>
                {{GeneratedCodeMarker}}
                public unsafe virtual {{methodSymbolInfo.ReturnTypeString}} {{methodName}}({{delegateTypeName}} {{origName}}, {{parameterDeclarationString}}) => {{origName}}({{parameterNameString}});
                """);

            typeBuilder.AppendLine(localBuilder.ToString());
            applyMethodBuilder.AppendLine($"ApplySingleDetour({methodName});");
        }

        typeBuilder.Append(
        $$"""

            {{GeneratedCodeMarker}}
            public unsafe override void ApplyDetour()
            {
                base.ApplyDetour();
            {{applyMethodBuilder.ToString().Trim().AddIndent(1)}}

                ApplyExtraDetour();
            }

            {{GeneratedCodeMarker}}
            {{SourceGeneratorHelper.NeverBrowsableIdentifier}}
            unsafe partial void ApplyExtraDetour();
            """);

        return typeBuilder.ToString();
    }
}
