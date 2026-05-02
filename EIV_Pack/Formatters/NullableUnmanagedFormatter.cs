namespace EIV_Pack.Formatters;

/// <summary>
/// A <see cref="Nullable{T}"/> <see langword="unmanaged"/> formatter.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class NullableUnmanagedFormatter<T> : BaseFormatter<T?> where T : unmanaged
{
    /// <inheritdoc />
    public override void Deserialize(ref PackReader reader, scoped ref T? value)
    {
        value = reader.ReadUnmanagedNullable<T>();
    }

    /// <inheritdoc />
    public override void Serialize(ref PackWriter writer, scoped ref readonly T? value)
    {
        writer.WriteUnmanagedNullable(value);
    }
}
