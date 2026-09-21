namespace EIV_Pack.Formatters;

/// <summary>
/// An <see langword="unmanaged"/> <see cref="ReadOnlyMemory{T}"/> formatter.
/// </summary>
/// <typeparam name="T">Any type.</typeparam>
public sealed class ReadOnlyMemoryUnmanagedFormatter<T> : BaseFormatter<ReadOnlyMemory<T?>>
    where T : unmanaged
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