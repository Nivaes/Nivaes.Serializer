namespace Nivaes.Serializer;

public interface IDeserializer<T>
{
    T Deserialize(ref PackerReader reader);
}
