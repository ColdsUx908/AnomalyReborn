namespace TransoceanicCodeAssist.Shared;

internal static class GeneratorValidation
{
    private static readonly DiagnosticDescriptor _InvalidDeclaration = new("TOGEN001", "Invalid generator declaration",
        "'{0}' must be a partial class{1}, with partial containing types", "Transoceanic.Generation", DiagnosticSeverity.Error, true);
    private static readonly DiagnosticDescriptor _UnsupportedMember = new("TOGEN002", "Unsupported generated member",
        "'{0}' is not generated ({1}); use an explicit wrapper or hook", "Transoceanic.Generation", DiagnosticSeverity.Warning, true);

    internal static bool IsValidDeclaration(INamedTypeSymbol symbol, bool detour)
    {
        if (symbol.TypeKind != TypeKind.Class || (detour && symbol.TypeParameters.Length == 0)) return false;
        for (INamedTypeSymbol containing = symbol; containing is not null; containing = containing.ContainingType)
            if (!containing.DeclaringSyntaxReferences.All(reference => reference.GetSyntax() is TypeDeclarationSyntax declaration
                && declaration.Modifiers.Any(SyntaxKind.PartialKeyword))) return false;
        return true;
    }

    internal static bool IsSupported(IMethodSymbol method) => UnsupportedReason(method) is null;

    private static string UnsupportedReason(IMethodSymbol method)
    {
        if (method.IsGenericMethod) return "generic methods are unsupported";
        if (method.ReturnsByRef || method.ReturnsByRefReadonly) return "reference returns are unsupported";
        if (!method.ReturnType.IsPubliclyAccessible || method.Parameters.Any(parameter => !parameter.Type.IsPubliclyAccessible))
            return "the signature contains inaccessible types";
        if (method.Parameters.Any(parameter => parameter.RefKind == RefKind.RefReadOnlyParameter))
            return "ref readonly parameters are unsupported";
        return null;
    }

    internal static void Register(IncrementalGeneratorInitializationContext context, string attributeName, bool detour)
    {
        IncrementalValuesProvider<GeneratorAttributeSyntaxContext> targets = context.SyntaxProvider.ForAttributeWithMetadataName(
            attributeName, static (node, _) => node is TypeDeclarationSyntax, static (target, _) => target);
        context.RegisterSourceOutput(targets, (output, target) =>
        {
            INamedTypeSymbol symbol = (INamedTypeSymbol)target.TargetSymbol;
            bool valid = IsValidDeclaration(symbol, detour);
            if (!valid)
            {
                output.ReportDiagnostic(Diagnostic.Create(_InvalidDeclaration, target.TargetNode.GetLocation(), symbol.Name,
                    detour ? " with at least one type parameter" : ""));
                return;
            }
            AttributeData attribute = target.Attributes[0];
            if (attribute.ConstructorArguments.FirstOrDefault().Value is not INamedTypeSymbol requested) return;
            INamedTypeSymbol source = target.SemanticModel.Compilation.WithAllMetadata().GetTypeByMetadataName(requested.FullMetadataName);
            if (source is null) return;
            foreach (IPropertySymbol property in source.GetMembers().OfType<IPropertySymbol>())
            {
                if (!detour && property.IsIndexer && property.DeclaredAccessibility != Accessibility.Public)
                    output.ReportDiagnostic(Diagnostic.Create(_UnsupportedMember, target.TargetNode.GetLocation(), property.ToDisplayString(), "indexers are unsupported"));
                if (!detour || property.IsStatic || !(property.IsVirtual || property.IsAbstract) || property.IsSealed) continue;
                foreach (IMethodSymbol accessor in new[] { property.GetMethod, property.SetMethod }.Where(method => method is not null))
                {
                    string reason = UnsupportedReason(accessor);
                    if (reason is not null)
                        output.ReportDiagnostic(Diagnostic.Create(_UnsupportedMember, target.TargetNode.GetLocation(), accessor.ToDisplayString(), reason));
                }
            }
            foreach (IMethodSymbol method in source.GetMembers().OfType<IMethodSymbol>())
            {
                if (method.MethodKind != MethodKind.Ordinary || method.IsImplicitlyDeclared || method.IsExtern) continue;
                if (detour ? method.IsStatic || !(method.IsVirtual || method.IsAbstract) || method.IsSealed
                    : method.DeclaredAccessibility == Accessibility.Public) continue;
                string reason = UnsupportedReason(method);
                if (reason is not null)
                    output.ReportDiagnostic(Diagnostic.Create(_UnsupportedMember, target.TargetNode.GetLocation(), method.ToDisplayString(), reason));
            }
        });
    }
}
