namespace TransoceanicCodeAssist.Generators;

/// <summary>从编译实际引用的 tML API 生成行为钩子与 Handler 分发；手写成员始终优先。</summary>
[Generator]
public sealed class BehaviorGenerator : IIncrementalGenerator
{
    private static readonly string GeneratedCodeMarker = SourceGeneratorHelper.GetGeneratedCodeMarker("BehaviorGenerator");

    private static readonly SymbolDisplayFormat SignatureFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .WithMemberOptions(SymbolDisplayMemberOptions.IncludeType | SymbolDisplayMemberOptions.IncludeParameters |
            SymbolDisplayMemberOptions.IncludeRef)
        .WithParameterOptions(SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName |
            SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeDefaultValue)
        .WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeTypeConstraints)
        .WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public void Initialize(IncrementalGeneratorInitializationContext context) =>
        context.RegisterSourceOutput(context.CompilationProvider, Generate);

    private static void Generate(SourceProductionContext context, Compilation compilation)
    {
        foreach (string entityName in new[] { "NPC", "Projectile", "Item" })
        {
            INamedTypeSymbol source = compilation.GetTypeByMetadataName("Terraria.ModLoader.Global" + entityName);
            INamedTypeSymbol entity = compilation.GetTypeByMetadataName("Terraria." + entityName);
            if (source is null || entity is null)
                continue;

            foreach (string kind in new[] { "Single", "Global" })
            {
                string name = kind + entityName + "Behavior";
                const string ns = "Transoceanic.Behaviors";
                INamedTypeSymbol target = compilation.GetTypeByMetadataName(ns + "." + name);
                if (target is null || !SymbolEqualityComparer.Default.Equals(target.ContainingAssembly, compilation.Assembly) ||
                    !target.DeclaringSyntaxReferences.All(r => r.GetSyntax(context.CancellationToken) is ClassDeclarationSyntax c &&
                        c.Modifiers.Any(SyntaxKind.PartialKeyword)))
                    continue;

                bool single = kind == "Single";
                StringBuilder code = new(SourceGeneratorHelper.FileHeader);
                code.Append("\n#nullable enable\nnamespace ").Append(ns).Append(";\n\npublic abstract partial class ").Append(name).Append("\n{\n");
                int count = 0;
                // Only declared hooks: GlobalType/ModType members manage registration, cloning and lifetime.
                foreach (IMethodSymbol method in source.GetMembers().OfType<IMethodSymbol>())
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                    if (method.MethodKind != MethodKind.Ordinary || method.IsStatic || !method.IsVirtual || method.IsSealed ||
                        method.IsAbstract || method.DeclaredAccessibility != Accessibility.Public ||
                        method.Name.StartsWith("Can", StringComparison.Ordinal) ||
                        method.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "System.ObsoleteAttribute") ||
                        method.Parameters.Length == 0 ||
                        method.Parameters[0].RefKind != RefKind.None ||
                        !SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, entity))
                        continue;

                    if (HasImplementation(target, method, single))
                        continue;

                    // Roslyn renders defaults, escaped names, nested types, nullable types and constraints.
                    string declaration = method.ToDisplayString(SignatureFormat);
                    if (single)
                    {
                        // Work on the parameter list, including tuple return types and defaults containing commas.
                        MethodDeclarationSyntax syntax = (MethodDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration(declaration + ";");
                        declaration = syntax.WithParameterList(syntax.ParameterList.WithParameters(
                            syntax.ParameterList.Parameters.RemoveAt(0))).WithSemicolonToken(default)
                            .NormalizeWhitespace().ToFullString();
                    }
                    string arguments = string.Join(", ", method.Parameters.Select((p, i) =>
                        single && i == 0 ? "this._Entity" : Argument(p)));
                    string typeArguments = method.TypeParameters.Length == 0 ? "" :
                        "<" + string.Join(", ", method.TypeParameters.Select(p => Escape(p.Name))) + ">";
                    code.Append("    /// <inheritdoc cref=\"").Append(source.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                        .Append('.').Append(method.Name).Append("\"/>\n    ").Append(GeneratedCodeMarker)
                        .Append("\n    public virtual ").Append(declaration)
                        .Append(" => ").Append(method.ReturnsByRef || method.ReturnsByRefReadonly ? "ref " : "")
                        .Append("__BehaviorDefaults.DefaultHooks.").Append(Escape(method.Name)).Append(typeArguments)
                        .Append('(').Append(arguments).Append(");\n\n");
                    count++;
                }
                if (count == 0)
                    continue;
                code.Append("    ").Append(GeneratedCodeMarker)
                    .Append("\n    [global::Terraria.ModLoader.Autoload(false)]\n    private sealed class __BehaviorDefaults : ")
                    .Append(source.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                    .Append("\n    {\n        ").Append(GeneratedCodeMarker)
                    .Append("\n        internal static readonly __BehaviorDefaults DefaultHooks = new();\n");
                for (INamedTypeSymbol type = source; type is not null; type = type.BaseType)
                {
                    IPropertySymbol conditional = type.GetMembers("ConditionallyAppliesToEntities").OfType<IPropertySymbol>().FirstOrDefault();
                    if (conditional is null)
                        continue;
                    if (conditional.IsAbstract)
                        code.Append("        ").Append(GeneratedCodeMarker)
                            .Append("\n        public override bool ConditionallyAppliesToEntities => false;\n");
                    break;
                }
                code.Append("    }\n}\n");
                context.AddSource(name + ".Hooks.g.cs", code.ToString());
            }
        }
        GenerateHandlers(context, compilation);
    }

    // The same symbol model drives both sides of the contract. Special aggregations and
    // network hooks stay handwritten; an explicit override always takes precedence.
    private static void GenerateHandlers(SourceProductionContext context, Compilation compilation)
    {
        foreach (string entityName in new[] { "NPC", "Projectile", "Item", "Player" })
        foreach (bool single in entityName == "Player" ? new[] { false } : [false, true])
        {
            bool player = entityName == "Player";
            string behaviorName = player ? "PlayerBehavior" : (single ? "Single" : "Global") + entityName + "Behavior";
            string handlerName = behaviorName + "Handler";
            INamedTypeSymbol source = compilation.GetTypeByMetadataName("Terraria.ModLoader." + (player ? "ModPlayer" : "Global" + entityName));
            INamedTypeSymbol entity = compilation.GetTypeByMetadataName("Terraria." + entityName);
            INamedTypeSymbol behavior = compilation.GetTypeByMetadataName("Transoceanic.Behaviors." + behaviorName);
            INamedTypeSymbol handler = compilation.GetTypeByMetadataName("Transoceanic.Behaviors.Dispatching." + handlerName + (single ? "`1" : ""));
            if (source is null || entity is null || behavior is null || handler is null ||
                !SymbolEqualityComparer.Default.Equals(handler.ContainingAssembly, compilation.Assembly) ||
                !handler.DeclaringSyntaxReferences.All(r => r.GetSyntax(context.CancellationToken) is ClassDeclarationSyntax c && c.Modifiers.Any(SyntaxKind.PartialKeyword)))
                continue;

            StringBuilder code = new(SourceGeneratorHelper.FileHeader);
            code.Append("\n#nullable enable\nnamespace Transoceanic.Behaviors.Dispatching;\n\npublic ")
                .Append(single ? "abstract" : "sealed").Append(" partial class ").Append(handlerName);
            if (single)
                code.Append('<').Append(Escape(handler.TypeParameters[0].Name)).Append('>');
            code.Append("\n{\n");
            List<IMethodSymbol> hooks = source.GetMembers().OfType<IMethodSymbol>().ToList();
            if (!player)
                for (INamedTypeSymbol type = source.BaseType; type is not null; type = type.BaseType)
                    hooks.AddRange(type.GetMembers("SetDefaults").OfType<IMethodSymbol>());
            int count = 0;
            foreach (IMethodSymbol method in hooks)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                if (method.MethodKind != MethodKind.Ordinary || method.IsStatic || method.IsSealed || method.IsAbstract ||
                    (!method.IsVirtual && !method.IsOverride) || method.DeclaredAccessibility != Accessibility.Public ||
                    method.ReturnsByRef || method.ReturnsByRefReadonly ||
                    method.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "System.ObsoleteAttribute") ||
                    method.Name is "SendExtraAI" or "ReceiveExtraAI" or "NetSend" or "NetReceive" or
                        "CopyClientState" or "SyncPlayer" or "SendClientChanges" ||
                    handler.GetMembers(method.Name).OfType<IMethodSymbol>().Any(m => SameSignature(m, method, 0)))
                    continue;

                bool entityHook = !player && method.Parameters.Length > 0 && method.Parameters[0].RefKind == RefKind.None &&
                    SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, entity);
                // Predicted generated hooks do not exist in the input compilation yet.
                bool generatedBehaviorHook = entityHook && !method.Name.StartsWith("Can", StringComparison.Ordinal) &&
                    (SymbolEqualityComparer.Default.Equals(method.ContainingType, source) || method.Name == "SetDefaults");
                if ((single && !entityHook) || (!generatedBehaviorHook && !HasImplementation(behavior, method, single)))
                    continue;
                if (!single && (method.Parameters.Any(p => p.RefKind == RefKind.Out) ||
                    (!method.ReturnsVoid && method.ReturnType.SpecialType != SpecialType.System_Boolean &&
                     method.ReturnType.SpecialType != SpecialType.System_Single &&
                     method.ReturnType.SpecialType != SpecialType.System_Double &&
                     !(method.ReturnType is INamedTypeSymbol nullable && nullable.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T) &&
                     !(method.ReturnType.IsReferenceType && method.ReturnType.SpecialType != SpecialType.System_String &&
                       method.ReturnType.SpecialType != SpecialType.System_Collections_IEnumerable &&
                       !method.ReturnType.AllInterfaces.Any(i => i.SpecialType == SpecialType.System_Collections_IEnumerable)))))
                    continue;
                if (!single && method.ReturnType.SpecialType is SpecialType.System_Single or SpecialType.System_Double &&
                    !method.Name.EndsWith("Multiplier", StringComparison.Ordinal))
                    continue;

                string generic = method.Arity == 0 ? "" : "<" + string.Join(", ", method.TypeParameters.Select(p => Escape(p.Name))) + ">";
                string args = string.Join(", ", method.Parameters.Select(Argument));
                string behaviorArgs = string.Join(", ", method.Parameters.Skip(single ? 1 : 0).Select(Argument));
                string call = "__behavior." + Escape(method.Name) + generic + "(" + behaviorArgs + ")";
                string fallback = "base." + Escape(method.Name) + generic + "(" + args + ")";
                code.Append("    ").Append(GeneratedCodeMarker).Append("\n    public override ")
                    .Append(method.ToDisplayString(SignatureFormat)).Append("\n    {\n");
                if (single)
                {
                    code.Append("        if (TryGetBehavior(").Append(Escape(method.Parameters[0].Name)).Append(", out var __behavior))\n");
                    if (method.ReturnsVoid)
                        code.Append("            ").Append(call).Append(";\n        else\n            ").Append(fallback).Append(";\n");
                    else
                        code.Append("            return ").Append(call).Append(";\n        return ").Append(fallback).Append(";\n");
                }
                else
                {
                    string enumeration = player ? "BehaviorSet.Enumerate(Player)" : "BehaviorSet.Enumerate()";
                    if (method.ReturnsVoid)
                        code.Append("        foreach (var __behavior in ").Append(enumeration).Append(")\n            ").Append(call).Append(";\n");
                    else if (method.ReturnType.SpecialType == SpecialType.System_Boolean)
                    {
                        code.Append("        bool __identity = ").Append(fallback).Append(";\n        bool __result = __identity;\n")
                            .Append("        foreach (var __behavior in ").Append(enumeration).Append(")\n        {\n            bool __value = ").Append(call)
                            .Append(";\n            __result = __identity ? __result & __value : __result | __value;\n        }\n        return __result;\n");
                    }
                    else if (method.ReturnType.SpecialType is SpecialType.System_Single or SpecialType.System_Double)
                        code.Append("        var __result = ").Append(fallback).Append(";\n        foreach (var __behavior in ").Append(enumeration)
                            .Append(")\n            __result *= ").Append(call).Append(";\n        return __result;\n");
                    else
                        code.Append("        foreach (var __behavior in ").Append(enumeration).Append(")\n        {\n            var __result = ")
                            .Append(call).Append(";\n            if (__result is not null)\n                return __result;\n        }\n        return ").Append(fallback).Append(";\n");
                }
                code.Append("    }\n\n");
                count++;
            }
            code.Append("}\n");
            if (count > 0)
                context.AddSource(handlerName + ".Dispatch.g.cs", code.ToString());
        }
    }

    private static bool SameSignature(IMethodSymbol candidate, IMethodSymbol method, int offset)
    {
        if (candidate.Arity != method.Arity || candidate.Parameters.Length != method.Parameters.Length - offset)
            return false;
        IMethodSymbol comparable = candidate.Arity == 0 ? candidate : candidate.Construct(method.TypeParameters.Cast<ITypeSymbol>().ToArray());
        return comparable.Parameters.Select((p, i) => (p.RefKind == RefKind.None) == (method.Parameters[i + offset].RefKind == RefKind.None) &&
            SymbolEqualityComparer.Default.Equals(p.Type, method.Parameters[i + offset].Type)).All(equal => equal);
    }

    private static bool HasImplementation(INamedTypeSymbol target, IMethodSymbol method, bool single)
    {
        int offset = single ? 1 : 0;
        for (INamedTypeSymbol type = target; type is not null; type = type.BaseType)
            foreach (IMethodSymbol candidate in type.GetMembers(method.Name).OfType<IMethodSymbol>())
            {
                if (SameSignature(candidate, method, offset))
                    return true;
            }
        return false;
    }

    private static string Escape(string name) => SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ||
        SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None ? "@" + name : name;

    private static string Argument(IParameterSymbol parameter) => (parameter.RefKind switch
    {
        RefKind.Ref => "ref ",
        RefKind.Out => "out ",
        RefKind.In => "in ",
        RefKind.RefReadOnlyParameter => "in ",
        _ => ""
    }) + Escape(parameter.Name);
}
