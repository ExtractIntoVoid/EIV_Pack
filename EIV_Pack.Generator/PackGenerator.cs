using Microsoft.CodeAnalysis;
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

        var fullType = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
           .Replace("global::", "")
           .Replace("<", "_")
           .Replace(">", "_");

        var TypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        var serializables = PreProcessing.GetSerializables(typeSymbol, context, generatorClass.Syntax);
        var initOnly = PreProcessing.InitOnly(ref serializables);     

        var classOrStructOrRecord = HelperMethods.ValueName(typeSymbol);

        string typeParameterStr = HelperMethods.GetTypeParameters(typeSymbol);

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

        string namespaceStr = HelperMethods.GetNameSpace(typeSymbol);

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
        sb.AppendLine($"partial {classOrStructOrRecord} {TypeName} : IPackable<{TypeName}>");
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

        string formatterName = generatorClass.IsNet8OrGreater ? $"EIV_Formatter<{TypeName}>" : $"{typeSymbol.Name}_Formatter{typeParameterStr}";

        sb.AppendLine(
            $$"""

                public static{{(hasRegister ? " new " : " ")}}void RegisterFormatter()
                {
                    if (!FormatterProvider.IsRegistered<{{TypeName}}>())
                    {
                        FormatterProvider.Register<{{TypeName}}>(new {{formatterName}}());
                    }

                    if (!FormatterProvider.IsRegistered<{{TypeName}}[]>())
                    {
                        FormatterProvider.Register(new ArrayFormatter<{{TypeName}}>());
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
                sealed class {{typeSymbol.Name}}_Formatter{{typeParameterStr}} : BaseFormatter<{{TypeName}}>{{ string.Join("\n, ", HelperMethods.GetWhereClauses(typeSymbol))}}
                {
                    public override void Serialize(ref PackWriter writer, scoped ref readonly {{TypeName}} value)
                    {
                        {{TypeName}}.SerializePackable(ref writer, in value);
                    }

                    public override void Deserialize(ref PackReader reader, scoped ref {{TypeName}} value)
                    {
                        {{TypeName}}.DeserializePackable(ref reader, ref value);
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
        var TypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var nullable = !typeSymbol.IsValueType && isNet8OrGreater ? "?" : string.Empty;
        var exceptInit = FieldAndParamList.Except(initOnly);
        bool hasInitOnly = initOnly.Count > 0;
        string ending = hasInitOnly ? string.Empty : ";";

        sb.AppendLine($"\tconst int EIV_PACK_FieldAndParamCount = {FieldAndParamList.Count};");
        sb.AppendLine();
        sb.AppendLine($"\tpublic static void DeserializePackable(ref PackReader reader, scoped ref {TypeName}{nullable} value)");
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
        sb.AppendLine($"\tpublic static void SerializePackable(ref PackWriter writer, scoped ref readonly {TypeName}{nullable} value)");
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
