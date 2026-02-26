using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace EIV_Pack.Generator;

internal static class PackGenerator
{
    internal static void Generate(GeneratorClass generatorClass, SourceProductionContext context)
    {
        if (!PreProcessing.EarlyReturn(generatorClass, context, out INamedTypeSymbol? typeSymbol))
            return;

        if (typeSymbol == null)
            return;

        var serializables = PreProcessing.GetSerializables(typeSymbol, context, generatorClass.Syntax);

        var initOnly = PreProcessing.InitOnly(ref serializables);

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
        sb.AppendLine();

        if (generatorClass.IsNet8OrGreater)
        {
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
        }

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
        {
            if (generatorClass.IsNet8OrGreater)
            {
                sb.AppendLine($"namespace {namespaceStr};");
            }
            else
            {
                sb.AppendLine($"namespace {namespaceStr}");
                sb.AppendLine("{");
            }
        }

        sb.AppendLine();
        sb.AppendLine($"partial {classOrStructOrRecord} {typeSymbol.Name} : IPackable<{typeSymbol.Name}>");
        sb.AppendLine("{");

        if (!typeSymbol.GetMembers().Any(x => x.IsStatic && x.Kind == SymbolKind.Method && x.Name == ".cctor"))
        {
            sb.AppendLine(
                $$"""

                    static {{typeSymbol.Name}}()
                    {
                        RegisterFormatter();
                    }
                
                """);
        }

        bool hasRegister = PreProcessing.HasRegisterFormatter(typeSymbol);

        string formatterName = generatorClass.IsNet8OrGreater ? $"EIV_Formatter<{typeSymbol.Name}>" : $"{typeSymbol.Name}_Formatter";

        sb.AppendLine(
            $$"""

                public static{{(hasRegister ? " new " : " ")}}void RegisterFormatter()
                {
                    if (!FormatterProvider.IsRegistered<{{typeSymbol.Name}}>())
                    {
                        FormatterProvider.Register<{{typeSymbol.Name}}>(new {{formatterName}}());
                    }

                    if (!FormatterProvider.IsRegistered<{{typeSymbol.Name}}[]>())
                    {
                        FormatterProvider.Register(new ArrayFormatter<{{typeSymbol.Name}}>());
                    }
                }
            
            """);

        GeneratePackable(ref typeSymbol, ref sb, ref serializables, ref initOnly, generatorClass.IsNet8OrGreater);

        sb.AppendLine("}");

        if (!string.IsNullOrEmpty(namespaceStr) && !generatorClass.IsNet8OrGreater)
        {
            sb.AppendLine();
            sb.AppendLine("}");
        }

        context.AddSource($"{fullType}.g.cs", sb.ToString());

        if (generatorClass.IsNet8OrGreater)
            return;

        // Generate Formatter here.

        sb = new();
        sb.AppendLine("// Generated with EIV_Pack.Generator.");
        sb.AppendLine("using EIV_Pack;");
        sb.AppendLine("using EIV_Pack.Formatters;");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(namespaceStr))
        {
            sb.AppendLine($"namespace {namespaceStr}");
            sb.AppendLine("{");
        }

        sb.AppendLine($$"""
                sealed class {{typeSymbol.Name}}_Formatter : BaseFormatter<{{typeSymbol.Name}}>
                {
                    public override void Serialize(ref PackWriter writer, scoped ref readonly {{typeSymbol.Name}} value)
                    {
                        {{typeSymbol.Name}}.SerializePackable(ref writer, in value);
                    }

                    public override void Deserialize(ref PackReader reader, scoped ref {{typeSymbol.Name}} value)
                    {
                        {{typeSymbol.Name}}.DeserializePackable(ref reader, ref value);
                    }
                }
            """);

        if (!string.IsNullOrEmpty(namespaceStr))
        {
            sb.AppendLine();
            sb.AppendLine("}");
        }

