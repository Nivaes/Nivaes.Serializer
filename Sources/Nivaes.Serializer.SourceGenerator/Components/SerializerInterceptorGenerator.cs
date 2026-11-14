using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nivaes.Serializer.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class SerializerInterceptorGenerator : IIncrementalGenerator
{
    private const string TargetType =
        "Nivaes.Serializer.SerializePacker";

    private const string SerializePackerAttribute =
        "Nivaes.Serializer.SerializePackerAttribute";

    private sealed class InterceptInfo
    {
        public int Version { get; }
        public string LocationData { get; }
        public string MethodName { get; }
        public string ModelType { get; }
        public string HelperType { get; }
        public string? SecondHelperType { get; }
        public string ReturnType { get; }
        public string Parameters { get; }
        public string Arguments { get; }
        public bool ReturnsVoid { get; }

        public bool HasSerializePackerAttribute { get; }
        public Location DiagnosticLocation { get; }

        public InterceptInfo(
            int version,
            string locationData,
            string methodName,
            string modelType,
            string helperType,
            string? secondHelperType,
            string returnType,
            string parameters,
            string arguments,
            bool returnsVoid,
            bool hasSerializePackerAttribute,
            Location diagnosticLocation)
        {
            Version = version;
            LocationData = locationData;
            MethodName = methodName;
            ModelType = modelType;
            HelperType = helperType;
            SecondHelperType = secondHelperType;
            ReturnType = returnType;
            Parameters = parameters;
            Arguments = arguments;
            ReturnsVoid = returnsVoid;

            HasSerializePackerAttribute = hasSerializePackerAttribute;
            DiagnosticLocation = diagnosticLocation;
        }
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var calls = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) =>
            {
                if (node is not InvocationExpressionSyntax invocation)
                    return false;

                if (invocation.Expression is MemberAccessExpressionSyntax member)
                {
                    var name = member.Name.Identifier.ValueText;

                    return name is "Serialize" or "Deserialize" or "Clone";
                }

                return false;
            },
            transform: static (ctx, ct) =>
            {
                var invocation = (InvocationExpressionSyntax)ctx.Node;

                if (ctx.SemanticModel.GetSymbolInfo(invocation, ct).Symbol
                    is not IMethodSymbol symbol)
                {
                    return null;
                }

                if (symbol.ContainingType.ToDisplayString() != TargetType)
                    return null;

                if (symbol.Arity != 1 || symbol.TypeArguments.Length != 1)
                    return null;

                if (symbol.Name is not ("Serialize" or "Deserialize" or "Clone"))
                    return null;

                var location = ctx.SemanticModel
                    .GetInterceptableLocation(invocation, ct);

                if (location is null)
                    return null;

                if (symbol.TypeArguments[0] is not INamedTypeSymbol model)
                    return null;

                var modelType = model.ToDisplayString(
                    NullableFormat);

                // Comprobar si el tipo tiene aplicado SerializePackerAttribute.
                var hasSerializePackerAttribute = model
                    .GetAttributes()
                    .Any(attribute =>
                        attribute.AttributeClass?.ToDisplayString() ==
                        SerializePackerAttribute);

                // Obtener los nombres de las clases auxiliares.
                var serializerClassName = model.GetSerializerClassName();
                var deserializerClassName = model.GetDeserializerClassName();

                // Construir los nombres completamente cualificados.
                var serializerType = GetQualifiedHelperTypeName(
                    model,
                    serializerClassName);

                var deserializerType = GetQualifiedHelperTypeName(
                    model,
                    deserializerClassName);

                string helperType;
                string? secondHelperType = null;

                switch (symbol.Name)
                {
                    case "Serialize":
                        helperType = serializerType;
                        break;

                    case "Deserialize":
                        helperType = deserializerType;
                        break;

                    case "Clone":
                        helperType = serializerType;
                        secondHelperType = deserializerType;
                        break;

                    default:
                        return null;
                }

                var parameters = string.Join(
                    ", ",
                    symbol.Parameters.Select(GetParameterDeclaration));

                var arguments = string.Join(
                    ", ",
                    symbol.Parameters.Select(GetArgument));

                var returnType = symbol.ReturnType.ToDisplayString(
                    NullableFormat);

                return new InterceptInfo(
                    location.Version,
                    location.Data,
                    symbol.Name,
                    modelType,
                    helperType,
                    secondHelperType,
                    returnType,
                    parameters,
                    arguments,
                    symbol.ReturnsVoid,
                    hasSerializePackerAttribute,
                    invocation.GetLocation());
            })
            .Where(static x => x is not null)
            .Collect();

        context.RegisterSourceOutput(calls, static (spc, items) =>
        {
            var source = new StringBuilder();

            source.AppendLine(Header);
            source.AppendLine(InterceptorClassTemplate);

            source.AppendLine();
            source.AppendLine(
                "namespace Nivaes.Serializer.SourceGenerator");
            source.AppendLine("{");

            source.AppendLine(
                "    internal static class SerializePackerInterceptors");
            source.AppendLine("    {");

            var index = 0;

            foreach (var item in items)
            {
                if (item is null)
                    continue;

                // Si el tipo no tiene SerializePackerAttribute,
                // generar un error de compilación y omitir el interceptor.
                if (!item.HasSerializePackerAttribute)
                {
                    spc.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.MissingSerializePackerAttribute,
                            item.DiagnosticLocation,
                            item.ModelType));

                    continue;
                }

                var locationData = SymbolDisplay.FormatLiteral(
                    item.LocationData,
                    quote: true);

                // Serialize y Deserialize utilizan dos argumentos genéricos.
                // Clone utiliza tres.
                var genericArguments = item.SecondHelperType is null
                    ? $"{item.ModelType}, {item.HelperType}"
                    : $"{item.ModelType}, {item.HelperType}, {item.SecondHelperType}";

                source.AppendLine(
                    $"        [global::System.Runtime.CompilerServices.InterceptsLocation({item.Version}, {locationData})]");

                source.AppendLine(
                    $"        public static {item.ReturnType} {item.MethodName}_{index}(");

                source.AppendLine(
                    $"            {item.Parameters})");

                source.AppendLine("        {");

                var methodCall =
                    $"global::Nivaes.Serializer.SerializePacker.{item.MethodName}<{genericArguments}>({item.Arguments})";

                if (item.ReturnsVoid)
                {
                    source.AppendLine($"            {methodCall};");
                }
                else
                {
                    source.AppendLine($"            return {methodCall};");
                }

                source.AppendLine("        }");
                source.AppendLine();

                index++;
            }

            source.AppendLine("    }");
            source.AppendLine("}");

            spc.AddSource(
                "SerializePackerInterceptors.g.cs",
                source.ToString());
        });
    }

    private static string GetQualifiedHelperTypeName(
        INamedTypeSymbol model,
        string helperClassName)
    {
        var ns = model.ContainingNamespace;

        return ns.IsGlobalNamespace
            ? $"global::{helperClassName}"
            : $"global::{ns.ToDisplayString()}.{helperClassName}";
    }

    private static string GetParameterDeclaration(
        IParameterSymbol parameter)
    {
        var modifier = parameter.RefKind switch
        {
            RefKind.Ref => "ref ",
            RefKind.Out => "out ",
            RefKind.In => "in ",
            RefKind.RefReadOnlyParameter => "ref readonly ",
            _ => string.Empty
        };

        var paramsModifier = parameter.IsParams ? "params " : "";

        var type = parameter.Type.ToDisplayString(NullableFormat);

        return $"{paramsModifier}{modifier}{type} {parameter.Name}";
    }

    private static string GetArgument(IParameterSymbol parameter)
    {
        var modifier = parameter.RefKind switch
        {
            RefKind.Ref => "ref ",
            RefKind.Out => "out ",
            RefKind.In => "in ",
            RefKind.RefReadOnlyParameter => "in ",
            _ => string.Empty
        };

        return $"{modifier}{parameter.Name}";
    }

    private static readonly SymbolDisplayFormat NullableFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
            SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions |
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    private const string Header =
        """
        //------------------------------------------------------------------------------
        // <auto-generated>
        //     Generated by Nivaes.Serializer.SourceGenerator
        // </auto-generated>
        //------------------------------------------------------------------------------

        #nullable enable
        """;

    private const string InterceptorClassTemplate =
        """
        namespace System.Runtime.CompilerServices
        {
            [global::System.Diagnostics.Conditional("DEBUG")]
            [global::System.AttributeUsage(
                global::System.AttributeTargets.Method,
                AllowMultiple = true)]
            sealed file class InterceptsLocationAttribute
                : global::System.Attribute
            {
                public InterceptsLocationAttribute(
                    int version,
                    string data)
                {
                    _ = version;
                    _ = data;
                }
            }
        }
        """;
}
