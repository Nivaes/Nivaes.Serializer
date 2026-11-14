using Microsoft.CodeAnalysis;

namespace Nivaes.Serializer.SourceGenerator
{
    internal static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor UnsupportedProperty =
            new(
             id: "NVS001",
             title: "Unsupported property type",
             messageFormat: "Property '{0}' has an unsupported type '{1}'.",
             category: "Nivaes.Serializer",
             defaultSeverity: DiagnosticSeverity.Error,
             isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MissingSerializePackerAttribute =
        new(
            id: "NVS002",
            title: "Missing SerializePackerAttribute",
            messageFormat: "The type '{0}' must be marked with [SerializePacker] to use Serialize, Deserialize or Clone.",
            category: "Nivaes.Serializer",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}
