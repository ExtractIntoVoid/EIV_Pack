namespace EIV_Pack.Formatters;

/// <summary>
/// An <see cref="Array"/> formatter.
/// </summary>
/// <typeparam name="T">Any type.</typeparam>
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
