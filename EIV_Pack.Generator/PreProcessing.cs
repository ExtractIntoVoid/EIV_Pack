using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EIV_Pack.Generator;

internal static class PreProcessing
{

    public static bool EarlyReturn(GeneratorClass generatorClass, SourceProductionContext context, out INamedTypeSymbol? typeSymbol, out int GenerateType)
    {
        GenerateType = 0;
        TypeDeclarationSyntax syntax = generatorClass.Syntax;
        var semanticModel = generatorClass.Compilation.GetSemanticModel(syntax.SyntaxTree);

        typeSymbol = semanticModel.GetDeclaredSymbol(syntax, context.CancellationToken);
        if (typeSymbol == null)
            return false;

        // return on private
        if (typeSymbol.DeclaredAccessibility == Accessibility.Private)
            return false;

        var packableAttribute = typeSymbol.GetAttributes().FirstOrDefault(attrib => attrib.AttributeClass?.Name == "EIV_PackableAttribute");
        if (packableAttribute == null)
            return true;



        // Dont have ctor args.
        if (packableAttribute.ConstructorArguments.Length != 0)
        {
            int? GenerateTypeNullable = packableAttribute.ConstructorArguments[0].Value as int?;

            // 2 is NoGenerate
            if (GenerateTypeNullable == 2)
                return false;

            GenerateType = GenerateTypeNullable ?? 0;
        }

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

        symbols = symbols.OrderBy(GetOrder).ToList();

        return symbols;
    }

    public static int GetOrder(ISymbol symbol)
    {
        var PackOrder = symbol.GetAttributes().FirstOrDefault(attrib => attrib.AttributeClass?.Name == "EIV_PackOrderAttribute");
        if (PackOrder == null)
            return int.MaxValue;

        var ctorArg = PackOrder.ConstructorArguments[0];
        int? order = ctorArg.Value as int?;
        if (!order.HasValue)
            return int.MaxValue;

        return order.Value;
    }

    public static bool HasOrder(ISymbol symbol)
    {
        var PackOrder = symbol.GetAttributes().FirstOrDefault(attrib => attrib.AttributeClass?.Name == "EIV_PackOrderAttribute");
        if (PackOrder == null)
            return false;

        var ctorArg = PackOrder.ConstructorArguments[0];
        int? order = ctorArg.Value as int?;
        return order.HasValue;
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

        if (typeSymbol.GetMembers().OfType<IMethodSymbol>().Any(x => x.Name == "RegisterFormatter" && x.IsStatic))
        {
            return true;
        }

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
        List<ISymbol> initOnly = [];

        foreach (var symbol in symbols)
        {
            if (symbol is IFieldSymbol fieldSymbol && fieldSymbol.IsRequired)
            {
                initOnly.Add(fieldSymbol);
                continue;
            }

            if (symbol is IPropertySymbol propertySymbol)
            {
                if (propertySymbol.IsRequired)
                {
                    initOnly.Add(symbol);
                    continue;
                }

                if (propertySymbol.SetMethod != null && propertySymbol.SetMethod.IsInitOnly)
                {
                    initOnly.Add(symbol);
                    continue;
                }
            }
        }

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
