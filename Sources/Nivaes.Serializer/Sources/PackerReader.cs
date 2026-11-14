using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Nivaes.Serializer;

public ref struct PackerReader
{
    private readonly ReadOnlySpan<byte> _spanBuffer;
    private int _position;

    private RecursiveReader _recursiveReader;

    public delegate void ReferenceSetterDelegate<TReference>(object value, TReference reference);

    internal PackerReader(ReadOnlySpan<byte> buffer, ref RecursiveReader recursiveReader)
    {
        _spanBuffer = buffer;
        _recursiveReader = recursiveReader;
    }

    #region Reads
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ReadOnlySpan<byte> ReadSpan(int size)
    {
        if ((uint)(_position + size) > (uint)_spanBuffer.Length)
            throw new InvalidOperationException(
                "Unexpected end of buffer.");

        var result = _spanBuffer.Slice(_position, size);

        _position += size;

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadByte()
    {
        return ReadSpan(1)[0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ReadBoolean()
    {
        return ReadByte() != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short ReadInt16()
    {
        var span = ReadSpan(2);

        return BinaryPrimitives.ReadInt16LittleEndian(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort ReadUInt16()
    {
        var span = ReadSpan(2);

        return BinaryPrimitives.ReadUInt16LittleEndian(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadInt32()
    {
        var span = ReadSpan(4);

        return BinaryPrimitives.ReadInt32LittleEndian(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int? ReadNullableInt32()
    {
        var hasValue = ReadBoolean();

        if (!hasValue)
            return default;

        var span = ReadSpan(4);

        return BinaryPrimitives.ReadInt32LittleEndian(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ReadUInt32()
    {
        var span = ReadSpan(4);

        return BinaryPrimitives.ReadUInt32LittleEndian(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long ReadInt64()
    {
        var span = ReadSpan(8);

        return BinaryPrimitives.ReadInt64LittleEndian(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong ReadUInt64()
    {
        var span = ReadSpan(8);

        return BinaryPrimitives.ReadUInt64LittleEndian(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadSingle()
    {
        return BitConverter.Int32BitsToSingle(ReadInt32());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ReadDouble()
    {
        return BitConverter.Int64BitsToDouble(ReadInt64());
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Half ReadHalf()
    {
        return BitConverter.UInt16BitsToHalf(ReadUInt16());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public decimal ReadDecimal()
    {
        Span<int> bits = stackalloc int[4];

        _spanBuffer
            .Slice(_position, 16)
            .CopyTo(MemoryMarshal.AsBytes(bits));

        _position += 16;

        return new decimal(bits);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Guid ReadGuid()
    {
        var span = ReadSpan(16);

        return new Guid(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DateTime ReadDateTime()
    {
        long ticks = ReadInt64();
        var kind = (DateTimeKind)ReadByte();

        return new DateTime(ticks, kind);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DateTimeOffset ReadDateTimeOffset()
    {
        long ticks = ReadInt64();
        short offsetMinutes = ReadInt16();

        return new DateTimeOffset(
            ticks,
            TimeSpan.FromMinutes(offsetMinutes));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadString()
    {
        int byteCount = ReadInt32();

        if (byteCount == -1)
            return null!;
        if (byteCount < -1)
            return string.Empty;

        var span = ReadSpan(byteCount);

        return Encoding.UTF8.GetString(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[]? ReadBytes()
    {
        int length = ReadInt32();

        if (length < 0)
            return null;

        var result = GC.AllocateUninitializedArray<byte>(length);

        ReadSpan(length).CopyTo(result);

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Memory<byte> ReadMemory()
    {
        int length = ReadInt32();

        if (length < 0)
            return null;

        var result = GC.AllocateUninitializedArray<byte>(length);

        ReadSpan(length).CopyTo(result);

        return result.AsMemory();
    }
    #endregion

    #region Reference readers
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TReference? ReadReference<T, TReference, TSerializer>(Action<object>? deserialiceReference = null)
        where TSerializer : IDeserializer<TReference>, new()
    {
        int idObject = ReadInt32();

        if (idObject == -1)
            return default(TReference);

        return _recursiveReader.Read<TReference, TSerializer>(idObject, deserialiceReference);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TReference[]? ReadArray<T, TReference, TSerializer>(Action<object>? deserialiceReference = null)
        where TSerializer : IDeserializer<TReference>, new()
    {
        int size = ReadInt32();

        if (size == -1)
            return default(TReference[]);

        if (size < -1)
            throw new InvalidOperationException("Invalid enumerable size.");

        if (size == 0)
            return [];

        var items = new TReference[size];
        for (int i = 0; i < size; i++)
        {
            items[i] = ReadReference<T, TReference, TSerializer>(deserialiceReference)!;
        }

        return items.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Array? ReadMultyArray<T, TReference, TSerializer>(Action<object>? deserializeReference = null)
    where TSerializer : IDeserializer<TReference>, new()
    {
        int rank = ReadInt32();

        if (rank == -1)
            return null;

        if (rank <= 0)
            throw new InvalidOperationException("Invalid array rank.");

        // Leer las dimensiones.
        var lengths = new int[rank];

        for (int i = 0; i < rank; i++)
        {
            lengths[i] = ReadInt32();

            if (lengths[i] < 0)
                throw new InvalidOperationException("Invalid array dimension.");
        }

        // Crear el array multidimensional.
        var array = Array.CreateInstance(typeof(TReference), lengths);

        if (array.Length == 0)
            return array;

        // Leer los elementos.
        var indices = new int[rank];

        for (int i = 0; i < array.Length; i++)
        {
            array.SetValue(
                ReadReference<T, TReference, TSerializer>(deserializeReference),
                indices);

            // Incrementar índices multidimensionales.
            for (int dimension = rank - 1; dimension >= 0; dimension--)
            {
                indices[dimension]++;

                if (indices[dimension] < lengths[dimension])
                    break;

                indices[dimension] = 0;
            }
        }

        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TReference>? ReadEnumerable<T, TReference, TSerializer>(Action<object>? deserialiceReference = null)
        where TSerializer : IDeserializer<TReference>, new()
    {
        int size = ReadInt32();

        if (size == -1)
            return default(TReference[]);

        if (size < -1)
            throw new InvalidOperationException("Invalid enumerable size.");

        if (size == 0)
            return [];

        var items = new TReference[size];
        for (int i = 0; i < size; i++)
        {
            items[i] = ReadReference<T, TReference, TSerializer>(deserialiceReference)!;
        }

        return items.ToArray();
    }
    #endregion
}
