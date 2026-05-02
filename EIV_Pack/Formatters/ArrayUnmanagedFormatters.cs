namespace EIV_Pack.Formatters;

/// <summary>
/// An <see langword="unmanaged"/> <see cref="Array"/> formatter.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class ArrayUnmanagedFormatter<T> : BaseFormatter<T?[]> where T : unmanaged
{
    /// <inheritdoc />
    public override void Serialize(ref PackWriter writer, scoped ref readonly T?[]? value)
    {
        writer.WriteArrayUnmanaged(value);
    }

    /// <inheritdoc />
    public override void Deserialize(ref PackReader reader, scoped ref T?[]? value)
    {
        reader.ReadArray(ref value);
    }
}

/// <summary>
/// An <see langword="unmanaged"/> <see cref="ArraySegment{T}"/> formatter.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class ArraySegmentUnmanagedFormatter<T> : BaseFormatter<ArraySegment<T?>> where T : unmanaged
{
    /// <inheritdoc />
    public override void Serialize(ref PackWriter writer, scoped ref readonly ArraySegment<T?> value)
    {
        writer.WriteSpanUnmanaged(value.AsSpan());
    }

    /// <inheritdoc />
    public override void Deserialize(ref PackReader reader, scoped ref ArraySegment<T?> value)
    {
        T?[] array = reader.ReadArrayUnmanaged<T>()!;
#if !NETSTANDARD2_0
        value = (ArraySegment<T?>)array;
#else
        value = new ArraySegment<T?>(array);
#endif
    }
}

/// <summary>
/// An <see langword="unmanaged"/> <see cref="Memory{T}"/> formatter.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class MemoryUnmanagedFormatter<T> : BaseFormatter<Memory<T?>> where T : unmanaged
{
    /// <inheritdoc />
    public override void Serialize(ref PackWriter writer, scoped ref readonly Memory<T?> value)
    {
        writer.WriteSpanUnmanaged(value.Span);
    }

    /// <inheritdoc />
    public override void Deserialize(ref PackReader reader, scoped ref Memory<T?> value)
    {
        value = reader.ReadArrayUnmanaged<T>();
    }
}

/// <summary>
/// An <see langword="unmanaged"/> <see cref="ReadOnlyMemory{T}"/> formatter.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class ReadOnlyMemoryUnmanagedFormatter<T> : BaseFormatter<ReadOnlyMemory<T?>> where T : unmanaged
{
    /// <inheritdoc />
    public override void Serialize(ref PackWriter writer, scoped ref readonly ReadOnlyMemory<T?> value)
    {
        writer.WriteSpanUnmanaged(value.Span);
    }

    /// <inheritdoc />
    public override void Deserialize(ref PackReader reader, scoped ref ReadOnlyMemory<T?> value)
    {
        value = reader.ReadArrayUnmanaged<T>();
    }
}