using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace Nivaes.Serializer;

public static partial class SerializerModel
{
    //public static ReadOnlySpan<byte> Serialize(in object model)
    //{
    //    throw new NotSupportedException("This method should be intercepted.");
    //}

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

    public static ReadOnlySpan<byte> Serialize(uint idType, in object value)
    {
        return SerializersCollections.Serialize(idType, in value);
    }

    public static ReadOnlySpan<byte> Serialize(Type type, in object value)
    {
        return SerializersCollections.Serialize(type, in value);
    }

    //public static object? Deserialize(ReadOnlySpan<byte> buffer)
    //{
    //    throw new NotSupportedException("This method should be intercepted.");
    //}

    public static T? Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>()
    {
        throw new NotSupportedException("This method should be intercepted.");
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

    public static object? Deserialize(uint idType, ReadOnlySpan<byte> buffer)
    {
        return SerializersCollections.Deserialize(idType, buffer);
    }

    public static object? Deserialize(Type type, ReadOnlySpan<byte> buffer)
    {
        return SerializersCollections.Deserialize(type, buffer);
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

    public static void Register<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>()
    {
        throw new NotSupportedException("This method should be intercepted.");
    }

    public static void Register<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T,
        TSerializer,
        TDeserializer>(uint idClass)
        where TSerializer : ISerializer<T>, new()
        where TDeserializer : IDeserializer<T>, new()
    {
        SerializersCollections.Register<T, TSerializer, TDeserializer>(idClass);
    }

    public static uint GetIdClass<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>()
    {
        throw new NotSupportedException("This method should be intercepted.");
    }
}
