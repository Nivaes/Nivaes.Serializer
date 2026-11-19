using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Nivaes.Serializer.SourceGenerator;

internal static class SourceGenerationHelper
{
    private static string GetPrefixSerializerClassName(INamedTypeSymbol symbol)
    {
        var names = new List<string>();

        for (var current = symbol;
             current is not null;
             current = current.ContainingType)
        {
            names.Add(current.Name);
        }

        names.Reverse();

        return string.Join("_", names);
    }

    public static string GetSerializerClassName(this INamedTypeSymbol symbol)
    {
        return GetPrefixSerializerClassName(symbol) + "Serializer";
    }

    public static string GetDeserializerClassName(this INamedTypeSymbol symbol)
    {
        return GetPrefixSerializerClassName(symbol) + "Deserializer";
    }

    public static uint GetIdClass(INamedTypeSymbol model)
    {
        var identity =
            $"{model.ContainingAssembly.Identity.Name}:{model.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat)}";

        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(identity));

        return unchecked(
            ((uint)bytes[0] << 24) |
            ((uint)bytes[1] << 16) |
            ((uint)bytes[2] << 8) |
            bytes[3]);
    }
}