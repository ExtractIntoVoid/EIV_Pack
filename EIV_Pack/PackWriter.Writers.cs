using EIV_Pack.Formatters;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EIV_Pack;

public ref partial struct PackWriter : IDisposable
{
    public readonly unsafe void WriteUnmanaged<T>(scoped in T value) where T : unmanaged
    {
        int size = sizeof(T);
        Span<byte> span = recyclable.GetSpan(size);
#if NET8_0_OR_GREATER
        MemoryMarshal.Write(span, value);
#else
        T val = value;
        MemoryMarshal.Write(span, ref val);
#endif
        recyclable.Advance(size);
    }

    public readonly void WriteUnmanagedNullable<T>(scoped in T? value) where T : unmanaged
    {
        WriteUnmanaged(value.HasValue);

        if (value.HasValue)
            WriteUnmanaged(value.Value);
    }

    public readonly void WriteHeader(int length = Constants.NullHeader)
    {
        WriteUnmanaged(length);
    }

    public readonly void WriteSmallHeader(byte length = Constants.SmallNullHeader)
    {
        WriteUnmanaged(length);
    }

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
}
