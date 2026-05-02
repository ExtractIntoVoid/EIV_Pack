namespace EIV_Pack.Formatters;

/// <summary>
/// A base formatter for all new types.
/// </summary>
/// <typeparam name="T">Any type.</typeparam>
public abstract class BaseFormatter<T> : IFormatter<T>, IFormatter
{
    /// <inheritdoc />
    public abstract void Deserialize(ref PackReader reader, scoped ref T? value);

    /// <inheritdoc />
    public void Deserialize(ref PackReader reader, scoped ref object? value)
    {
        T? v = (value == null) ? default : (T?)value;
        Deserialize(ref reader, ref v);
        value = v;
    }

    /// <inheritdoc />
    public abstract void Serialize(ref PackWriter writer, scoped ref readonly T? value);

    /// <inheritdoc />
    public void Serialize(ref PackWriter writer, scoped ref readonly object? value)
    {
        T? v = (value == null) ? default : (T?)value;
        Serialize(ref writer, ref v);
    }
}
