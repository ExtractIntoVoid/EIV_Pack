namespace EIV_Pack.Formatters;

/// <summary>
/// An <see langword="unmanaged"/> formatter.
/// </summary>
/// <typeparam name="T">Any <see langword="unmanaged"/> type.</typeparam>
public sealed class UnmanagedFormatter<T> : BaseFormatter<T>
    where T : unmanaged
{
    /// <inheritdoc />
    public override void Deserialize(ref PackReader reader, scoped ref T value)
    {
        value = reader.ReadUnmanaged<T>();
    }

    /// <inheritdoc />
    public override void Serialize(ref PackWriter writer, scoped ref readonly T value)
    {
        writer.WriteUnmanaged(value);
    }
}
