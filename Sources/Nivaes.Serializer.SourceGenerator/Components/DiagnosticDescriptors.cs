using Microsoft.CodeAnalysis;

namespace Nivaes.Serializer.SourceGenerator
{
    internal static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor UnsupportedProperty =
            new(
             id: "SPK001",
             title: "Unsupported property type",
             messageFormat: "Property '{0}' has an unsupported type '{1}'.",
             category: "SerializePacker",
             DiagnosticSeverity.Error,
             isEnabledByDefault: true);
    }
}
