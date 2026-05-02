namespace EIV_Pack.Formatters;

/// <summary>
/// An <see cref="Array"/> formatter.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class ArrayFormatter<T> : BaseFormatter<T?[]>
{
    /// <inheritdoc />
    public override void Serialize(ref PackWriter writer, scoped ref readonly T?[]? value)
    {
        writer.WriteArray(value);
    }

    /// <inheritdoc />
    public override void Deserialize(ref PackReader reader, scoped ref T?[]? value)
    {
        reader.ReadArray(ref value);
    }
}

/// <summary>
/// An <see cref="ArraySegment{T}"/> formatter.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class ArraySegmentFormatter<T> : BaseFormatter<ArraySegment<T?>>
{
    /// <inheritdoc />
    public override void Serialize(ref PackWriter writer, scoped ref readonly ArraySegment<T?> value)
    {
        writer.WriteSpan(value.AsSpan());
    }

    /// <inheritdoc />
    public override void Deserialize(ref PackReader reader, scoped ref ArraySegment<T?> value)
    {
        T?[] array = reader.ReadArray<T>()!;
#if !NETSTANDARD2_0
        value = (ArraySegment<T?>)array;
#else
        value = new ArraySegment<T?>(array);
#endif
    }
}

/// <summary>
/// A <see cref="Memory{T}"/> formatter.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class MemoryFormatter<T> : BaseFormatter<Memory<T?>>
{
    /// <inheritdoc />
    public override void Serialize(ref PackWriter writer, scoped ref readonly Memory<T?> value)
    {
        writer.WriteSpan(value.Span);
    }

    /// <inheritdoc />
    public override void Deserialize(ref PackReader reader, scoped ref Memory<T?> value)
    {
        value = reader.ReadArray<T>();
    }
}

/// <summary>
/// A <see cref="ReadOnlyMemory{T}"/> formatter.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class ReadOnlyMemoryFormatter<T> : BaseFormatter<ReadOnlyMemory<T?>>
{
    /// <inheritdoc />
    public override void Serialize(ref PackWriter writer, scoped ref readonly ReadOnlyMemory<T?> value)
    {
        writer.WriteSpan(value.Span);
    }

    /// <inheritdoc />
    public override void Deserialize(ref PackReader reader, scoped ref ReadOnlyMemory<T?> value)
    {
        value = reader.ReadArray<T>();
    }
}