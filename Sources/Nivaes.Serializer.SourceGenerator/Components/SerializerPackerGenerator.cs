using System;
using System.Linq;
using System.Text;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nivaes.Serializer.SourceGenerator;

[Generator]
public sealed class SerializerPackerGenerator : IIncrementalGenerator
{
    private const string AttributeMetadataName =
        "Nivaes.Serializer.SerializePackerAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var types = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: AttributeMetadataName,
            predicate: static (node, _) => node is TypeDeclarationSyntax,
            transform: static (attributeContext, _) =>
            {
                var symbol = (INamedTypeSymbol)attributeContext.TargetSymbol;
                var declaration = (TypeDeclarationSyntax)attributeContext.TargetNode;

                var unionCases = ImmutableArray.CreateBuilder<ITypeSymbol>();

                // Obtener las variantes desde:
                // union UnionModel(Type1Model, Type2Model, Type3Model);
                if (declaration.ParameterList is not null)
                {
                    foreach (var parameter in declaration.ParameterList.Parameters)
                    {
                        if (parameter.Type is null)
                            continue;

                        var caseType = attributeContext.SemanticModel
                            .GetTypeInfo(parameter.Type)
                            .Type;

                        if (caseType is not null)
                            unionCases.Add(caseType);
                    }
                }

                return (
                    Symbol: symbol,
                    UnionCases: unionCases.ToImmutable()
                );
            });

        context.RegisterSourceOutput(
            types,
            static (productionContext, data) =>
            {
                Generate(
                    productionContext,
                    data.Symbol,
                    data.UnionCases);
            });
    }

    // ---------------------------------------------------------
    // GENERATE
    // ---------------------------------------------------------

    private static void Generate(
        SourceProductionContext context,
        INamedTypeSymbol symbol,
        ImmutableArray<ITypeSymbol> unionCases)
    {
        // Esta versión no contempla tipos genéricos ni anidados.
        if (symbol.IsGenericType)
            return;

        if (symbol.TypeKind is not (TypeKind.Class or TypeKind.Struct))
            return;

        // -----------------------------------------------------
        // UNION
        // -----------------------------------------------------
#pragma warning disable RSEXPERIMENTAL006 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        if (symbol.IsUnion)
        {
            if (unionCases.IsDefaultOrEmpty)
                return;

            GenerateUnion(context, symbol, unionCases);
            return;
        }
#pragma warning restore RSEXPERIMENTAL006 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        // -----------------------------------------------------
        // CLASS / STRUCT NORMAL
        // -----------------------------------------------------

        var properties = GetProperties(symbol);

        foreach (var property in properties)
        {
            if (!IsSupported(property.Type))
            {
                ReportUnsupportedProperty(context, property);
                return;
            }
        }

        var serializer = GenerateSerializer(symbol, properties);
        var deserializer = GenerateDeserializer(symbol, properties);

        var hintPrefix = GetHintPrefix(symbol);

        context.AddSource(
            $"{hintPrefix}Serializer.g.cs",
            serializer);

        context.AddSource(
            $"{hintPrefix}Deserializer.g.cs",
            deserializer);
    }

    // ---------------------------------------------------------
    // PROPERTIES
    // ---------------------------------------------------------

    private static IPropertySymbol[] GetProperties(
        INamedTypeSymbol symbol)
    {
        return symbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p =>
                !p.IsStatic &&
                !p.IsIndexer &&
                p.GetMethod is not null &&
                p.SetMethod is not null &&
                p.GetMethod.DeclaredAccessibility == Accessibility.Public &&
                p.SetMethod.DeclaredAccessibility == Accessibility.Public)
            .OrderBy(p => p.Locations
                .FirstOrDefault(l => l.IsInSource)?
                .SourceSpan.Start ?? int.MaxValue)
            .ToArray();
    }

    private static void ReportUnsupportedProperty(
        SourceProductionContext context,
        IPropertySymbol property)
    {
        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticDescriptors.UnsupportedProperty,
                property.Locations.FirstOrDefault(),
                property.Name,
                property.Type.ToDisplayString()));
    }

    // ---------------------------------------------------------
    // UNION GENERATION
    // ---------------------------------------------------------

    private static void GenerateUnion(
        SourceProductionContext context,
        INamedTypeSymbol symbol,
        ImmutableArray<ITypeSymbol> cases)
    {
        var caseProperties = new IPropertySymbol[cases.Length][];

        for (var i = 0; i < cases.Length; i++)
        {
            if (cases[i] is not INamedTypeSymbol caseType ||
                caseType.TypeKind is not (TypeKind.Class or TypeKind.Struct))
            {
                return;
            }

            caseProperties[i] = GetProperties(caseType);

            foreach (var property in caseProperties[i])
            {
                if (!IsSupported(property.Type))
                {
                    ReportUnsupportedProperty(context, property);
                    return;
                }
            }
        }

        var serializer = GenerateUnionSerializer(
            symbol,
            cases,
            caseProperties);

        var deserializer = GenerateUnionDeserializer(
            symbol,
            cases,
            caseProperties);

        var hintPrefix = GetHintPrefix(symbol);

        context.AddSource(
            $"{hintPrefix}Serializer.g.cs",
            serializer);

        context.AddSource(
            $"{hintPrefix}Deserializer.g.cs",
            deserializer);
    }

    // ---------------------------------------------------------
    // UNION SERIALIZER
    // ---------------------------------------------------------

    private static string GenerateUnionSerializer(
        INamedTypeSymbol symbol,
        ImmutableArray<ITypeSymbol> cases,
        IPropertySymbol[][] properties)
    {
        var typeName = GetTypeName(symbol);

        var serializerName =
            SourceGenerationHelper.GetSerializerClassName(symbol);

        var accessibility = GetAccessibility(symbol);

        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using Nivaes.Serializer;");

        AppendNamespaceStart(sb, symbol);

        sb.AppendLine(
            $"{accessibility} sealed class {serializerName} : ISerializer<{typeName}>");

        sb.AppendLine("{");

        sb.AppendLine(
            $"    void ISerializer<{typeName}>.Serialize(");

        sb.AppendLine(
            $"        scoped in {typeName} item,");

        sb.AppendLine("        ref PackerWriter writer)");

        sb.AppendLine("    {");

        sb.AppendLine("        switch (item)");
        sb.AppendLine("        {");

        for (var i = 0; i < cases.Length; i++)
        {
            var caseName = GetTypeName(cases[i]);
            var variableName = $"case{i}";

            sb.AppendLine(
                $"            case {caseName} {variableName}:");

            // Discriminador.
            sb.AppendLine(
                $"                writer.Write({i + 1});");

            // Serializar propiedades.
            foreach (var property in properties[i])
            {
                var expression = GetWriteExpression(
                    property,
                    variableName,
                    "                ");

                sb.AppendLine($"                {expression}");
            }

            sb.AppendLine("                break;");
            sb.AppendLine();
        }

        sb.AppendLine("            default:");

        sb.AppendLine(
            "                throw new global::System.InvalidOperationException(\"Invalid union case to serialize.\");");

        sb.AppendLine("        }");

        sb.AppendLine("    }");
        sb.AppendLine("}");

        AppendNamespaceEnd(sb, symbol);

        return sb.ToString();
    }

    // ---------------------------------------------------------
    // UNION DESERIALIZER
    // ---------------------------------------------------------

    private static string GenerateUnionDeserializer(
        INamedTypeSymbol symbol,
        ImmutableArray<ITypeSymbol> cases,
        IPropertySymbol[][] properties)
    {
        var typeName = GetTypeName(symbol);

        var deserializerName =
            SourceGenerationHelper.GetDeserializerClassName(symbol);

        var accessibility = GetAccessibility(symbol);

        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using Nivaes.Serializer;");

        AppendNamespaceStart(sb, symbol);

        sb.AppendLine(
            $"{accessibility} sealed class {deserializerName} : IDeserializer<{typeName}>");

        sb.AppendLine("{");

        // Almacenar la variante concreta mientras se resuelven referencias.
        // object permite conservar cualquier variante sin convertirla a la unión.
        sb.AppendLine("    private object? _value;");

        sb.AppendLine();

        sb.AppendLine(
            $"    {typeName} IDeserializer<{typeName}>.Deserialize(");

        sb.AppendLine("        ref PackerReader reader)");

        sb.AppendLine("    {");

        sb.AppendLine(
            "        int idType = reader.ReadInt32();");

        sb.AppendLine();

        sb.AppendLine("        switch (idType)");
        sb.AppendLine("        {");

        for (var i = 0; i < cases.Length; i++)
        {
            var caseName = GetTypeName(cases[i]);

            sb.AppendLine($"            case {i + 1}:");
            sb.AppendLine("            {");

            // -------------------------------------------------
            // READ PROPERTIES
            // -------------------------------------------------

            for (var j = 0; j < properties[i].Length; j++)
            {
                var property = properties[i][j];

                var variableName = $"__value{j}";

                var readExpression = GetReadExpression(property);

                sb.AppendLine(
                    $"                var {variableName} = {readExpression};");
            }

            sb.AppendLine();

            // -------------------------------------------------
            // CREATE CASE
            // -------------------------------------------------

            sb.AppendLine(
                $"                var value = new {caseName}");

            sb.AppendLine("                {");

            for (var j = 0; j < properties[i].Length; j++)
            {
                var property = properties[i][j];

                sb.AppendLine(
                    $"                    {property.Name} = __value{j}!,");
            }

            sb.AppendLine("                };");

            sb.AppendLine();

            // Guardar la variante concreta para los setters de referencias.
            sb.AppendLine("                _value = value;");

            // La conversión implícita a la unión se realiza una sola vez.
            sb.AppendLine("                return value;");

            sb.AppendLine("            }");
            sb.AppendLine();
        }

        // -----------------------------------------------------
        // INVALID DISCRIMINATOR
        // -----------------------------------------------------

        sb.AppendLine("            default:");

        sb.AppendLine(
            $"                throw new global::System.InvalidOperationException(\"Invalid id type to deserialize {symbol.Name}\");");

        sb.AppendLine("        }");

        sb.AppendLine("    }");

        // -----------------------------------------------------
        // REFERENCE SETTERS
        // -----------------------------------------------------

        GenerateUnionReferenceSetters(
            sb,
            cases,
            properties);

        sb.AppendLine("}");

        AppendNamespaceEnd(sb, symbol);

        return sb.ToString();
    }

    // ---------------------------------------------------------
    // UNION REFERENCE SETTERS
    // ---------------------------------------------------------

    private static void GenerateUnionReferenceSetters(
        StringBuilder sb,
        ImmutableArray<ITypeSymbol> cases,
        IPropertySymbol[][] properties)
    {
        var referenceProperties = properties
            .SelectMany(p => p)
            .Where(p =>
                HasSerializedReferences(p.Type) &&
                CanGenerateReferenceSetter(p))
            .Select(p => p.Name)
            .Distinct()
            .ToArray();

        foreach (var propertyName in referenceProperties)
        {
            sb.AppendLine();

            sb.AppendLine(
                $"    public void Reference{propertyName}(object reference)");

            sb.AppendLine("    {");

            sb.AppendLine("        switch (_value)");
            sb.AppendLine("        {");

            for (var i = 0; i < cases.Length; i++)
            {
                var property = properties[i]
                    .FirstOrDefault(p => p.Name == propertyName);

                if (property is null ||
                    !HasSerializedReferences(property.Type) ||
                    !CanGenerateReferenceSetter(property))
                {
                    continue;
                }

                var caseName = GetTypeName(cases[i]);

                var propertyType = GetTypeName(property.Type);

                sb.AppendLine(
                    $"            case {caseName} item:");

                sb.AppendLine(
                    $"                item.{property.Name} = ({propertyType})reference;");

                // Volver a almacenar la variante.
                // Es necesario también para variantes struct.
                sb.AppendLine("                _value = item;");

                sb.AppendLine("                break;");
            }

            sb.AppendLine("        }");

            sb.AppendLine("    }");
        }
    }

    // ---------------------------------------------------------
    // NORMAL SERIALIZER
    // ---------------------------------------------------------

    private static string GenerateSerializer(
        INamedTypeSymbol symbol,
        IPropertySymbol[] properties)
    {
        var typeName = GetTypeName(symbol);

        var serializerName =
            SourceGenerationHelper.GetSerializerClassName(symbol);

        var accessibility = GetAccessibility(symbol);

        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using Nivaes.Serializer;");

        AppendNamespaceStart(sb, symbol);

        sb.AppendLine(
            $"{accessibility} sealed class {serializerName} : ISerializer<{typeName}>");

        sb.AppendLine("{");

        sb.AppendLine(
            $"    void ISerializer<{typeName}>.Serialize(");

        sb.AppendLine(
            $"        scoped in {typeName} model,");

        sb.AppendLine("        ref PackerWriter writer)");

        sb.AppendLine("    {");

        foreach (var property in properties)
        {
            var expression = GetWriteExpression(property);

            sb.AppendLine($"        {expression}");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        AppendNamespaceEnd(sb, symbol);

        return sb.ToString();
    }

    // ---------------------------------------------------------
    // NORMAL DESERIALIZER
    // ---------------------------------------------------------

    private static string GenerateDeserializer(
        INamedTypeSymbol symbol,
        IPropertySymbol[] properties)
    {
        var typeName = GetTypeName(symbol);

        var deserializerName =
            SourceGenerationHelper.GetDeserializerClassName(symbol);

        var accessibility = GetAccessibility(symbol);

        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using Nivaes.Serializer;");

        AppendNamespaceStart(sb, symbol);

        sb.AppendLine(
            $"{accessibility} sealed class {deserializerName} : IDeserializer<{typeName}>");

        sb.AppendLine("{");

        // Almacenar el objeto deserializado.
        if (symbol.IsReferenceType)
            sb.AppendLine($"    private {typeName}? _value;");
        else
            sb.AppendLine($"    private {typeName} _value = default!;");

        sb.AppendLine();

        sb.AppendLine(
            $"    {typeName} IDeserializer<{typeName}>.Deserialize(");

        sb.AppendLine("        ref PackerReader reader)");

        sb.AppendLine("    {");

        // Leer propiedades.
        for (var i = 0; i < properties.Length; i++)
        {
            var property = properties[i];

            var variableName = $"__value{i}";

            var readExpression = GetReadExpression(property);

            sb.AppendLine(
                $"        var {variableName} = {readExpression};");
        }

        sb.AppendLine();

        // Crear objeto.
        sb.AppendLine($"        _value = new {typeName}");

        sb.AppendLine("        {");

        for (var i = 0; i < properties.Length; i++)
        {
            var property = properties[i];

            sb.AppendLine(
                $"            {property.Name} = __value{i}!,");
        }

        sb.AppendLine("        };");

        sb.AppendLine();

        sb.AppendLine("        return _value!;");

        sb.AppendLine("    }");

        // Generar setters para referencias diferidas.
        foreach (var property in properties)
        {
            if (!HasSerializedReferences(property.Type))
                continue;

            if (!CanGenerateReferenceSetter(property))
                continue;

            var propertyType = GetTypeName(property.Type);

            sb.AppendLine();

            sb.AppendLine(
                $"    public void Reference{property.Name}(object reference)");

            sb.AppendLine("    {");

            sb.AppendLine(
                $"        _value!.{property.Name} = ({propertyType})reference;");

            sb.AppendLine("    }");
        }

        sb.AppendLine("}");

        AppendNamespaceEnd(sb, symbol);

        return sb.ToString();
    }

    // ---------------------------------------------------------
    // INIT PROPERTY DETECTION
    // ---------------------------------------------------------

    private static bool CanGenerateReferenceSetter(
        IPropertySymbol property)
    {
        return property.SetMethod is not null &&
               !property.SetMethod.IsInitOnly;
    }

    // ---------------------------------------------------------
    // PROPERTY WRITE EXPRESSIONS
    // ---------------------------------------------------------

    private static string GetWriteExpression(
        IPropertySymbol property,
        string modelName = "model",
        string indent = "        ")
    {
        var type = property.Type;

        var value = $"{modelName}.{property.Name}";

        // Nullable<T>.
        if (TryGetNullableUnderlyingType(type, out var nullableType))
        {
            // Nullable de tipos serializados.
            if (IsSerializedType(nullableType))
            {
                var typeName = GetTypeName(nullableType);
                var serializerName = GetSerializerName(nullableType);

                return
                    $"writer.Write({value}.HasValue);\n" +
                    $"{indent}if ({value}.HasValue)\n" +
                    $"{indent}    writer.Write<{typeName}, {serializerName}>({value}.Value);";
            }

            // Nullable<T> enum: escribir presencia y valor subyacente.
            if (nullableType.TypeKind == TypeKind.Enum)
            {
                var enumType = (INamedTypeSymbol)nullableType;
                var underlyingType = GetTypeName(enumType.EnumUnderlyingType!);

                return
                    $"writer.Write({value}.HasValue);\n" +
                    $"{indent}if ({value}.HasValue)\n" +
                    $"{indent}    writer.Write(({underlyingType}){value}.Value);";
            }

            // Nullable de tipos simples: PackerWriter.Write(T?).
            return $"writer.Write({value});";
        }

        // byte[]
        if (IsByteArray(type))
            return $"writer.Write({value});";

        // Arrays unidimensionales y multidimensionales.
        if (type is IArrayTypeSymbol arrayType)
        {
            var elementType = arrayType.ElementType;

            // T[]
            if (arrayType.Rank == 1)
            {
                if (IsSerializedType(elementType))
                {
                    var elementTypeName = GetTypeName(elementType);

                    var serializerName = GetSerializerName(elementType);

                    return
                        $"writer.Write<{elementTypeName}, {serializerName}>({value});";
                }

                return
                    $"writer.Write<{GetTypeName(elementType)}>({value});";
            }

            // T[,], T[,,], etc.
            if (IsSerializedType(elementType))
            {
                var elementTypeName = GetTypeName(elementType);

                var serializerName = GetSerializerName(elementType);

                return
                    $"writer.WriteArray<{elementTypeName}, {serializerName}>({value});";
            }

            return
                $"writer.WriteArray<{GetTypeName(elementType)}>({value});";
        }

        // IEnumerable<T>, List<T>, ICollection<T>, etc.
        if (IsEnumerableType(type, out var enumerableElement))
        {
            if (IsSerializedType(enumerableElement))
            {
                var elementTypeName = GetTypeName(enumerableElement);

                var serializerName = GetSerializerName(enumerableElement);

                return
                    $"writer.Write<{elementTypeName}, {serializerName}>({value});";
            }

            return
                $"writer.Write<{GetTypeName(enumerableElement)}>({value});";
        }

        // Clases y structs serializados.
        if (IsSerializedType(type))
        {
            var referenceType = GetTypeName(type);

            var serializerName = GetSerializerName(type);

            return
                $"writer.Write<{referenceType}, {serializerName}>({value});";
        }

        // Enumeraciones.
        if (type.TypeKind == TypeKind.Enum)
        {
            var enumType = (INamedTypeSymbol)type;

            var underlyingType = GetTypeName(enumType.EnumUnderlyingType!);

            return
                $"writer.Write(({underlyingType}){value});";
        }

        // Tipos simples.
        return $"writer.Write({value});";
    }

    // ---------------------------------------------------------
    // PROPERTY READ EXPRESSIONS
    // ---------------------------------------------------------

    private static string GetReadExpression(IPropertySymbol property)
    {
        var type = property.Type;

        // Nullable<T>.
        if (TryGetNullableUnderlyingType(type, out var nullableType))
        {
            // Nullable de tipos serializados.
            if (IsSerializedType(nullableType))
            {
                var rootType = GetTypeName(property.ContainingType);
                var typeName = GetTypeName(nullableType);
                var deserializerName = GetDeserializerName(nullableType);

                return
                    $"reader.ReadBoolean() " +
                    $"? reader.ReadReference<{rootType}, {typeName}, {deserializerName}>() " +
                    $" : ({GetTypeName(type)})null";
            }

            // Nullable de enumeraciones.
            if (nullableType.TypeKind == TypeKind.Enum)
            {
                var enumType = (INamedTypeSymbol)nullableType;
                var underlyingType = enumType.EnumUnderlyingType!;
                var readMethod = GetReadMethod(underlyingType);

                return
                    $"reader.ReadBoolean() " +
                    $"? ({GetTypeName(type)})reader.{readMethod}() " +
                    $" : ({GetTypeName(type)})null";
            }

            // Nullable de tipos simples: ReadNullableInt32(), etc.
            var nullableReadMethod = GetNullableReadMethod(nullableType);

            return $"reader.{nullableReadMethod}()";
        }

        // byte[]
        if (IsByteArray(type))
            return "reader.ReadBytes()";

        // Arrays.
        if (type is IArrayTypeSymbol arrayType)
            return GetArrayReadExpression(property, arrayType);

        // IEnumerable<T>, List<T>, ICollection<T>, etc.
        if (IsEnumerableType(type, out var enumerableElement))
        {
            if (IsSerializedType(enumerableElement))
            {
                var rootType = GetTypeName(property.ContainingType);

                var referenceType = GetTypeName(enumerableElement);

                var deserializerName = GetDeserializerName(enumerableElement);

                if (enumerableElement.IsReferenceType &&
                    CanGenerateReferenceSetter(property))
                {
                    return
                        $"reader.ReadEnumerable<{rootType}, {referenceType}, {deserializerName}>(Reference{property.Name}!)";
                }

                return
                    $"reader.ReadEnumerable<{rootType}, {referenceType}, {deserializerName}>()";
            }

            return
                $"reader.ReadEnumerable<{GetTypeName(enumerableElement)}>()";
        }

        // Clases y structs serializados.
        if (IsSerializedType(type))
        {
            var rootType = GetTypeName(property.ContainingType);

            var referenceType = GetTypeName(type);

            var deserializerName = GetDeserializerName(type);

            if (type.IsReferenceType &&
                CanGenerateReferenceSetter(property))
            {
                return
                    $"reader.ReadReference<{rootType}, {referenceType}, {deserializerName}>(Reference{property.Name}!)";
            }

            return
                $"reader.ReadReference<{rootType}, {referenceType}, {deserializerName}>()";
        }

        // Enumeraciones.
        if (type.TypeKind == TypeKind.Enum)
        {
            var enumType = (INamedTypeSymbol)type;

            var underlyingType = enumType.EnumUnderlyingType!;

            return
                $"({GetTypeName(type)})reader.{GetReadMethod(underlyingType)}()";
        }

        // Tipos simples.
        return $"reader.{GetReadMethod(type)}()";
    }

    // ---------------------------------------------------------
    // ARRAY READ EXPRESSIONS
    // ---------------------------------------------------------

    private static string GetArrayReadExpression(
        IPropertySymbol property,
        IArrayTypeSymbol arrayType)
    {
        var elementType = arrayType.ElementType;

        var isMultidimensional = arrayType.Rank > 1;

        var methodName = isMultidimensional
            ? "ReadMultyArray"
            : "ReadArray";

        // Arrays de tipos serializados.
        if (IsSerializedType(elementType))
        {
            var rootType = GetTypeName(property.ContainingType);

            var referenceType = GetTypeName(elementType);

            var deserializerName = GetDeserializerName(elementType);

            string expression;

            if (elementType.IsReferenceType &&
                CanGenerateReferenceSetter(property))
            {
                expression =
                    $"reader.{methodName}<{rootType}, {referenceType}, {deserializerName}>(Reference{property.Name}!)";
            }
            else
            {
                expression =
                    $"reader.{methodName}<{rootType}, {referenceType}, {deserializerName}>()";
            }

            if (!isMultidimensional)
                return expression;

            return
                $"({GetTypeName(arrayType)})({expression} ?? {GetEmptyArrayExpression(arrayType)})";
        }

        // Arrays de tipos simples.
        var simpleExpression =
            $"reader.{methodName}<{GetTypeName(elementType)}>()";

        if (!isMultidimensional)
            return simpleExpression;

        return
            $"({GetTypeName(arrayType)})({simpleExpression} ?? {GetEmptyArrayExpression(arrayType)})";
    }

    // ---------------------------------------------------------
    // EMPTY MULTIDIMENSIONAL ARRAYS
    // ---------------------------------------------------------

    private static string GetEmptyArrayExpression(
        IArrayTypeSymbol arrayType)
    {
        var elementType = GetTypeName(arrayType.ElementType);

        var dimensions = string.Join(
            ", ",
            Enumerable.Repeat("0", arrayType.Rank));

        return $"new {elementType}[{dimensions}]";
    }

    // ---------------------------------------------------------
    // READ METHOD
    // ---------------------------------------------------------

    private static string? GetReadMethod(ITypeSymbol type)
    {
        if (type.ToDisplayString() == "System.Guid")
            return "ReadGuid";

        if (IsByteArray(type))
            return "ReadBytes";

        var displayType = type.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat);

        if (displayType == "global::System.Memory<byte>")
            return "ReadMemory";

        if (displayType == "global::System.Half")
            return "ReadHalf";

        if (displayType == "global::System.DateTime")
            return "ReadDateTime";

        if (displayType == "global::System.DateTimeOffset")
            return "ReadDateTimeOffset";

        return type.SpecialType switch
        {
            SpecialType.System_String => "ReadString",
            SpecialType.System_Boolean => "ReadBoolean",
            SpecialType.System_Byte => "ReadByte",
            SpecialType.System_SByte => "ReadSByte",
            SpecialType.System_Int16 => "ReadInt16",
            SpecialType.System_UInt16 => "ReadUInt16",
            SpecialType.System_Int32 => "ReadInt32",
            SpecialType.System_UInt32 => "ReadUInt32",
            SpecialType.System_Int64 => "ReadInt64",
            SpecialType.System_UInt64 => "ReadUInt64",
            SpecialType.System_Single => "ReadSingle",
            SpecialType.System_Double => "ReadDouble",
            SpecialType.System_Decimal => "ReadDecimal",

            _ => null
        };
    }

    private static string? GetNullableReadMethod(ITypeSymbol type)
    {
        var readMethod = GetReadMethod(type);

        if (readMethod is null)
            return null;

        return $"ReadNullable{readMethod.Substring("Read".Length)}";
    }

    private static bool IsSupported(ITypeSymbol type)
    {
        // Nullable<T>.
        if (TryGetNullableUnderlyingType(type, out var underlyingType))
        {
            // Nullable de tipos serializados.
            if (IsSerializedType(underlyingType))
                return true;

            // Nullable de enumeraciones: se usa su tipo subyacente.
            if (underlyingType.TypeKind == TypeKind.Enum)
            {
                var enumType = (INamedTypeSymbol)underlyingType;
                return GetReadMethod(enumType.EnumUnderlyingType!) is not null;
            }

            // Para los demás nullable debe existir ReadNullable<T>.
            return IsSupported(underlyingType) &&
                   GetNullableReadMethod(underlyingType) is not null;
        }

        // byte[]
        if (IsByteArray(type))
            return true;

        // Arrays: T[], T[,], T[,,], etc.
        if (type is IArrayTypeSymbol arrayType)
            return IsSupported(arrayType.ElementType);

        // IEnumerable<T>, List<T>, ICollection<T>, etc.
        if (IsEnumerableType(type, out var elementType))
            return IsSupported(elementType);

        // Enumeraciones.
        if (type.TypeKind == TypeKind.Enum)
        {
            var enumType = (INamedTypeSymbol)type;
            return GetReadMethod(enumType.EnumUnderlyingType!) is not null;
        }

        // Clases y estructuras con serializador generado.
        if (IsSerializedType(type))
            return true;

        // Tipos simples y tipos especiales implementados.
        return GetReadMethod(type) is not null;
    }

    private static bool TryGetNullableUnderlyingType(
        ITypeSymbol type,
        out ITypeSymbol underlyingType)
    {
        if (type is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.SpecialType ==
            SpecialType.System_Nullable_T)
        {
            underlyingType = namedType.TypeArguments[0];
            return true;
        }

        underlyingType = null!;

        return false;
    }

    private static bool IsByteArray(ITypeSymbol type)
    {
        return type is IArrayTypeSymbol arrayType &&
               arrayType.Rank == 1 &&
               arrayType.ElementType.SpecialType == SpecialType.System_Byte;
    }

    private static bool IsEnumerableType(
        ITypeSymbol type,
        out ITypeSymbol elementType)
    {
        elementType = null!;

        // No considerar string como IEnumerable<char>.
        if (type.SpecialType == SpecialType.System_String)
            return false;

        if (type is not INamedTypeSymbol namedType)
            return false;

        return ImplementsIEnumerable(namedType, out elementType);
    }

    private static bool ImplementsIEnumerable(
        INamedTypeSymbol type,
        out ITypeSymbol elementType)
    {
        var enumerableInterface = type.AllInterfaces
            .FirstOrDefault(i =>
                i.OriginalDefinition.SpecialType ==
                SpecialType.System_Collections_Generic_IEnumerable_T);

        if (enumerableInterface is not null)
        {
            elementType = enumerableInterface.TypeArguments[0];
            return true;
        }

        // También contempla el propio tipo IEnumerable<T>.
        if (type.OriginalDefinition.SpecialType ==
            SpecialType.System_Collections_Generic_IEnumerable_T)
        {
            elementType = type.TypeArguments[0];
            return true;
        }

        elementType = null!;

        return false;
    }

    private static bool IsSerializedType(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedType)
            return false;

        if (namedType.TypeKind is not (
            TypeKind.Class or TypeKind.Struct))
            return false;

        if (namedType.IsGenericType)
            return false;

        return namedType.GetAttributes().Any(attribute =>
            attribute.AttributeClass?.ToDisplayString() ==
            AttributeMetadataName);
    }

    // Solo las clases serializadas tienen identidad de referencia.
    private static bool IsSerializedReferenceType(ITypeSymbol type)
    {
        return type.IsReferenceType && IsSerializedType(type);
    }

    private static bool HasSerializedReferences(ITypeSymbol type)
    {
        if (IsSerializedReferenceType(type))
            return true;

        // Arrays de cualquier dimensión.
        if (type is IArrayTypeSymbol arrayType)
            return HasSerializedReferences(arrayType.ElementType);

        // Colecciones.
        return IsEnumerableType(type, out var elementType) &&
               HasSerializedReferences(elementType);
    }

    // ---------------------------------------------------------
    // SYMBOL NAME HELPERS
    // ---------------------------------------------------------

    private static string GetTypeName(ITypeSymbol symbol)
    {
        return symbol.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat);
    }

    private static string GetSerializerName(ITypeSymbol symbol)
    {
        var namedType = (INamedTypeSymbol)symbol;

        return GetContainingNamespace(symbol) +
               SourceGenerationHelper.GetSerializerClassName(namedType);
    }

    private static string GetDeserializerName(ITypeSymbol symbol)
    {
        var namedType = (INamedTypeSymbol)symbol;

        return GetContainingNamespace(symbol) +
               SourceGenerationHelper.GetDeserializerClassName(namedType);
    }

    private static string GetContainingNamespace(ITypeSymbol symbol)
    {
        var ns = symbol.ContainingNamespace;

        return ns.IsGlobalNamespace
            ? "global::"
            : $"global::{ns.ToDisplayString()}.";
    }

    private static string GetAccessibility(INamedTypeSymbol symbol)
    {
        return symbol.DeclaredAccessibility == Accessibility.Public
            ? "public"
            : "internal";
    }

    private static string GetHintPrefix(INamedTypeSymbol symbol)
    {
        return symbol.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat)
            .Replace("global::", "")
            .Replace(".", "_")
            .Replace("<", "_")
            .Replace(">", "_");
    }

    // ---------------------------------------------------------
    // NAMESPACE HELPERS
    // ---------------------------------------------------------

    private static void AppendNamespaceStart(
        StringBuilder sb,
        INamedTypeSymbol symbol)
    {
        if (!symbol.ContainingNamespace.IsGlobalNamespace)
        {
            sb.AppendLine(
                $"namespace {symbol.ContainingNamespace.ToDisplayString()};");

            sb.AppendLine();
        }
    }

    private static void AppendNamespaceEnd(
        StringBuilder sb,
        INamedTypeSymbol symbol)
    {
        // Se utiliza namespace con sintaxis de fichero.
    }
}