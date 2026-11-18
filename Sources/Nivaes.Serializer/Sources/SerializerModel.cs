using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace Nivaes.Serializer;

public static partial class SerializerModel
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

    public static ReadOnlySpan<byte> Serialize(int idType, in object value)
    {
        if(SerializersCollections.IdTypeSerializers.TryGetValue(idType, out var serializer))
        {
            return serializer(value);
        }

        throw new Exception($"Type {idType} not registered.");
    }

    public static ReadOnlySpan<byte> Serialize(Type type, in object value)
    {
        if (SerializersCollections.TypeSerializers.TryGetValue(type, out var serializer))
        {
            return serializer(value);
        }

        throw new Exception($"Type {type.FullName!} not registered.");
    }

    public static T? Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(ReadOnlySpan<byte> buffer)
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

    public static object? Deserialize(int idType, ReadOnlySpan<byte> buffer)
    {
        if (SerializersCollections.IdTypeDeserializers.TryGetValue(idType, out var serializer))
        {
            return serializer(buffer);
        }

        throw new Exception($"Type {idType} not registered.");
    }

    public static object? Deserialize(Type type, ReadOnlySpan<byte> buffer)
    {
        if (SerializersCollections.TypeDeserializers.TryGetValue(type, out var serializer))
        {
            return serializer(buffer);
        }

        throw new Exception($"Type {type.FullName!} not registered.");
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
