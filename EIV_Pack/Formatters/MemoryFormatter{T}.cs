namespace EIV_Pack.Formatters;

/// <summary>
/// A <see cref="Memory{T}"/> formatter.
/// </summary>
/// <typeparam name="T">Any type.</typeparam>
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
