using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Nivaes.Serializer;

public static class SerializersCollections
{
    internal delegate byte[] SerializeDelegate(in object value);
    internal delegate object? DeserializeDelegate(ReadOnlySpan<byte> buffer);

    internal static readonly Dictionary<Type, SerializeDelegate> TypeSerializers = new();

    internal static readonly Dictionary<Type, DeserializeDelegate> TypeDeserializers = new();

    internal static readonly Dictionary<int, SerializeDelegate> IdTypeSerializers = new();

    internal static readonly Dictionary<int, DeserializeDelegate> IdTypeDeserializers = new();


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
}
