using System.Diagnostics.CodeAnalysis;

namespace Nivaes.Serializer;

public static partial class SerializeModel
{
    public static ReadOnlySpan<byte> Serialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(in T model)
    {
        throw new NotSupportedException("This method should be intercepted.");
    }

    public static ReadOnlySpan<byte> Serialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T, TSerializer>(in T value)
        where TSerializer : ISerializer<T>, new()
    {
        if (value == null)
            return [];

        var write = new PackerWriter();
        var serializer = new TSerializer();

        return write.WriteRoot(value, serializer);
    }

    public static T? Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(ReadOnlySpan<byte> cache)
    {
        throw new NotSupportedException("This method should be intercepted.");
    }

    public static T? Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T, TDeserializer>(ReadOnlySpan<byte> buffer)
           where TDeserializer : IDeserializer<T>, new()
    {
        var recursiveReader = new RecursiveReader(buffer);

        var value = recursiveReader.Read<T, TDeserializer>(0, null);

        recursiveReader.DeserializeCircular();

        return value;
    }

    public static T? Clone<T>(in T value)
    {
        throw new NotSupportedException("This method should be intercepted.");
    }

    public static T? Clone<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T, TSerializer, TDeserializer>(in T value)
        where TSerializer : ISerializer<T>, new()
        where TDeserializer : IDeserializer<T>, new()   
    {
        var buffer = Serialize<T, TSerializer>(in value);
        return Deserialize<T, TDeserializer>(buffer);
    }
}
