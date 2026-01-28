using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace EIVPack.Generator;

internal static class PackGenerator
{
    internal static void Generate(TypeDeclarationSyntax syntax, Compilation compilation, SourceProductionContext context)
    {
        var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);

        var typeSymbol = semanticModel.GetDeclaredSymbol(syntax, context.CancellationToken);
        if (typeSymbol == null)
        {
            return;
        }

        // verify is partial
        if (!syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MustBePartial, syntax.Identifier.GetLocation(), typeSymbol.Name));
            return;
        }
    }
}
