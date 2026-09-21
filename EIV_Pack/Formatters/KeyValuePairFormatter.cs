namespace EIV_Pack.Formatters;

/// <summary>
/// A <see cref="KeyValuePair{TKey, TValue}"/> formatter.
/// </summary>
/// <typeparam name="TKey">Any type for key.</typeparam>
/// <typeparam name="TValue">Any type for value.</typeparam>
public sealed class KeyValuePairFormatter<TKey, TValue> : BaseFormatter<KeyValuePair<TKey?, TValue?>>
{
    /// <inheritdoc />
    public override void Deserialize(ref PackReader reader, scoped ref KeyValuePair<TKey?, TValue?> value)
    {
        value = new KeyValuePair<TKey?, TValue?>(
            reader.ReadValue<TKey>(),
            reader.ReadValue<TValue>());
    }

    /// <inheritdoc />
    public override void Serialize(ref PackWriter writer, scoped ref readonly KeyValuePair<TKey?, TValue?> value)
    {
        writer.WriteValue(value.Key);
        writer.WriteValue(value.Value);
    }
}
