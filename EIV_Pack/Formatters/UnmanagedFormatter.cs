namespace EIV_Pack.Formatters;

public sealed class UnmanagedFormatter<T> : IFormatter<T> where T : unmanaged
{
    public void Deserialize(ref PackReader reader, scoped ref T value)
    {
        value = reader.ReadUnmanaged<T>();
    }

    public void Serialize(ref PackWriter writer, scoped ref readonly T value)
    {
        writer.WriteUnmanaged(value);
    }
}
