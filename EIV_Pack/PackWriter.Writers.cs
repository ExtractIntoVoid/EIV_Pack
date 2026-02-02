using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EIV_Pack;

public ref partial struct PackWriter : IDisposable
{
    public readonly unsafe void WriteUnmanaged<T>(scoped in T value) where T : unmanaged
    {
        int size = sizeof(T);
        Span<byte> span = recyclable.GetSpan(size);
        MemoryMarshal.Write(span, value);
        recyclable.Advance(size);
    }

    public readonly unsafe void WriteUnmanagedNullable<T>(scoped in T? value) where T : unmanaged
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
        var data = TextEncoding.GetBytes(value);
        recyclable.Write(data);
    }

    public void WriteArray<T>(T?[]? array)
    {
        if (array == null)
        {
            WriteHeader();
            return;
        }


        var formatter = FormatterProvider.GetFormatter<T>();

        WriteHeader(array.Length);
        for (int i = 0; i < array.Length; i++)
        {
            formatter.Serialize(ref this, ref array[i]);
        }
    }

    public void WriteSpan<T>(scoped Span<T?> value, bool useHeader = true)
    {
        var formatter = FormatterProvider.GetFormatter<T>();

        if (useHeader)
            WriteHeader(value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            formatter.Serialize(ref this, ref value[i]);
        }
    }

    public void WriteSpan<T>(scoped ReadOnlySpan<T?> value, bool useHeader = true)
    {
        var formatter = FormatterProvider.GetFormatter<T>();

        if (useHeader)
            WriteHeader(value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            formatter.Serialize(ref this, ref Unsafe.AsRef(in value[i]));
        }
    }
}
