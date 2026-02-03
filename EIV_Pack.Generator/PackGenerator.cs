using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace EIV_Pack.Generator;

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
        if (!syntax.Modifiers.Any(static m => m.IsKind(SyntaxKind.PartialKeyword)))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MustBePartial, syntax.Identifier.GetLocation(), typeSymbol.Name));
            return;
        }

        // return on private
        if (typeSymbol.DeclaredAccessibility == Accessibility.Private)
            return;

        var fieldOrParamList = GetFieldOrParams(ref typeSymbol);

        if (fieldOrParamList.Count == 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.NoFieldOrProperties, syntax.Identifier.GetLocation(), typeSymbol.Name));
            return;
        }

        var fullType = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
           .Replace("global::", "")
           .Replace("<", "_")
           .Replace(">", "_");

        var classOrStructOrRecord = (typeSymbol.IsRecord, typeSymbol.IsValueType) switch
        {
            (true, true) => "record struct",
            (true, false) => "record",
            (false, true) => "struct",
            (false, false) => "class",
        };

        StringBuilder sb = new();
        sb.AppendLine("// Generated with EIV_Pack.Generator.");
        sb.AppendLine("using EIV_Pack;");
        sb.AppendLine("using EIV_Pack.Formatters;");
        sb.AppendLine("#nullable enable");

        List<string> names = [];
        INamespaceSymbol namespaceSymbol = typeSymbol.ContainingNamespace;
        while (namespaceSymbol != null)
        {
            if (namespaceSymbol.Name.Contains("<global namespace>"))
                break;

            names.Add(namespaceSymbol.Name);
            namespaceSymbol = namespaceSymbol.ContainingNamespace;
        }

        names.Reverse();

        string namespaceStr = string.Empty;

        if (names.Count != 0)
            namespaceStr = string.Join(".", names);

        if (names.Count > 1)
            namespaceStr = namespaceStr.Substring(1);

        if (!string.IsNullOrEmpty(namespaceStr))
            sb.AppendLine($"namespace {namespaceStr};");


        sb.AppendLine();
        sb.AppendLine($"partial {classOrStructOrRecord} {typeSymbol.Name} : IPackable<{typeSymbol.Name}>, IFormatter<{typeSymbol.Name}>");
        sb.AppendLine("{");

        if (!typeSymbol.GetMembers().Any(x => x.IsStatic && x.Kind == SymbolKind.Method && x.Name == ".cctor"))
        {
            sb.AppendLine("""

                    static __REPLACE__()
                    {
                        FormatterProvider.Register<__REPLACE__>();
                    }

                """.Replace("__REPLACE__", typeSymbol.Name));
        }

        sb.AppendLine("""

                public static void RegisterFormatter()
                {
                    if (!FormatterProvider.IsRegistered<__REPLACE__>())
                    {
                        FormatterProvider.Register(new __REPLACE__());
                    }

                    if (!FormatterProvider.IsRegistered<__REPLACE__[]>())
                    {
                        FormatterProvider.Register(new ArrayFormatter<__REPLACE__>());
                    }
                }

            """.Replace("__REPLACE__", typeSymbol.Name));

        GeneratePackable(ref syntax, ref typeSymbol, ref sb, ref fieldOrParamList);

        sb.AppendLine("""

                public void Deserialize(ref PackReader reader, scoped ref __REPLACE__ value)
                {
                    DeserializePackable(ref reader, ref value);
                }

                public void Serialize(ref PackWriter writer, scoped ref readonly __REPLACE__ value)
                {
                    SerializePackable(ref writer, in value);
                }

            """.Replace("__REPLACE__", $"{typeSymbol.Name}{(typeSymbol.IsValueType ? "" : "?")}"));

        sb.AppendLine("}");
        context.AddSource($"{fullType}.g.cs", sb.ToString());
    }


    internal static void GeneratePackable(ref TypeDeclarationSyntax _, ref INamedTypeSymbol typeSymbol, ref StringBuilder sb, ref List<ISymbol> FieldAndParamList)
    {
        var nullable = typeSymbol.IsValueType ? "" : "?";


        sb.AppendLine($"\tconst int EIV_PACK_FieldAndParamCount = {FieldAndParamList.Count};");

        sb.AppendLine($"\tpublic static void DeserializePackable(ref PackReader reader, scoped ref {typeSymbol.Name}{nullable} value)");
        sb.AppendLine("\t{");
        if (FieldAndParamList.Count <= 255)
            sb.AppendLine($"\t\tif (!reader.TryReadSmallHeader(out byte header) || header != EIV_PACK_FieldAndParamCount)");
        else
            sb.AppendLine($"\t\tif (!reader.TryReadHeader(out int header) || header != EIV_PACK_FieldAndParamCount)");

        if (!typeSymbol.IsValueType)
            sb.AppendLine("""
                            {
                                value = null;
                                return;
                            }

                            value ??= new();
                    """);
        else
            sb.AppendLine("\t\tvalue = new();");
        foreach (var item in FieldAndParamList)
        {
            WriteDesParam(ref sb, item);
        }

        sb.AppendLine("\t}");

        sb.AppendLine($"\tpublic static void SerializePackable(ref PackWriter writer, scoped ref readonly {typeSymbol.Name}{nullable} value)");
        sb.AppendLine("\t{");
        if (!typeSymbol.IsValueType)
        {
            if (FieldAndParamList.Count <= 255)
                sb.AppendLine("""
                        if (value == null)
                        {
                            writer.WriteSmallHeader();
                            return;
                        }
                """);
            else
                sb.AppendLine("""
                        if (value == null)
                        {
                            writer.WriteHeader();
                            return;
                        }
                    """);
        }

        if (FieldAndParamList.Count <= 255)
            sb.AppendLine("\t\twriter.WriteSmallHeader(EIV_PACK_FieldAndParamCount);");
        else
            sb.AppendLine("\t\twriter.WriteHeader(EIV_PACK_FieldAndParamCount);");

        foreach (var item in FieldAndParamList)
        {
            WriteSerParam(ref sb, item);
        }

        sb.AppendLine("\t}");
    }

    internal static void WriteSerParam(ref StringBuilder sb, ISymbol symbol)
    {
        ITypeSymbol? type = null;
        switch (symbol)
        {
            case IFieldSymbol fieldSymbol:
                type = fieldSymbol.Type;
                break;
            case IPropertySymbol propertySymbol:
                type = propertySymbol.Type;
                break;
            default:
                break;
        }

        if (type == null)
        {
            sb.AppendLine($"// ERROR CANT GENERATE : {symbol.Name}");
            return;
        }

        if (type is INamedTypeSymbol namedType)
        {
            if (type.IsUnmanagedType && !namedType.IsGenericType)
            {
                sb.AppendLine($"\t\twriter.WriteUnmanaged<{type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>(value.{symbol.Name});");
                return;
            }

            if (type.SpecialType == SpecialType.System_String)
            {
                sb.AppendLine($"\t\twriter.WriteString(value.{symbol.Name});");
                return;
            }
        }

        if (type is IArrayTypeSymbol arrayTypeSymbol)
        {
            sb.AppendLine($"\t\twriter.WriteArray<{arrayTypeSymbol.ElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>(value.{symbol.Name});");
            return;
        }

        sb.AppendLine($"\t\twriter.WriteValue<{type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>(value.{symbol.Name});");
    }

    internal static void WriteDesParam(ref StringBuilder sb, ISymbol symbol)
    {
        ITypeSymbol? type = null;
        NullableAnnotation nullableAnnotation = NullableAnnotation.None;
        switch (symbol)
        {
            case IFieldSymbol fieldSymbol:
                nullableAnnotation = fieldSymbol.NullableAnnotation;
                type = fieldSymbol.Type;
                break;
            case IPropertySymbol propertySymbol:
                nullableAnnotation = propertySymbol.NullableAnnotation;
                type = propertySymbol.Type;
                break;
            default:
                break;
        }

        if (type == null)
        {
            sb.AppendLine($"// ERROR CANT GENERATE : {symbol.Name}");
            return;
        }

        if (type is INamedTypeSymbol namedType)
        {
            if (type.IsUnmanagedType && !namedType.IsGenericType)
            {
                sb.AppendLine($"\t\tvalue.{symbol.Name} = reader.ReadUnmanaged<{type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>();");
                return;
            }

            if (type.SpecialType == SpecialType.System_String)
            {
                sb.AppendLine($"\t\tvalue.{symbol.Name} = reader.ReadString(){(nullableAnnotation == NullableAnnotation.NotAnnotated ? "!" : string.Empty)};");
                return;
            }
        }

        if (type is IArrayTypeSymbol arrayTypeSymbol)
        {
            sb.AppendLine($"\t\tvalue.{symbol.Name} = reader.ReadArray<{arrayTypeSymbol.ElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>(){(nullableAnnotation == NullableAnnotation.NotAnnotated ? " ?? []" : string.Empty)};");
            return;
        }

        sb.AppendLine($"\t\tvalue.{symbol.Name} = reader.ReadValue<{type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>(){(nullableAnnotation == NullableAnnotation.NotAnnotated ? "!" : string.Empty)};");
    }

    static List<ISymbol> GetFieldOrParams(ref INamedTypeSymbol typeSymbol)
    {
        var fieldOrParam = typeSymbol.GetMembers().Where(static x =>
        x.Kind is SymbolKind.Property or SymbolKind.Field &&
        !x.Name.Contains("k__BackingField") &&
        !x.IsStatic && !x.IsAbstract &&
        x.Name != "EqualityContract"
        );


        var fieldOrParamList = fieldOrParam.ToList();

        foreach (var prop in fieldOrParamList.Where(x => x.Kind is SymbolKind.Property).ToList())
        {
            IPropertySymbol propSymbol = (IPropertySymbol)prop;
            if (propSymbol.IsReadOnly)
            {
                fieldOrParamList.Remove(prop);
                continue;
            }

            var getMethod = propSymbol.GetMethod;
            if (getMethod == null)
            {
                fieldOrParamList.Remove(prop);
                continue;
            }

            if (getMethod.DeclaredAccessibility == Accessibility.Private)
            {
                fieldOrParamList.Remove(prop);
                continue;
            }


            var setMethod = propSymbol.SetMethod;
            if (setMethod == null)
            {
                fieldOrParamList.Remove(prop);
                continue;
            }

            if (setMethod.IsInitOnly)
            {
                fieldOrParamList.Remove(prop);
                continue;
            }

            if (setMethod.DeclaredAccessibility == Accessibility.Private)
            {
                fieldOrParamList.Remove(prop);
                continue;
            }
        }

        foreach (var field in fieldOrParamList.Where(x => x.Kind is SymbolKind.Field).ToList())
        {
            IFieldSymbol fieldSymbol = (IFieldSymbol)field;

            if (fieldSymbol.IsReadOnly)
            {
                fieldOrParamList.Remove(field);
            }
        }


        return fieldOrParamList;
    }
}
