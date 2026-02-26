namespace EIV_Pack.Formatters;

public abstract class BaseFormatter<T> : IFormatter<T>, IFormatter
{
    public abstract void Deserialize(ref PackReader reader, scoped ref T? value);

    public void Deserialize(ref PackReader reader, scoped ref object? value)
    {
        T? v = (value == null) ? default : (T?)value;
        Deserialize(ref reader, ref v);
        value = v;
    }

    public abstract void Serialize(ref PackWriter writer, scoped ref readonly T? value);

    public void Serialize(ref PackWriter writer, scoped ref readonly object? value)
    {
        T? v = (value == null) ? default : (T?)value;
        Serialize(ref writer, ref v);
    }
}
