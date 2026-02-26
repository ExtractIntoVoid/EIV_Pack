namespace EIV_Pack.Formatters;

public sealed class UnmanagedFormatter<T> : BaseFormatter<T> where T : unmanaged
{
    public override void Deserialize(ref PackReader reader, scoped ref T value)
    {
        value = reader.ReadUnmanaged<T>();
    }

    public override void Serialize(ref PackWriter writer, scoped ref readonly T value)
    {
        writer.WriteUnmanaged(value);
    }
}
