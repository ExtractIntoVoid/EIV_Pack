namespace EIV_Pack.Formatters;

public interface IFormatterRegister
{
#if NET8_0_OR_GREATER
    static abstract void RegisterFormatter();
#endif
}

public interface IFormatter
{
    void Serialize(ref PackWriter writer, scoped ref readonly object? value);

    void Deserialize(ref PackReader reader, scoped ref object? value);
}

public interface IFormatter<T> : IFormatter
{
    void Serialize(ref PackWriter writer, scoped ref readonly T? value);

    void Deserialize(ref PackReader reader, scoped ref T? value);
}

public interface IPackable<T> : IFormatterRegister
{
#if NET8_0_OR_GREATER
    static abstract void SerializePackable(ref PackWriter writer, scoped ref readonly T? value);

    static abstract void DeserializePackable(ref PackReader reader, scoped ref T? value);
#endif
}

