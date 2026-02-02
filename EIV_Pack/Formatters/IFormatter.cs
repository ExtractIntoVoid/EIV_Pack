namespace EIV_Pack.Formatters;

public interface IFormatterRegister
{
    static abstract void RegisterFormatter();
}

public interface IFormatter<T>
{
    abstract void Serialize(ref PackWriter writer, scoped ref readonly T? value);

    abstract void Deserialize(ref PackReader reader, scoped ref T? value);
}

public interface IPackable<T> : IFormatterRegister
{
    static abstract void SerializePackable(ref PackWriter writer, scoped ref readonly T? value);

    static abstract void DeserializePackable(ref PackReader reader, scoped ref T? value);

}