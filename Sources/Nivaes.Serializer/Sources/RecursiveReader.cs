using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Nivaes.Serializer;

internal ref struct RecursiveReader
{
    private delegate void DeserializeCircularDelegate(ref RecursiveReader recursiveReader, object item, in object serializer, in Action<object> deserializerReference);

    private struct Reference
    {
        public bool IsCreate;
        public object Item;
    }

    private struct ReferenceReader
    {
        public int IdReference;
        public object Deserializer;
        public Action<object>? DeserialiceReference;
    }

    private readonly ReadOnlySpan<byte> _buffer;
    private readonly ReadOnlySpan<int> _initPositions;
    private readonly Reference[] _references;
    private readonly List<ReferenceReader> _referenceReaders;

    public RecursiveReader(ReadOnlySpan<byte> buffer)
    {
        _buffer = buffer;
        _initPositions = GetOffsets(buffer);
        _references = new Reference[_initPositions.Length];
        _referenceReaders = new List<ReferenceReader>(_initPositions.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int[] GetOffsets(ReadOnlySpan<byte> buffer)
    {
        var result = new int[buffer.Length / 4];

        int count = 0;
        int position = 0;

        while (position + 4 <= buffer.Length)
        {
            result[count++] = position + 4;

            int jump = BinaryPrimitives.ReadInt32LittleEndian(
                buffer.Slice(position, 4));

            if (jump <= 0)
                break;

            position += jump;
        }

        return result[..count];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TReference? ReadItem<TReference, TReferenceDeserializer>(int idObject, TReferenceDeserializer deserializer)
       where TReferenceDeserializer : IDeserializer<TReference>, new()
    {
        if (!_references[idObject].IsCreate)
        {
            var position = _initPositions[idObject];
            var packerReader = new PackerReader(_buffer[position..], ref this);

            _references[idObject].IsCreate = true;
            _references[idObject].Item = deserializer.Deserialize(ref packerReader)!;
        }

        return (TReference?)_references[idObject].Item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TReference? Read<TReference, TReferenceDeserializer>(int idObject, Action<object>? deserialiceReference)
        where TReferenceDeserializer : IDeserializer<TReference>, new()
    {
        var deserializer = new TReferenceDeserializer();

        var value = ReadItem<TReference, TReferenceDeserializer>(idObject, deserializer);

        if (deserialiceReference != null && value == null)
        {
            _referenceReaders.Add(new ReferenceReader
            {
                IdReference = idObject,
                Deserializer = deserializer,
                DeserialiceReference = deserialiceReference
            });
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DeserializeCircular()
    {
        foreach (var reference in _referenceReaders)
        {
            var item = _references[reference.IdReference].Item;
            reference.DeserialiceReference!(item);
        }
    }
}
