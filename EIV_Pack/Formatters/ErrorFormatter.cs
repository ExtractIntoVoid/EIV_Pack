namespace EIV_Pack.Formatters;

/// <summary>
/// A formatter ot throw error when something is not registered.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ErrorFormatter<T> : BaseFormatter<T>
{
    /// <inheritdoc />
    public override void Deserialize(ref PackReader reader, scoped ref T? value)
    {
        throw new PackException($"{typeof(T).FullName} is not registered in this provider.");
    }

    /// <inheritdoc />
    public override void Serialize(ref PackWriter writer, scoped ref readonly T? value)
    {
        throw new PackException($"{typeof(T).FullName} is not registered in this provider.");
    }
}
