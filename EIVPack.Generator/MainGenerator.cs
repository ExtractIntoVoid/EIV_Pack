using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace EIVPack.Generator;

[Generator(LanguageNames.CSharp)]
public class MainGenerator : IIncrementalGenerator
{
    public const string AttributeFullName = "EIVPack.EIVPackableAttribute";
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var typeDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName(
               AttributeFullName,
               predicate: static (node, token) =>
               {
                   // search [EIVPackable] class or struct or interface or record
                   return (node is ClassDeclarationSyntax
                                or StructDeclarationSyntax
                                or RecordDeclarationSyntax
                                or InterfaceDeclarationSyntax);
               },
               transform: static (context, token) =>
               {
                   return (TypeDeclarationSyntax)context.TargetNode;
               })
               .WithTrackingName("EIVPack.EIVPackable.1_ForAttributeEIVPackableAttribute");

        context.RegisterSourceOutput(typeDeclarations.Combine(context.CompilationProvider), static (context, source) =>
        {
            PackGenerator.Generate(source.Left, source.Right, context);
        });
    }
}
