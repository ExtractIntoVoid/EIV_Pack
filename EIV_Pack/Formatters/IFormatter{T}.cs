namespace EIV_Pack.Formatters;

/// <summary>
/// A <typeparamref name="T"/> type formatter to write and read the type.
/// </summary>
/// <typeparam name="T">Any Type.</typeparam>
public interface IFormatter<T> : IFormatter
{
    /// <summary>
    /// Write the <paramref name="value"/> with <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">The writer to write the data into it.</param>
    /// <param name="value">The value to write.</param>
    void Serialize(ref PackWriter writer, scoped ref readonly T? value);

    /// <summary>
    /// Reads te <paramref name="value"/> from the <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The reader to read the data from.</param>
    /// <param name="value">The new vale.</param>
    void Deserialize(ref PackReader reader, scoped ref T? value);
}
