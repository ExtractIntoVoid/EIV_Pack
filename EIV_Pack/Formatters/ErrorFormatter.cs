namespace EIV_Pack.Formatters;

public class ErrorFormatter<T> : IFormatter<T>
{
    public void Deserialize(ref PackReader reader, scoped ref T? value)
    {
        PackException.ThrowNotRegisteredInProvider(typeof(T));
    }

    public void Serialize(ref PackWriter writer, scoped ref readonly T? value)
    {
        PackException.ThrowNotRegisteredInProvider(typeof(T));
    }
}
