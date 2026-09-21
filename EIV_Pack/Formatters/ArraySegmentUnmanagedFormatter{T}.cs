namespace EIV_Pack.Formatters;

/// <summary>
/// An <see langword="unmanaged"/> <see cref="ArraySegment{T}"/> formatter.
/// </summary>
/// <typeparam name="T">Any type.</typeparam>
public sealed class ArraySegmentUnmanagedFormatter<T> : BaseFormatter<ArraySegment<T?>>
    where T : unmanaged
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
