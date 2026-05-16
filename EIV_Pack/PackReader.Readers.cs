using EIV_Pack.Formatters;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EIV_Pack;

public ref partial struct PackReader
{
    /// <summary>
    /// Reads an <see langword="unmanaged"/> type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>The readed <see langword="unmanaged"/> type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when cannot read the size of the type.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T ReadUnmanaged<T>() where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        if (Remaining < size)
        {
            throw new InvalidOperationException("Remaining bytes cannot read this type!");
        }

#if !NETSTANDARD2_0
        T value = MemoryMarshal.Read<T>(currentBuffer[..size]);
#else
        T value = MemoryMarshal.Read<T>(currentBuffer.Slice(0, size));
#endif

        Advance(size);
        return value;
    }

    /// <summary>
    /// Reads an <see langword="unmanaged"/> nullable type.
    /// </summary>
    /// <typeparam name="T">Any <see langword="unmanaged"/> type</typeparam>
    /// <returns>The value or <see langword="null"/>.</returns>
    public T? ReadUnmanagedNullable<T>() where T : unmanaged
    {
        return ReadUnmanaged<byte>() != 0 ? ReadUnmanaged<T>() : default(T?);
    }

    /// <summary>
    /// Tries to read <see langword="unmanaged"/> type.
    /// </summary>
    /// <typeparam name="T">Any <see langword="unmanaged"/> type</typeparam>
    /// <param name="value">The value or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if successfully read the <paramref name="value"/>; otherwise, <see langword="false"/>.</returns>
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
#if !NETSTANDARD2_0
        [..size];
#else
        .Slice(0, size);
