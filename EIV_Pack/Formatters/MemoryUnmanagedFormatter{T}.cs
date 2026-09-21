namespace EIV_Pack.Formatters;

/// <summary>
/// An <see langword="unmanaged"/> <see cref="Memory{T}"/> formatter.
/// </summary>
/// <typeparam name="T">Any type.</typeparam>
public sealed class MemoryUnmanagedFormatter<T> : BaseFormatter<Memory<T?>>
    where T : unmanaged
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
