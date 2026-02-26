using EIV_Pack.Formatters;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace EIV_Pack;

public ref struct PackReader
{
    private readonly ReadOnlySequence<byte> bufferSource;
    private ReadOnlySpan<byte> currentBuffer;
    public readonly Encoding TextEncoding;
    public readonly long Length;
    public int Consumed { get; private set; } = 0;
    public readonly long Remaining => Length - Consumed;

    public PackReader(in ReadOnlySequence<byte> sequence)
    {
        bufferSource = sequence;
        currentBuffer = sequence.First.Span;
        Length = sequence.Length;
        TextEncoding = Encoding.UTF8;
    }

    public PackReader(ReadOnlySpan<byte> buffer)
    {
        bufferSource = new ReadOnlySequence<byte>(buffer.ToArray());
        currentBuffer = buffer;
        Length = buffer.Length;
        TextEncoding = Encoding.UTF8;
    }

    public void Advance(int count)
    {
        if (count == 0)
            return;

        if (Remaining < count)
        {
            throw new InvalidOperationException("Remaining bytes cannot read this type!");
        }

        currentBuffer = bufferSource.Slice(Consumed + count, Remaining - count).First.Span;
        Consumed += count;
    }

    public void SetConsumed(int consumed)
    {
        if (consumed > Length)
        {
            return;
        }

        Consumed = consumed;
        currentBuffer = bufferSource.Slice(Consumed, Remaining).First.Span;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T ReadUnmanaged<T>() where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        if (Remaining < size)
        {
            throw new InvalidOperationException("Remaining bytes cannot read this type!");
        }

#if NET8_0_OR_GREATER
        T value = MemoryMarshal.Read<T>(currentBuffer[..size]);
#else
        T value = MemoryMarshal.Read<T>(currentBuffer.Slice(0, size));
#endif

        Advance(size);
        return value;
    }

    public T? ReadUnmanagedNullable<T>() where T : unmanaged
    {
        return ReadUnmanaged<byte>() != 0 ? ReadUnmanaged<T>() : default(T?);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadHeader()
    {
        return ReadUnmanaged<int>();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryPeekUnmanaged<T>(out T value) where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        if (Remaining < size)
        {
            value = default;
            return false;
        }

        ReadOnlySpan<byte> buffer = currentBuffer
#if NET8_0_OR_GREATER
        [..size];
#else
        .Slice(0, size);
#endif

        value = MemoryMarshal.Read<T>(buffer);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryPeekHeader(out int length)
    {
        return TryPeekUnmanaged(out length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadHeader(out int length)
    {
        if (!TryPeekUnmanaged(out length))
            return false;

        Advance(4);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadSmallHeader(out byte length)
    {
        if (!TryPeekUnmanaged(out length))
            return false;

        Advance(1);
        return true;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool PeekIsNullOrEmpty()
    {
        if (!TryPeekHeader(out int len))
            return true;

        return len == Constants.NullHeader;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string? ReadString()
    {
        if (!TryReadHeader(out int len))
        {
            return null;
        }

        if (len == 0)
            return string.Empty;

#if NET8_0_OR_GREATER
        string str = TextEncoding.GetString(currentBuffer[..len]);
#else
        string str = TextEncoding.GetString(currentBuffer.Slice(0, len).ToArray());
#endif

        Advance(len);
        return str;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadPackable<T>(scoped ref T? value)
        where T : IPackable<T>
    {
#if NET8_0_OR_GREATER
        T.DeserializePackable(ref this, ref value);
#else
        ReadValue(ref value);
#endif

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? ReadPackable<T>()
        where T : IPackable<T>
    {
        T? value = default;
#if NET8_0_OR_GREATER
        T.DeserializePackable(ref this, ref value);
#else
        ReadValue(ref value);
#endif
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadValue<T>(scoped ref T? value)
    {
        FormatterProvider.GetFormatter<T>().Deserialize(ref this, ref value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadValue<T>(scoped ref T? value, IFormatter<T> formatter)
    {
        formatter.Deserialize(ref this, ref value);
    }

    public void ReadValue(scoped ref object? value, IFormatter formatter)
    {
        formatter.Deserialize(ref this, ref value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? ReadValue<T>()
    {
        T? value = default;
        FormatterProvider.GetFormatter<T>().Deserialize(ref this, ref value);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? ReadValueWithFormatter<T>(IFormatter<T> formatter)
    {
        T? value = default;
        formatter.Deserialize(ref this, ref value);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T?[]? ReadArray<T>()
    {
        T?[]? value = default;
        ReadArray(ref value);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadArray<T>(scoped ref T?[]? value)
    {
        if (!TryReadHeader(out int length) || length == Constants.NullHeader)
        {
            value = null;
            return;
        }

        if (length == 0)
        {
            value = [];
            return;
        }

        if (value == null || value.Length != length)
        {
            value = new T[length];
        }

        IFormatter<T> formatter = FormatterProvider.GetFormatter<T>();
        for (int i = 0; i < length; i++)
        {
            formatter.Deserialize(ref this, ref value[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadArray<T>(scoped ref T?[]? value, int length)
    {
        if (length == 0)
        {
            value = [];
            return;
        }

        if (value == null || value.Length != length)
        {
            value = new T[length];
        }

        IFormatter<T> formatter = FormatterProvider.GetFormatter<T>();
        for (int i = 0; i < length; i++)
        {
            formatter.Deserialize(ref this, ref value[i]);
        }
    }
}
