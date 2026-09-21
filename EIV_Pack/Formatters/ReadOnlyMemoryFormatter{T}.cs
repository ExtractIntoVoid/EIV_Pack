namespace EIV_Pack.Formatters;

/// <summary>
/// A <see cref="ReadOnlyMemory{T}"/> formatter.
/// </summary>
/// <typeparam name="T">Any type.</typeparam>
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