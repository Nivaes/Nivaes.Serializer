using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Nivaes.Serializer;

public static class SerializersCollections
{
    private delegate byte[] SerializeDelegate(in object value);
    private delegate object? DeserializeDelegate(ReadOnlySpan<byte> buffer);

    private static readonly Dictionary<Type, SerializeDelegate> TypeSerializers = new();

    private static readonly Dictionary<Type, DeserializeDelegate> TypeDeserializers = new();

    private static readonly Dictionary<int, SerializeDelegate> IdTypeSerializers = new();

    private static readonly Dictionary<int, DeserializeDelegate> IdTypeDeserializers = new();

    public static void Register<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>()
    {
        throw new NotSupportedException("This method should be intercepted.");
    }

    public static void Register<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T,
        TSerializer,
        TDeserializer>(int idClass)
        where TSerializer : ISerializer<T>, new()
        where TDeserializer : IDeserializer<T>, new()
    {
        TypeSerializers.Add(typeof(T), Serialize<T, TSerializer>);
        IdTypeSerializers.Add(idClass, Serialize<T, TSerializer>);

        TypeDeserializers.Add(typeof(T), Deserialize<T, TDeserializer>);
        IdTypeDeserializers.Add(idClass, Deserialize<T, TDeserializer>);
    }

    private static byte[] Serialize<T, TSerializer>(in object value)
         where TSerializer : ISerializer<T>, new()
    {
        return SerializerModel.Serialize<T, TSerializer>((T)value).ToArray();
    }

    private static object? Deserialize<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T,
        TDeserializer>(ReadOnlySpan<byte> buffer)
        where TDeserializer : IDeserializer<T>, new()
    {
        return SerializerModel.Deserialize<T, TDeserializer>(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ReadOnlySpan<byte> Serialize(int idType, in object value)
    {
        if (SerializersCollections.IdTypeSerializers.TryGetValue(idType, out var serializer))
        {
            return serializer(value);
        }

        throw new Exception($"Type {idType} not registered.");
    }

    internal static ReadOnlySpan<byte> Serialize(Type type, in object value)
    {
        if (SerializersCollections.TypeSerializers.TryGetValue(type, out var serializer))
        {
            return serializer(value);
        }

        throw new Exception($"Type {type.FullName!} not registered.");
    }

    internal static object? Deserialize(int idType, ReadOnlySpan<byte> buffer)
    {
        if (SerializersCollections.IdTypeDeserializers.TryGetValue(idType, out var serializer))
        {
            return serializer(buffer);
        }

        throw new Exception($"Type {idType} not registered.");
    }

    internal static object? Deserialize(Type type, ReadOnlySpan<byte> buffer)
    {
        if (SerializersCollections.TypeDeserializers.TryGetValue(type, out var serializer))
        {
            return serializer(buffer);
        }

        throw new Exception($"Type {type.FullName!} not registered.");
    }
}
