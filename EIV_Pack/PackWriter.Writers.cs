using EIV_Pack.Formatters;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EIV_Pack;

public ref partial struct PackWriter : IDisposable
{
    /// <summary>
    /// Writes the unmanaged value into the buffer.
    /// </summary>
    /// <typeparam name="T">The unmanaged type of the value to write.</typeparam>
    /// <param name="value">The value to write into the buffer.</param>
    public readonly unsafe void WriteUnmanaged<T>(scoped in T value) where T : unmanaged
    {
        int size = sizeof(T);
        Span<byte> span = recyclable.GetSpan(size);
#if !NETSTANDARD2_0
        MemoryMarshal.Write(span, value);
#else
        T val = value;
        MemoryMarshal.Write(span, ref val);
#endif
        recyclable.Advance(size);
    }

    /// <summary>
    /// Writes the unmanaged nullable value into the buffer.
    /// </summary>
    /// <typeparam name="T">The unmanaged type of the value to write.</typeparam>
    /// <param name="value">The value to write into the buffer.</param>
    public readonly void WriteUnmanagedNullable<T>(scoped in T? value) where T : unmanaged
    {
        WriteUnmanaged(value.HasValue);

        if (value.HasValue)
            WriteUnmanaged(value.Value);
    }

    /// <summary>
    /// Writes header (length) into the buffer.
    /// </summary>
    /// <param name="length">The header value.</param>
    public readonly void WriteHeader(int length = Constants.NullHeader)
    {
        WriteUnmanaged(length);
    }

    /// <summary>
    /// Writes small header (length) into the buffer.
    /// </summary>
    /// <param name="length">The small header value.</param>
    public readonly void WriteSmallHeader(byte length = Constants.SmallNullHeader)
    {
        WriteUnmanaged(length);
    }

    /// <summary>
    /// Wrties string vlaue into the buffer.
    /// </summary>
    /// <param name="value">The string value.</param>
    public readonly void WriteString(string? value)
    {
        if (value == null)
        {
            WriteHeader();
            return;
        }

        if (value.Length == 0)
        {
            WriteHeader(0);
            return;
        }

        WriteHeader(value.Length);
        byte[] data = TextEncoding.GetBytes(value);
        recyclable.Write(data);
    }

    /// <summary>
    /// Writes an array into the buffer.
    /// </summary>
    /// <typeparam name="T">Any type.</typeparam>
    /// <param name="array">The values to write.</param>
    public void WriteArray<T>(T?[]? array)
    {
        if (array == null)
        {
            WriteHeader();
            return;
        }


        IFormatter<T> formatter = FormatterProvider.GetFormatter<T>();

        WriteHeader(array.Length);
        for (int i = 0; i < array.Length; i++)
        {
            formatter.Serialize(ref this, ref array[i]);
        }
    }

    /// <summary>
    /// Writes an unmanaged array into the buffer.
    /// </summary>
    /// <typeparam name="T">Any type.</typeparam>
    /// <param name="array">The values to write.</param>
    public readonly void WriteArrayUnmanaged<T>(T?[]? array) where T : unmanaged
    {
        if (array == null)
        {
            WriteHeader();
            return;
        }

        WriteHeader(array.Length);
        for (int i = 0; i < array.Length; i++)
        {
            WriteUnmanagedNullable(in array[i]);
        }
    }

    /// <summary>
    /// Writes a span into the buffer.
    /// </summary>
    /// <typeparam name="T">Any type.</typeparam>
    /// <param name="value">The values to write.</param>
    /// <param name="useHeader">Whether it should write the length.</param>
    public void WriteSpan<T>(scoped Span<T?> value, bool useHeader = true)
    {
        IFormatter<T> formatter = FormatterProvider.GetFormatter<T>();

        if (useHeader)
            WriteHeader(value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            formatter.Serialize(ref this, ref value[i]);
        }
    }

    /// <summary>
    /// Writes an unmanaged span into the buffer.
    /// </summary>
    /// <typeparam name="T">Any type.</typeparam>
    /// <param name="value">The values to write.</param>
    /// <param name="useHeader">Whether it should write the length.</param>
    public readonly void WriteSpanUnmanaged<T>(scoped Span<T?> value, bool useHeader = true) where T : unmanaged
    {
        if (useHeader)
            WriteHeader(value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            WriteUnmanagedNullable(in value[i]);
        }
    }

    /// <summary>
    /// Writes a readonly span into the buffer.
    /// </summary>
    /// <typeparam name="T">Any type.</typeparam>
    /// <param name="value">The values to write.</param>
    /// <param name="useHeader">Whether it should write the length.</param>
    public void WriteSpan<T>(scoped ReadOnlySpan<T?> value, bool useHeader = true)
    {
        IFormatter<T> formatter = FormatterProvider.GetFormatter<T>();

        if (useHeader)
            WriteHeader(value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            formatter.Serialize(ref this, ref Unsafe.AsRef(in value[i]));
        }
    }

    /// <summary>
    /// Writes a readonly unmanaged span into the buffer.
    /// </summary>
    /// <typeparam name="T">Any type.</typeparam>
    /// <param name="value">The values to write.</param>
    /// <param name="useHeader">Whether it should write the length.</param>
    public readonly void WriteSpanUnmanaged<T>(scoped ReadOnlySpan<T?> value, bool useHeader = true) where T : unmanaged
    {
        if (useHeader)
            WriteHeader(value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            WriteUnmanagedNullable(in value[i]);
        }
    }
}