#endif

        value = MemoryMarshal.Read<T>(buffer);
        return true;
    }

    /// <summary>
    /// Reads an <see langword="int"/> header type.
    /// </summary>
    /// <returns>The header.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadHeader()
    {
        return ReadUnmanaged<int>();
    }

    /// <summary>
    /// Tries to peak the header value.
    /// </summary>
    /// <param name="value">The value or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if successfully peeked the <paramref name="value"/>; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryPeekHeader(out int value)
    {
        return TryPeekUnmanaged(out value);
    }

    /// <summary>
    /// Tries to read header value.
    /// </summary>
    /// <param name="value">The value or 0.</param>
    /// <returns><see langword="true"/> if successfully read the <paramref name="value"/>; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadHeader(out int value)
    {
        if (!TryPeekUnmanaged(out value))
            return false;

        Advance(4);
        return true;
    }

    /// <summary>
    /// Tries to read small header value.
    /// </summary>
    /// <param name="value">The value or 0.</param>
    /// <returns><see langword="true"/> if successfully read the <paramref name="value"/>; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadSmallHeader(out byte value)
    {
        if (!TryPeekUnmanaged(out value))
            return false;

        Advance(1);
        return true;
    }

    /// <summary>
    /// Peeks header and checks if the value is null.
    /// </summary>
    /// <returns><see langword="true"/> if null header or cant read header value; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool PeekIsNullOrEmpty()
    {
        if (!TryPeekHeader(out int len))
            return true;

        return len == Constants.NullHeader;
    }

    /// <summary>
    /// Read nullable string value.
    /// </summary>
    /// <returns><see langword="null"/> if cannot read the value; otherwise, the read <see cref="string"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string? ReadString()
    {
        if (!TryReadHeader(out int len))
        {
            return null;
        }

        if (len == 0)
            return string.Empty;

#if !NETSTANDARD2_0
        string str = TextEncoding.GetString(currentBuffer[..len]);
#else
        string str = TextEncoding.GetString(currentBuffer.Slice(0, len).ToArray());
#endif

        Advance(len);
        return str;
    }

    /// <summary>
    /// Reads <see cref="IPackable{T}"/> value.
    /// </summary>
    /// <typeparam name="T">Any <see cref="IPackable{T}"/> type.</typeparam>
    /// <param name="value">The value to read into.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadPackable<T>(scoped ref T? value)
        where T : IPackable<T>
    {
#if !NETSTANDARD2_0
        T.DeserializePackable(ref this, ref value);
#else
        ReadValue(ref value);
#endif

    }

    /// <summary>
    /// Reads <see cref="IPackable{T}"/> value.
    /// </summary>
    /// <typeparam name="T">Any <see cref="IPackable{T}"/> type.</typeparam>
    /// <returns>The read <typeparamref name="T"/> value; otherwise, <see langword="default"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? ReadPackable<T>()
        where T : IPackable<T>
    {
        T? value = default;
#if !NETSTANDARD2_0
        T.DeserializePackable(ref this, ref value);
#else
        ReadValue(ref value);
#endif
        return value;
    }

    /// <summary>
    /// Reads <typeparamref name="T"/> type <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T">Any type that are registered.</typeparam>
    /// <param name="value">The read value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadValue<T>(scoped ref T? value)
    {
        FormatterProvider.GetFormatter<T>().Deserialize(ref this, ref value);
    }

    /// <summary>
    /// Read the <paramref name="value"/> with provided <paramref name="formatter"/>.
    /// </summary>
    /// <typeparam name="T">Any type that are registered.</typeparam>
    /// <param name="value">The read value.</param>
    /// <param name="formatter">The formatter to deserialize the value with.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadValue<T>(scoped ref T? value, IFormatter<T> formatter)
    {
        formatter.Deserialize(ref this, ref value);
    }

    /// <summary>
    /// Read object <paramref name="value"/> with provided <paramref name="formatter"/>.
    /// </summary>
    /// <param name="value">The read value.</param>
    /// <param name="formatter">The formatter to deserialize the value with.</param>
    public void ReadValue(scoped ref object? value, IFormatter formatter)
    {
        formatter.Deserialize(ref this, ref value);
    }

    /// <summary>
    /// Reads <typeparamref name="T"/> type.
    /// </summary>
    /// <typeparam name="T">Any type that are registered.</typeparam>
    /// <returns>The read <typeparamref name="T"/> value; otherwise, <see langword="default"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? ReadValue<T>()
    {
        T? value = default;
        FormatterProvider.GetFormatter<T>().Deserialize(ref this, ref value);
        return value;
    }

    /// <summary>
    /// Reads <typeparamref name="T"/> value with provided <paramref name="formatter"/>.
    /// </summary>
    /// <typeparam name="T">Any type that are registered.</typeparam>
    /// <param name="formatter">The formatter to deserialize the value with.</param>
    /// <returns>The read <typeparamref name="T"/> value; otherwise, <see langword="default"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? ReadValueWithFormatter<T>(IFormatter<T> formatter)
    {
        T? value = default;
        formatter.Deserialize(ref this, ref value);
        return value;
    }

    /// <summary>
    /// Reads <typeparamref name="T"/> array.
    /// </summary>
    /// <typeparam name="T">Any type that are registered.</typeparam>
    /// <returns>The read <typeparamref name="T"/> array; otherwise, <see langword="default"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T?[]? ReadArray<T>()
    {
        T?[]? value = default;
        ReadArray(ref value);
        return value;
    }

    /// <summary>
    /// Reads <typeparamref name="T"/> array into <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T">Any type that are registered.</typeparam>
    /// <param name="value">The value reference to read to.</param>
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

    /// <summary>
    /// Reads <typeparamref name="T"/> array into <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T">Any type that are registered.</typeparam>
    /// <param name="value">The value reference to read to.</param>
    /// <param name="length">The length of the values to read.</param>
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

    /// <summary>
    /// Reads an array of nullable unmanaged values of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Any <see langword="unmanaged"/> registered type.</typeparam>
    /// <returns>The read <typeparamref name="T"/> array; otherwise, <see langword="default"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T?[]? ReadArrayUnmanaged<T>() where T : unmanaged
    {
        T?[]? value = default;
        ReadArrayUnmanaged(ref value);
        return value;
    }

    /// <summary>
    /// Reads an array of nullable unmanaged values of type <typeparamref name="T"/> into <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T">Any <see langword="unmanaged"/> registered type.</typeparam>
    /// <param name="value">The value reference to read to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadArrayUnmanaged<T>(scoped ref T?[]? value) where T : unmanaged
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
            value = new T?[length];
        }

        for (int i = 0; i < length; i++)
        {
            value[i] = ReadUnmanagedNullable<T>();
        }
    }

    /// <summary>
    /// Reads an array of nullable unmanaged values of type <typeparamref name="T"/> into <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T">Any <see langword="unmanaged"/> registered type.</typeparam>
    /// <param name="value">The value reference to read to.</param>
    /// <param name="length">The length of the values to read.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadArrayUnmanaged<T>(scoped ref T?[]? value, int length) where T : unmanaged
    {
        if (length == 0)
        {
            value = [];
            return;
        }

        if (value == null || value.Length != length)
        {
            value = new T?[length];
        }

        for (int i = 0; i < length; i++)
        {
            value[i] = ReadUnmanagedNullable<T>();
        }
    }
}
