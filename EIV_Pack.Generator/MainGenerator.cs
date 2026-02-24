using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EIV_Pack.Generator;

[Generator(LanguageNames.CSharp)]
public class MainGenerator : IIncrementalGenerator
{
    public const string AttributeFullName = "EIV_Pack.EIV_PackableAttribute";
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var typeDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName(
               AttributeFullName,
               predicate: static (node, token) =>
               {
                   // search [EIV_Packable] class or struct or record
                   return node is ClassDeclarationSyntax
                                or StructDeclarationSyntax
                                or RecordDeclarationSyntax;
               },
               transform: static (context, token) =>
               {
                   return (TypeDeclarationSyntax)context.TargetNode;
               })
               .WithTrackingName("EIV_Pack.EIV_Packable.1_ForAttributeEIV_PackableAttribute");

        var options = context.ParseOptionsProvider.Select((option, token) =>
        {
            return option.PreprocessorSymbolNames.Contains("NET8_0_OR_GREATER");
        });

        context.RegisterSourceOutput(typeDeclarations.Combine(context.CompilationProvider).Combine(options), static (context, source) =>
        {
            GeneratorClass generatorClass = new()
            { 
                Syntax = source.Left.Left,
                Compilation = source.Left.Right,
                IsNet8OrGreater = source.Right,
            };
            PackGenerator.Generate(generatorClass, context);
        });
    }
}

public class GeneratorClass
{
    public TypeDeclarationSyntax Syntax { get; set; } = default!;
    public Compilation Compilation { get; set; } = default!;
    public bool IsNet8OrGreater { get; set; } = default!;
}
