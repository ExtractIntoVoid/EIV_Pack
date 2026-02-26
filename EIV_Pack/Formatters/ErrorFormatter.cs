namespace EIV_Pack.Formatters;

public class ErrorFormatter<T> : BaseFormatter<T>
{
    public override void Deserialize(ref PackReader reader, scoped ref T? value)
    {
        PackException.ThrowNotRegisteredInProvider(typeof(T));
    }

    public override void Serialize(ref PackWriter writer, scoped ref readonly T? value)
    {
        PackException.ThrowNotRegisteredInProvider(typeof(T));
    }
}
