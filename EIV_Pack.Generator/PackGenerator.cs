using Microsoft.CodeAnalysis;
using System.Text;

namespace EIV_Pack.Generator;

internal static class PackGenerator
{
    internal static void Generate(GeneratorClass generatorClass, SourceProductionContext context)
    {
        if (!PreProcessing.EarlyReturn(generatorClass, context, out INamedTypeSymbol? typeSymbol, out int GenerateType))
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

        if (initOnly.Count > 0 && GenerateType == 1)
        {
            foreach ( var initOnlySymbol in initOnly)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.VersionTolerantNoReadOnly, initOnlySymbol.Locations[0], typeSymbol.Name, initOnlySymbol.Name));
                return;
            }
        }

        if (GenerateType == 1)
        {
            foreach (var item in serializables.Where(static x => !PreProcessing.HasOrder(x)))
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.VersionTolerantMustOrder, item.Locations[0], typeSymbol.Name, item.Name));
            }
        }

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
        sb.AppendLine("/// <inheritdoc />");
        sb.AppendLine($"partial {classOrStructOrRecord} {TypeName} : IPackable<{TypeName}>");
        sb.AppendLine("{");

        if (!typeSymbol.GetMembers().Any(x => x.IsStatic && x.Kind == SymbolKind.Method && x.Name == ".cctor"))
        {
            sb.AppendLine(
                $$"""

                    /// <inheritdoc />
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

                /// <inheritdoc />
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

        GeneratePackable(ref typeSymbol, ref sb, ref serializables, ref initOnly, generatorClass.IsNet8OrGreater, GenerateType == 1);

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
                /// <inheritdoc />
                sealed class {{typeSymbol.Name}}_Formatter{{typeParameterStr}} : BaseFormatter<{{TypeName}}>{{ string.Join("\n, ", HelperMethods.GetWhereClauses(typeSymbol))}}
                {
                    /// <inheritdoc />
                    public override void Serialize(ref PackWriter writer, scoped ref readonly {{TypeName}} value)
                    {
                        {{TypeName}}.SerializePackable(ref writer, in value);
                    }

                    /// <inheritdoc />
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


    internal static void GeneratePackable(ref INamedTypeSymbol typeSymbol, 
        ref StringBuilder sb, 
        ref List<ISymbol> FieldAndParamList, 
        ref List<ISymbol> initOnly, 
        bool isNet8OrGreater,
        bool isVersionTolerant)
    {
        if (typeSymbol.IsValueType)
        {
            if (isVersionTolerant)
                GeneratePackable_ValueType_VersionTolerant(ref typeSymbol, ref sb, ref FieldAndParamList);
            else
                GeneratePackable_ValueType(ref typeSymbol, ref sb, ref FieldAndParamList, ref initOnly);

            return;
        }

        if (isVersionTolerant)
        {
            GeneratePackable_VersionTolerant(ref typeSymbol, ref sb, ref FieldAndParamList, isNet8OrGreater);
            return;
        }

        var TypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var nullable = isNet8OrGreater ? "?" : string.Empty;
        var exceptInit = FieldAndParamList.Except(initOnly);
        bool hasInitOnly = initOnly.Count > 0;
        string ending = hasInitOnly ? string.Empty : ";";
        string useSmall = FieldAndParamList.Count <= 255 ? "Small" : string.Empty;

        sb.AppendLine($"\tprivate const int EIV_PACK_FieldAndParamCount = {FieldAndParamList.Count};");

        sb.AppendLine();
        sb.AppendLine("/// <inheritdoc />");
        sb.AppendLine($"\tpublic static void DeserializePackable(ref PackReader reader, scoped ref {TypeName}{nullable} value)");
        sb.AppendLine("\t{");

       sb.AppendLine($"\t\tif (!reader.TryRead{useSmall}Header(out var header) || header != EIV_PACK_FieldAndParamCount)");


        sb.AppendLine(
                        $$"""
                            {
                                value = null;
                                return;
                            }

                            value ??= new(){{ending}}
                    """);

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
        sb.AppendLine("/// <inheritdoc />");
        sb.AppendLine($"\tpublic static void SerializePackable(ref PackWriter writer, scoped ref readonly {TypeName}{nullable} value)");
        sb.AppendLine("\t{");

        sb.AppendLine(
                        $$"""
                        if (value == null)
                        {
                            writer.Write{{useSmall}}Header();
                            return;
                        }
                """);

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



    internal static void GeneratePackable_ValueType(ref INamedTypeSymbol typeSymbol,
        ref StringBuilder sb,
        ref List<ISymbol> FieldAndParamList,
        ref List<ISymbol> initOnly)
    {
        var TypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var exceptInit = FieldAndParamList.Except(initOnly);
        bool hasInitOnly = initOnly.Count > 0;
        string ending = hasInitOnly ? string.Empty : ";";
        string useSmall = FieldAndParamList.Count <= 255 ? "Small" : string.Empty;

        sb.AppendLine($"\tprivate const int EIV_PACK_FieldAndParamCount = {FieldAndParamList.Count};");
        sb.AppendLine();
        sb.AppendLine("/// <inheritdoc />");
        sb.AppendLine($"\tpublic static void DeserializePackable(ref PackReader reader, scoped ref {TypeName} value)");
        sb.AppendLine("\t{");

        sb.AppendLine($"\t\tif (!reader.TryRead{useSmall}Header(out var header) || header != EIV_PACK_FieldAndParamCount)");

        sb.AppendLine(
    $$"""
                            {
                                value = default;
                                return;
                            }

                            value = new(){{ending}}
                    """);

        if (hasInitOnly)
        {
            sb.AppendLine("\t\t{");

            foreach (var item in initOnly)
                WriteDesParam(ref sb, item, false);

            sb.AppendLine("\t\t};");
        }

        foreach (var item in exceptInit)
            WriteDesParam(ref sb, item);

        sb.AppendLine("\t}");
        sb.AppendLine();
        sb.AppendLine("/// <inheritdoc />");
        sb.AppendLine($"\tpublic static void SerializePackable(ref PackWriter writer, scoped ref readonly {TypeName} value)");
        sb.AppendLine("\t{");
        sb.AppendLine($"\t\twriter.Write{useSmall}Header(EIV_PACK_FieldAndParamCount);");

        foreach (var item in initOnly)
            WriteSerParam(ref sb, item);

        foreach (var item in exceptInit)
            WriteSerParam(ref sb, item);

        sb.AppendLine("\t}");
    }

    internal static void GeneratePackable_ValueType_VersionTolerant(ref INamedTypeSymbol typeSymbol,
        ref StringBuilder sb,
        ref List<ISymbol> FieldAndParamList)
    {
        var TypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        sb.AppendLine();
        sb.AppendLine("/// <inheritdoc />");
        sb.AppendLine($"\tpublic static void DeserializePackable(ref PackReader reader, scoped ref {TypeName} value)");
        sb.AppendLine("\t{");

        sb.AppendLine($"\t\tif (!reader.TryReadHeader(out var len) || len == -1)");

        sb.AppendLine(
    $$"""
                            {
                                value = default;
                                return;
                            }

                            value = new();

                            for (int i = 0; i < len; i++)
                            {
                                int order = reader.ReadHeader();
                                switch (order)
                                {
                                    default:
                                        break;
                    """);

        foreach (var item in FieldAndParamList)
        {
            sb.AppendLine();
            sb.AppendLine($"                case {PreProcessing.GetOrder(item)}:");
            WriteDesParam(ref sb, item, inSwitch: true);
            sb.AppendLine("                    break;");
        }
            

        sb.AppendLine(
    $$"""
                                }
                            }
                    """);

        sb.AppendLine("\t}");
        sb.AppendLine();
        sb.AppendLine("/// <inheritdoc />");
        sb.AppendLine($"\tpublic static void SerializePackable(ref PackWriter writer, scoped ref readonly {TypeName} value)");
        sb.AppendLine("\t{");
        sb.AppendLine($"\t\twriter.WriteHeader({FieldAndParamList.Count});");

        foreach (var item in FieldAndParamList)
        {
            sb.AppendLine($"\t\twriter.WriteUnmanaged<int>({PreProcessing.GetOrder(item)});");
            WriteSerParam(ref sb, item);
        }

        sb.AppendLine("\t}");
    }

    internal static void GeneratePackable_VersionTolerant(ref INamedTypeSymbol typeSymbol,
        ref StringBuilder sb,
        ref List<ISymbol> FieldAndParamList,
        bool isNet8OrGreater)
    {
        var TypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var nullable = isNet8OrGreater ? "?" : string.Empty;

        sb.AppendLine();
        sb.AppendLine("/// <inheritdoc />");
        sb.AppendLine($"\tpublic static void DeserializePackable(ref PackReader reader, scoped ref {TypeName}{nullable} value)");
        sb.AppendLine("\t{");

        sb.AppendLine($"\t\tif (!reader.TryReadHeader(out var len) || len == -1)");

        sb.AppendLine(
    $$"""
                            {
                                value = null;
                                return;
                            }

                            value ??= new();

                            for (int i = 0; i < len; i++)
                            {
                                int order = reader.ReadHeader();
                                switch (order)
                                {
                                    default:
                                        break;
                    """);

        foreach (var item in FieldAndParamList)
        {
            sb.AppendLine();
            sb.AppendLine($"                case {PreProcessing.GetOrder(item)}:");
            WriteDesParam(ref sb, item, inSwitch: true);
            sb.AppendLine("                    break;");
        }


        sb.AppendLine(
    $$"""
                                }
                            }
                    """);

        sb.AppendLine("\t}");
        sb.AppendLine();
        sb.AppendLine("/// <inheritdoc />");
        sb.AppendLine($"\tpublic static void SerializePackable(ref PackWriter writer, scoped ref readonly {TypeName}{nullable} value)");
        sb.AppendLine("\t{");
        sb.AppendLine(
                """
                        if (value == null)
                        {
                            writer.WriteHeader();
                            return;
                        }
                """);
        sb.AppendLine($"\t\twriter.WriteHeader({FieldAndParamList.Count});");

        foreach (var item in FieldAndParamList)
        {
            sb.AppendLine($"\t\twriter.WriteUnmanaged<int>({PreProcessing.GetOrder(item)});");
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

    internal static void WriteDesParam(ref StringBuilder sb, ISymbol symbol, bool useValue = true, bool inSwitch = false)
    {
        string val = useValue ? "value." : "\t";
        string endColon = useValue ? ";" : ",";
        string morePad = inSwitch ? "\t\t\t" : string.Empty;
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
                sb.AppendLine($"\t\t{morePad}{val}{symbol.Name} = reader.ReadUnmanaged<{type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>(){endColon}");
                return;
            }

            if (type.SpecialType == SpecialType.System_String)
            {
                sb.AppendLine($"\t\t{morePad}{val}{symbol.Name} = reader.ReadString(){(nullableAnnotation == NullableAnnotation.NotAnnotated ? "!" : string.Empty)}{endColon}");
                return;
            }
        }

        if (type is IArrayTypeSymbol arrayTypeSymbol)
        {
            sb.AppendLine($"\t\t{morePad}{val}{symbol.Name} = reader.ReadArray<{arrayTypeSymbol.ElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>(){(nullableAnnotation == NullableAnnotation.NotAnnotated ? " ?? []" : string.Empty)}{endColon}");
            return;
        }

        sb.AppendLine($"\t\t{morePad}{val}{symbol.Name} = reader.ReadValue<{type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>(){(nullableAnnotation == NullableAnnotation.NotAnnotated ? "!" : string.Empty)}{endColon}");
    }

}
