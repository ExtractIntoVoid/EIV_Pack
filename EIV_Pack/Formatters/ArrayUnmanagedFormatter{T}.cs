namespace EIV_Pack.Formatters;

/// <summary>
/// An <see langword="unmanaged"/> <see cref="Array"/> formatter.
/// </summary>
/// <typeparam name="T">Any type.</typeparam>
public sealed class ArrayUnmanagedFormatter<T> : BaseFormatter<T?[]>
    where T : unmanaged
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
