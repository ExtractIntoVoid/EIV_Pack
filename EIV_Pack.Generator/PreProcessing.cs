using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EIV_Pack.Generator;

internal static class PreProcessing
{

    public static bool EarlyReturn(GeneratorClass generatorClass, SourceProductionContext context, out INamedTypeSymbol? typeSymbol)
    {
        TypeDeclarationSyntax syntax = generatorClass.Syntax;
        var semanticModel = generatorClass.Compilation.GetSemanticModel(syntax.SyntaxTree);

        typeSymbol = semanticModel.GetDeclaredSymbol(syntax, context.CancellationToken);
        if (typeSymbol == null)
            return false;

        // return on private
        if (typeSymbol.DeclaredAccessibility == Accessibility.Private)
            return false;

        // verify is partial
        if (!syntax.Modifiers.Any(static m => m.IsKind(SyntaxKind.PartialKeyword)))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MustBePartial, syntax.Identifier.GetLocation(), typeSymbol.Name));
            return false;
        }

        return true;
    }

    public static List<ISymbol> GetSerializables(INamedTypeSymbol symbol, SourceProductionContext context, TypeDeclarationSyntax syntax)
    {
        List<ISymbol> symbols = GetFieldOrParams(symbol);
        RemoveIgnored(ref symbols, symbol);

        if (symbols.Count == 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.NoFieldOrProperties, syntax.Identifier.GetLocation(), symbol.Name));
            return [];
        }

        return symbols;
    }

    static List<ISymbol> GetFieldOrParams(INamedTypeSymbol symbol)
    {
        List<ISymbol> symbols = [];
        if (symbol.BaseType != null)
        {
            symbols.AddRange(GetFieldOrParams(symbol.BaseType));
        }

        var fields = symbol.GetMembers().OfType<IFieldSymbol>().Where(
            f => !f.IsStatic
            && !f.IsAbstract
            && !f.IsImplicitlyDeclared
            && !f.IsReadOnly
            && f.DeclaredAccessibility != Accessibility.Private
            && f.Name != "EqualityContract"
            && !f.GetAttributes().Any(att => att.AttributeClass?.Name == "EIV_PackIgnoreAttribute")
            ).ToList();

        symbols.AddRange(fields);

        var properties = symbol.GetMembers().OfType<IPropertySymbol>().Where(
            p => !p.IsStatic
            && !p.IsReadOnly
            && p.DeclaredAccessibility != Accessibility.Private
            && p.GetMethod != null && p.GetMethod.DeclaredAccessibility != Accessibility.Private
            && p.SetMethod != null && p.SetMethod.DeclaredAccessibility != Accessibility.Private
            && !p.GetAttributes().Any(att => att.AttributeClass?.Name == "EIV_PackIgnoreAttribute")
            );

        symbols.AddRange(properties);
        return symbols;
    }

    public static bool HasRegisterFormatter(INamedTypeSymbol typeSymbol)
    {
        bool ret = false;

        if (typeSymbol.BaseType != null)
        {
            ret |= typeSymbol.BaseType.GetAttributes().Any(att => att.AttributeClass?.Name == "EIV_PackableAttribute");
            if (!ret)
                ret |= HasRegisterFormatter(typeSymbol.BaseType);
        }

        return ret;
    }

    public static List<ISymbol> InitOnly(ref List<ISymbol> symbols)
    {
        var initOnly = symbols.Where(x => x is IPropertySymbol property && property.SetMethod != null && property.SetMethod.IsInitOnly).ToList();
        initOnly.AddRange(symbols.Where(x => x is IFieldSymbol fieldSymbol && fieldSymbol.IsRequired));
        initOnly.AddRange(symbols.Where(x => x is IPropertySymbol property && property.IsRequired));

        return initOnly;
    }

    static void RemoveIgnored(ref List<ISymbol> symbols, INamedTypeSymbol typeSymbol)
    {
        List<string> propertiesToRemove = [];
        List<string> fieldsToIgnore = [];
        foreach (var item in typeSymbol.GetAttributes())
        {
            if (item.AttributeClass == null)
                continue;

            if (item.AttributeClass.Name == "EIV_PackIgnorePropertiesAttribute")
            {
                var ctorArg = item.ConstructorArguments[0];
                if (ctorArg.Kind == TypedConstantKind.Array && ctorArg.Values != null)
                {
                    foreach (var element in ctorArg.Values)
                    {
                        if (element.Value is string s)
                            propertiesToRemove.Add(s);
                    }
                }
                else if (ctorArg.Value is string single)
                {
                    propertiesToRemove.Add(single);
                }
            }

            if (item.AttributeClass.Name == "EIV_PackIgnoreFieldsAttribute")
            {
                var ctorArg = item.ConstructorArguments[0];
                if (ctorArg.Kind == TypedConstantKind.Array && ctorArg.Values != null)
                {
                    foreach (var element in ctorArg.Values)
                    {
                        if (element.Value is string s)
                            fieldsToIgnore.Add(s);
                    }
                }
                else if (ctorArg.Value is string single)
                {
                    fieldsToIgnore.Add(single);
                }
            }
        }

        symbols.RemoveAll(s => s.Kind == SymbolKind.Property && propertiesToRemove.Contains(s.Name));
        symbols.RemoveAll(s => s.Kind == SymbolKind.Field && fieldsToIgnore.Contains(s.Name));
    }
}
