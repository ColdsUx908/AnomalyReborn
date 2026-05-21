// Developed by ColdsUx

namespace TransoceanicCodeAssist.SourceGenerators;

[Generator]
public class TypeDetourGenerator : IIncrementalGenerator
{
    private sealed class TypeDetourTargetSymbolInfo : TypeSymbolInfoBase
    {
        public const string AttributeFullName = SourceGeneratorHelper.ExternalAttributePrefix + "TypeDetourClassAttribute";

        public INamedTypeSymbol TargetTypeSymbol { get; }
        public List<(IMethodSymbol Symbol, bool FromProperty)> VirtualMethodSymbols { get; }
        public string[] TypeParameterNames { get; }

        public string FirstTypeParameterName => TypeParameterNames[0];

        public TypeDetourTargetSymbolInfo(INamedTypeSymbol symbol, Compilation compilation) : base(symbol)
        {
            if (!Valid)
                return;

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
                        if (getter is not null)
                            instanceMethodSymbols.Add((getter, true));
                        IMethodSymbol setter = propertySymbol.SetMethod;
                        if (setter is not null)
                            instanceMethodSymbols.Add((setter, true));
                        break;
                    case IMethodSymbol methodSymbol when methodSymbol.MethodKind == MethodKind.Ordinary && !methodSymbol.IsGenericMethod:
                        instanceMethodSymbols.Add((methodSymbol, false));
                        break;
                    default:
                        break;
                }
            }

            VirtualMethodSymbols = [.. instanceMethodSymbols.GroupBy(m => m.Symbol.Name).Where(g => g.Count() == 1).Select(g => g.First())]; //不处理重载方法
            TypeParameterNames = [.. Symbol.TypeParameters.Select(p => p.Name)];
        }
    }

    public static string GeneratedCodeMarker => SourceGeneratorHelper.GetGeneratedCodeMarker("TypeDetourGenerator");

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<TypeDetourTargetSymbolInfo>> c = SourceGeneratorHelper.GetCollectedTypeInfos(context, (s, complition) => new TypeDetourTargetSymbolInfo(s, complition));
        context.RegisterSourceOutput(c, RegisterAction);
    }

    private static void RegisterAction(SourceProductionContext spc, ImmutableArray<TypeDetourTargetSymbolInfo> source)
    {
        string fileName = "TypeDetour.g.cs";

        StringBuilder builder = new();

        builder.AppendLine(SourceGeneratorHelper.FileHeader);

        for (int i = 0; i < source.Length; i++)
        {
            TypeDetourTargetSymbolInfo typeInfo = source[i];
            if (i > 0)
                builder.AppendLine();
            builder.AppendLine(RegisterType(typeInfo));
        }

        spc.AddSource(fileName, builder.ToString());
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
            string memberName = fromProperty ? name[4..] : name;

            List<string> paramTypeNames = [..
                from IParameterSymbol param in methodSymbol.Parameters
                let typeName = param.Type.Name
                    .Replace(".", "_")
                    .Replace("<", "_")
                    .Replace(">", "_")
                    .Replace(",", "_")
                    .Replace(" ", "")
                    + (param.RefKind != RefKind.None ? "ByRef" : "")
                select typeName];

            string delegateTypeName = $"Orig_{name}";
            string methodName = $"Detour_{name}";

            MethodSymbolInfoBase methodSymbolInfo = new(methodSymbol);

            string parameterDeclarationString = string.Join(", ", [$"{typeInfo.FirstTypeParameterName} self", .. methodSymbolInfo.ParameterDeclarationsWithObjectForNonPublic]);
            string parameterNameString = string.Join(", ", ["self", .. methodSymbolInfo.ParameterNamesForCall]);
            localBuilder.Append($$"""
                // {{name}}
                {{GeneratedCodeMarker}}
                {{SourceGeneratorHelper.NeverBrowsableIdentifier}}
                public delegate {{methodSymbolInfo.ReturnTypeString}} {{delegateTypeName}}({{parameterDeclarationString}});
                /// <inheritdoc cref="{{typeInfo.TargetTypeSymbol.ToDisplayStringBetter()}}.{{memberName}}"/>
                {{GeneratedCodeMarker}}
                public virtual {{methodSymbolInfo.ReturnTypeString}} {{methodName}}({{delegateTypeName}} orig, {{parameterDeclarationString}}) => orig({{parameterNameString}});
                """);

            typeBuilder.AppendLine(localBuilder.ToString());
            applyMethodBuilder.AppendLine($"ApplySingleDetour({methodName});");
        }

        typeBuilder.Append(
            $$"""

            {{GeneratedCodeMarker}}
            public override void ApplyDetour()
            {
                base.ApplyDetour();
            {{applyMethodBuilder.ToString().Trim().AddIndent(1)}}

                ApplyExtraDetour();
            }

            {{GeneratedCodeMarker}}
            {{SourceGeneratorHelper.NeverBrowsableIdentifier}}
            partial void ApplyExtraDetour();
            """);

        return typeBuilder.ToString();
    }
}