        context.AddSource($"{fullType}_Formatter.g.cs", sb.ToString());
    }


    internal static void GeneratePackable(ref INamedTypeSymbol typeSymbol, ref StringBuilder sb, ref List<ISymbol> FieldAndParamList, ref List<ISymbol> initOnly, bool isNet8OrGreater)
    {
        var nullable = !typeSymbol.IsValueType && isNet8OrGreater ? "?" : string.Empty;
        var exceptInit = FieldAndParamList.Except(initOnly);
        bool hasInitOnly = initOnly.Count > 0;
        string ending = hasInitOnly ? string.Empty : ";";

        sb.AppendLine($"\tconst int EIV_PACK_FieldAndParamCount = {FieldAndParamList.Count};");
        sb.AppendLine();
        sb.AppendLine($"\tpublic static void DeserializePackable(ref PackReader reader, scoped ref {typeSymbol.Name}{nullable} value)");
        sb.AppendLine("\t{");
        if (FieldAndParamList.Count <= 255)
            sb.AppendLine($"\t\tif (!reader.TryReadSmallHeader(out byte header) || header != EIV_PACK_FieldAndParamCount)");
        else
            sb.AppendLine($"\t\tif (!reader.TryReadHeader(out int header) || header != EIV_PACK_FieldAndParamCount)");

        if (!typeSymbol.IsValueType)
            sb.AppendLine(
                $$"""
                            {
                                value = null;
                                return;
                            }

                            value ??= new(){{ending}}
                    """);
        else
            sb.AppendLine($"\t\tvalue = new(){ending}");

        if (hasInitOnly)
        {
            sb.AppendLine("\t\t{");

            foreach (var item in initOnly)
            {
                WriteDesParam(ref sb, item, false);
            }

            sb.AppendLine("\t\t};");
        }

        foreach (var item in exceptInit)
        {
            WriteDesParam(ref sb, item);
        }

        sb.AppendLine("\t}");
        sb.AppendLine();
        sb.AppendLine($"\tpublic static void SerializePackable(ref PackWriter writer, scoped ref readonly {typeSymbol.Name}{nullable} value)");
        sb.AppendLine("\t{");

        string useSmall = FieldAndParamList.Count <= 255 ? "Small" : string.Empty;

        if (!typeSymbol.IsValueType)
        {
            sb.AppendLine(
                $$"""
                        if (value == null)
                        {
                            writer.Write{{useSmall}}Header();
                            return;
                        }
                """);
        }

        sb.AppendLine($"\t\twriter.Write{useSmall}Header(EIV_PACK_FieldAndParamCount);");

        foreach (var item in initOnly)
        {
            WriteSerParam(ref sb, item);
        }

        foreach (var item in exceptInit)
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

    internal static void WriteDesParam(ref StringBuilder sb, ISymbol symbol, bool useValue = true)
    {
        string val = useValue ? "value." : "\t";
        string endColon = useValue ? ";" : ",";
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
                sb.AppendLine($"\t\t{val}{symbol.Name} = reader.ReadUnmanaged<{type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>(){endColon}");
                return;
            }

            if (type.SpecialType == SpecialType.System_String)
            {
                sb.AppendLine($"\t\t{val}{symbol.Name} = reader.ReadString(){(nullableAnnotation == NullableAnnotation.NotAnnotated ? "!" : string.Empty)}{endColon}");
                return;
            }
        }

        if (type is IArrayTypeSymbol arrayTypeSymbol)
        {
            sb.AppendLine($"\t\t{val}{symbol.Name} = reader.ReadArray<{arrayTypeSymbol.ElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>(){(nullableAnnotation == NullableAnnotation.NotAnnotated ? " ?? []" : string.Empty)}{endColon}");
            return;
        }

        sb.AppendLine($"\t\t{val}{symbol.Name} = reader.ReadValue<{type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>(){(nullableAnnotation == NullableAnnotation.NotAnnotated ? "!" : string.Empty)}{endColon}");
    }

}
