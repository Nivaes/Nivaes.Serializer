using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Nivaes.Serializer;

public ref partial struct PackerWriter
{
    private const int InitialCapacity = 2048;
    private byte[] _buffer;
    private Span<byte> _span;
    private int _position;
    private int _firstPosition;

    #region Recursive
    private delegate void SerializeDelegate(scoped in object item, scoped in object serializer, ref PackerWriter writer);

    private struct Entry
    {
        public int Hash;
        public object Serializer;
        public object Item;
        public SerializeDelegate Serialize;
    }

    private const int EntriesCapacity = 5;

    private Entry[] _entries;

    private int _count;

    private int _serializeCount;
    #endregion

    public PackerWriter()
    { 
        _buffer = GC.AllocateUninitializedArray<byte>(InitialCapacity);
        _span = _buffer;
        _firstPosition = 0;
        _position = 0;

        _entries = new Entry[EntriesCapacity];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly byte[] ToArray()
    {
        return _span[.._position].ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<byte> ToSpan()
    {
        return _span[.._position];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Ensure(int size)
    {
        if ((uint)(_position + size) <= (uint)_span.Length)
            return;

        Grow(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Grow(int size)
    {
        int required = _position + size;

        int newSize = Math.Max(
            required,
            Math.Max(_span.Length * 2, 256));

        var newBuffer =
            GC.AllocateUninitializedArray<byte>(newSize);

        _span[.._position].CopyTo(newBuffer);

        _buffer = newBuffer;
        _span = newBuffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ReserveSizeInt32()
    {
        _position += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteSize()
    {
        var size = _position - _firstPosition;
        BinaryPrimitives.WriteInt32LittleEndian(
           _span[_firstPosition..],
           size);

        _firstPosition = _position;
    }

    #region Writers
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(byte value)
    {
        Ensure(1);

        _span[_position++] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(bool value)
    {
        Write(value ? (byte)1 : (byte)0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(short value)
    {
        Ensure(2);

        BinaryPrimitives.WriteInt16LittleEndian(
            _span[_position..],
            value);

        _position += 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ushort value)
    {
        Ensure(2);

        BinaryPrimitives.WriteUInt16LittleEndian(
            _span[_position..],
            value);

        _position += 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(int value)
    {
        Ensure(4);

        BinaryPrimitives.WriteInt32LittleEndian(
            _span[_position..],
            value);

        _position += 4;
    }

    public void Write(int? value)
    {
        Write(value.HasValue);

        if (value.HasValue)
        {
            Ensure(4);

            BinaryPrimitives.WriteInt32LittleEndian(
                _span[_position..],
                value.Value);

            _position += 4;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(uint value)
    {
        Ensure(4);

        BinaryPrimitives.WriteUInt32LittleEndian(
            _span[_position..],
            value);

        _position += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(long value)
    {
        Ensure(8);

        BinaryPrimitives.WriteInt64LittleEndian(
            _span[_position..],
            value);

        _position += 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ulong value)
    {
        Ensure(8);

        BinaryPrimitives.WriteUInt64LittleEndian(
            _span[_position..],
            value);

        _position += 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(float value)
    {
        Write(BitConverter.SingleToInt32Bits(value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(double value)
    {
        Write(BitConverter.DoubleToInt64Bits(value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(decimal value)
    {
        Ensure(16);

        Span<int> bits = stackalloc int[4];

        decimal.GetBits(value, bits);

        MemoryMarshal.AsBytes(bits)
            .CopyTo(_span[_position..]);

        _position += 16;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(Half value)
    {
        Write(BitConverter.HalfToUInt16Bits(value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(Guid value)
    {
        Ensure(16);

        value.TryWriteBytes(_span[_position..]);

        _position += 16;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(DateTime value)
    {
        Write(value.Ticks);
        Write((byte)value.Kind);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(DateTimeOffset value)
    {
        Write(value.Ticks);
        Write((short)value.Offset.TotalMinutes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(string? value)
    {
        if (value is null)
        {
            Write(-1);
            return;
        }
        if(string.IsNullOrEmpty(value))
        {
            Write(-2);
            return;
        }

        int byteCount = Encoding.UTF8.GetByteCount(value);

        Write(byteCount);

        Ensure(byteCount);

        int written = Encoding.UTF8.GetBytes(
            value,
            _span[_position..]);

        _position += written;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(byte[]? value)
    {
        if (value is null)
        {
            Write(-1);
            return;
        }

        Write(value.Length);

        Ensure(value.Length);

        value.AsSpan().CopyTo(_span[_position..]);

        _position += value.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(Memory<byte>? value)
    {
        if (value is null)
        {
            Write(-1);
            return;
        }

        var memory = value.Value;

        Write(memory.Length);

        Ensure(memory.Length);

        memory.Span.CopyTo(_span[_position..]);

        _position += memory.Length;
    }    
    #endregion

    #region Reference writers
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write<T, TSerializer>(in T? value)
        where TSerializer : ISerializer<T>, new()
    {
        if (value is null)
        {
            Write(-1);
            return;
        }

        var id = Add<T>(value, new TSerializer());

        Write(id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write<T, TSerializer>(T[]? value)
         where TSerializer : ISerializer<T>, new()
    {
        if (value is null)
        {
            Write(-1);
            return;
        }

        Write(value.Length);

        foreach (var item in value)
        {
            Write<T, TSerializer>(item);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteArray<T, TSerializer>(Array? value)
         where TSerializer : ISerializer<T>, new()
    {
        if (value is null)
        {
            Write(-1);
            return;
        }

        // Número de dimensiones.
        Write(value.Rank);

        // Longitud de cada dimensión.
        for (int i = 0; i < value.Rank; i++)
            Write(value.GetLength(i));

        // Serializar elementos.
        foreach (T item in value)
            Write<T, TSerializer>(item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write<T, TSerializer>(IEnumerable<T>? value)
         where TSerializer : ISerializer<T>, new()
    {
        if (value is null)
        {
            Write(-1);
            return;
        }

        Write(value.Count());

        foreach (var item in value)
        {
            Write<T, TSerializer>(item);
        }
    }
    #endregion
}
