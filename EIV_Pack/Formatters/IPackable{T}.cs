namespace EIV_Pack.Formatters;

/// <summary>
/// A packable formatter.
/// </summary>
/// <typeparam name="T">Any type.</typeparam>
/// <remarks>
/// Only available for NET8+.
/// </remarks>
#pragma warning disable CA1040
public interface IPackable<T> : IFormatterRegister
{
#if !NETSTANDARD2_0
    /// <summary>
    /// Write the <paramref name="value"/> with <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">The writer to write the data into it.</param>
    /// <param name="value">The value to write.</param>
    static abstract void SerializePackable(ref PackWriter writer, scoped ref readonly T? value);

    /// <summary>
    /// Reads te <paramref name="value"/> from the <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The reader to read the data from.</param>
    /// <param name="value">The new vale.</param>
    static abstract void DeserializePackable(ref PackReader reader, scoped ref T? value);
#endif
}
#pragma warning restore CA1040
