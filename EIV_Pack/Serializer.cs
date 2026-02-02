using System.Buffers;

namespace EIV_Pack;

public static class Serializer
{
    public static byte[] SerializeArray<T>(in T?[]? value)
    {
        if (value == null)
            return [];

        if (value.Length == 0)
            return Constants.EmptyCollection.ToArray();

        using PackWriter writer = new();
        writer.WriteArray(value);
        byte[] bytes = writer.GetBytes();
        writer.Dispose();
        return bytes;
    }

    public static ArraySegment<byte> SerializeArray_Segment<T>(in T?[]? value)
    {
        if (value == null)
            return [];

        if (value.Length == 0)
            return Constants.EmptyCollection.ToArray();

        using PackWriter writer = new();
        writer.WriteArray(value);
        ArraySegment<byte> bytes = writer.GetAsSegment();
        writer.Dispose();
        return bytes;
    }

    public static ReadOnlySequence<byte> SerializeArray_Sequence<T>(in T?[]? value)
    {
        if (value == null)
            return ReadOnlySequence<byte>.Empty;

        if (value.Length == 0)
            return new(Constants.EmptyCollection.ToArray());

        using PackWriter writer = new();
        writer.WriteArray(value);
        ReadOnlySequence<byte> bytes = writer.GetReadOnlySequence();
        writer.Dispose();
        return bytes;
    }

    public static T?[]? DeserializeArray<T>(in byte[] bytes)
    {
        if (bytes.Length == 0)
            return null;

        if (Constants.EmptyCollection.SequenceEqual(bytes))
            return [];

        PackReader reader = new(bytes);
        return reader.ReadArray<T>();
    }

    public static T?[]? DeserializeArray<T>(in ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 0)
            return null;

        if (Constants.EmptyCollection.SequenceEqual(bytes))
            return [];

        PackReader reader = new(bytes);
        return reader.ReadArray<T>();
    }

    public static T?[]? DeserializeArray<T>(in ReadOnlySequence<byte> bytes)
    {
        if (bytes.Length == 0)
            return null;

        if (Constants.EmptyCollection.SequenceEqual(bytes.FirstSpan))
            return [];

        PackReader reader = new(bytes);
        return reader.ReadArray<T>();
    }

    public static byte[] Serialize<T>(in T? value)
    {
        using PackWriter writer = new();
        writer.WriteValue(value);
        byte[] bytes = writer.GetBytes();
        writer.Dispose();
        return bytes;
    }

    public static ArraySegment<byte> Serialize_Segment<T>(in T? value)
    {
        using PackWriter writer = new();
        writer.WriteValue(value);
        ArraySegment<byte> bytes = writer.GetAsSegment();
        writer.Dispose();
        return bytes;
    }

    public static ReadOnlySequence<byte> Serialize_Sequence<T>(in T? value)
    {
        using PackWriter writer = new();
        writer.WriteValue(value);
        ReadOnlySequence<byte> bytes = writer.GetReadOnlySequence();
        writer.Dispose();
        return bytes;
    }

    public static T? Deserialize<T>(in byte[] bytes)
    {
        T? value = default;
        PackReader reader = new(bytes);
        reader.ReadValue(ref value);
        return value;
    }

    public static T? Deserialize<T>(in ReadOnlySpan<byte> bytes)
    {
        T? value = default;
        PackReader reader = new(bytes);
        reader.ReadValue(ref value);
        return value;
    }

    public static T? Deserialize<T>(in ReadOnlySequence<byte> bytes)
    {
        T? value = default;
        PackReader reader = new(bytes);
        reader.ReadValue(ref value);
        return value;
    }

    public static void Deserialize<T>(in byte[] bytes, scoped ref T? value)
    {
        PackReader reader = new(bytes);
        reader.ReadValue(ref value);
    }

    public static void Deserialize<T>(in ReadOnlySpan<byte> bytes, scoped ref T? value)
    {
        PackReader reader = new(bytes);
        reader.ReadValue(ref value);
    }

    public static void Deserialize<T>(in ReadOnlySequence<byte> bytes, scoped ref T? value)
    {
        PackReader reader = new(bytes);
        reader.ReadValue(ref value);
    }
}
