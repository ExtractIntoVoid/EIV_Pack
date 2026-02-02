namespace EIV_Pack.Formatters;

public sealed class NullableFormatter<T> : IFormatter<T?> where T : unmanaged
{
    public void Deserialize(ref PackReader reader, scoped ref T? value)
    {
        value = reader.ReadUnmanagedNullable<T>();
    }

    public void Serialize(ref PackWriter writer, scoped ref readonly T? value)
    {
        writer.WriteUnmanagedNullable(value);
    }
}
