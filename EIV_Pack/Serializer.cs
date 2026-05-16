using System.Buffers;

namespace EIV_Pack;

/// <summary>
/// Static helpers for serializing and deserializing values and arrays to and from binary representations.
/// </summary>
public static class Serializer
{
    /// <summary>
    /// Serialize <typeparamref name="T"/> array.
    /// </summary>
    /// <typeparam name="T">Any registered type.</typeparam>
    /// <param name="value">The values to serialize.</param>
    /// <returns>The serialized data array.</returns>
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

    /// <summary>
    /// Serialize <typeparamref name="T"/> array.
    /// </summary>
    /// <typeparam name="T">Any registered type.</typeparam>
    /// <param name="value">The values to serialize.</param>
    /// <returns>The serialized data segment.</returns>
    public static ArraySegment<byte> SerializeArray_Segment<T>(in T?[]? value)
    {
        if (value == null)
            return [];

        if (value.Length == 0)
        {
#if !NETSTANDARD2_0
            return Constants.EmptyCollection.ToArray();
#else
            return new ArraySegment<byte>(Constants.EmptyCollection.ToArray());
#endif
        }

        using PackWriter writer = new();
        writer.WriteArray(value);
        ArraySegment<byte> bytes = writer.GetAsSegment();
        writer.Dispose();
        return bytes;
    }

    /// <summary>
    /// Serialize <typeparamref name="T"/> array.
    /// </summary>
    /// <typeparam name="T">Any registered type.</typeparam>
    /// <param name="value">The values to serialize.</param>
    /// <returns>The serialized data sequence.</returns>
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

    /// <summary>
    /// Deserialize <typeparamref name="T"/> array.
    /// </summary>
    /// <typeparam name="T">Any registered type.</typeparam>
    /// <param name="bytes">The input serialized data.</param>
    /// <returns>The deserialized values.</returns>
    public static T?[]? DeserializeArray<T>(in byte[] bytes)
    {
        if (bytes.Length == 0)
            return null;

        if (Constants.EmptyCollection.SequenceEqual(bytes))
            return [];

        PackReader reader = new(bytes);
        return reader.ReadArray<T>();
    }

    /// <summary>
    /// Deserialize <typeparamref name="T"/> array.
    /// </summary>
    /// <typeparam name="T">Any registered type.</typeparam>
    /// <param name="bytes">The input serialized data.</param>
    /// <returns>The deserialized values.</returns>
    public static T?[]? DeserializeArray<T>(in ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 0)
            return null;

        if (Constants.EmptyCollection.SequenceEqual(bytes))
            return [];

        PackReader reader = new(bytes);
        return reader.ReadArray<T>();
    }

    /// <summary>
    /// Deserialize <typeparamref name="T"/> array.
    /// </summary>
    /// <typeparam name="T">Any registered type.</typeparam>
    /// <param name="bytes">The input serialized data.</param>
    /// <returns>The deserialized values.</returns>
    public static T?[]? DeserializeArray<T>(in ReadOnlySequence<byte> bytes)
    {
        if (bytes.Length == 0)
            return null;

        if (Constants.EmptyCollection.SequenceEqual(bytes.First.Span))
            return [];

        PackReader reader = new(bytes);
        return reader.ReadArray<T>();
    }

    /// <summary>
    /// Serialize <typeparamref name="T"/> value.
    /// </summary>
    /// <typeparam name="T">Any registered type.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The serialized data.</returns>
    public static byte[] Serialize<T>(in T? value)
    {
        using PackWriter writer = new();
        writer.WriteValue(value);
        byte[] bytes = writer.GetBytes();
        writer.Dispose();
        return bytes;
    }

    /// <summary>
    /// Serialize <typeparamref name="T"/> value.
    /// </summary>
    /// <typeparam name="T">Any registered type.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The serialized data segment.</returns>
    public static ArraySegment<byte> Serialize_Segment<T>(in T? value)
    {
        using PackWriter writer = new();
        writer.WriteValue(value);
        ArraySegment<byte> bytes = writer.GetAsSegment();
        writer.Dispose();
        return bytes;
    }

    /// <summary>
    /// Serialize <typeparamref name="T"/> value.
    /// </summary>
    /// <typeparam name="T">Any registered type.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The serialized data sequence.</returns>
    public static ReadOnlySequence<byte> Serialize_Sequence<T>(in T? value)
    {
        using PackWriter writer = new();
        writer.WriteValue(value);
        ReadOnlySequence<byte> bytes = writer.GetReadOnlySequence();
        writer.Dispose();
        return bytes;
    }

    /// <summary>
    /// Deserialize <typeparamref name="T"/> value.
    /// </summary>
    /// <typeparam name="T">Any registered type.</typeparam>
    /// <param name="bytes">The input serialized data.</param>
    /// <returns>The deserialized value.</returns>
    public static T? Deserialize<T>(in byte[] bytes)
    {
        T? value = default;
        PackReader reader = new(bytes);
        reader.ReadValue(ref value);
        return value;
    }

    /// <summary>
    /// Deserialize <typeparamref name="T"/> value.
    /// </summary>
    /// <typeparam name="T">Any registered type.</typeparam>
    /// <param name="bytes">The input serialized data.</param>
    /// <returns>The deserialized value.</returns>
    public static T? Deserialize<T>(in ReadOnlySpan<byte> bytes)
    {
        T? value = default;
        PackReader reader = new(bytes);
        reader.ReadValue(ref value);
        return value;
    }

    /// <summary>
    /// Deserialize <typeparamref name="T"/> value.
    /// </summary>
    /// <typeparam name="T">Any registered type.</typeparam>
    /// <param name="bytes">The input serialized data.</param>
    /// <returns>The deserialized value.</returns>
    public static T? Deserialize<T>(in ReadOnlySequence<byte> bytes)
    {
        T? value = default;
        PackReader reader = new(bytes);
        reader.ReadValue(ref value);
        return value;
    }

    /// <summary>
    /// Deserialize <typeparamref name="T"/> value.
    /// </summary>
    /// <typeparam name="T">Any registered type.</typeparam>
    /// <param name="bytes">The input serialized data.</param>
    /// <param name="value">The reference to deserialize the value to.</param>
    public static void Deserialize<T>(in byte[] bytes, scoped ref T? value)
    {
        PackReader reader = new(bytes);
        reader.ReadValue(ref value);
    }

    /// <summary>
    /// Deserialize <typeparamref name="T"/> value.
    /// </summary>
    /// <typeparam name="T">Any registered type.</typeparam>
    /// <param name="bytes">The input serialized data.</param>
    /// <param name="value">The reference to deserialize the value to.</param>
    public static void Deserialize<T>(in ReadOnlySpan<byte> bytes, scoped ref T? value)
    {
        PackReader reader = new(bytes);
        reader.ReadValue(ref value);
    }

    /// <summary>
    /// Deserialize <typeparamref name="T"/> value.
    /// </summary>
    /// <typeparam name="T">Any registered type.</typeparam>
    /// <param name="bytes">The input serialized data.</param>
    /// <param name="value">The reference to deserialize the value to.</param>
    public static void Deserialize<T>(in ReadOnlySequence<byte> bytes, scoped ref T? value)
    {
        PackReader reader = new(bytes);
        reader.ReadValue(ref value);
    }
}
