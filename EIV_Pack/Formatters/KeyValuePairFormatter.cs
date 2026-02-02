namespace EIV_Pack.Formatters;

public sealed class KeyValuePairFormatter<TKey, TValue> : IFormatter<KeyValuePair<TKey?, TValue?>>
{
    public void Deserialize(ref PackReader reader, scoped ref KeyValuePair<TKey?, TValue?> value)
    {
        value = new KeyValuePair<TKey?, TValue?>(
            reader.ReadValue<TKey>(),
            reader.ReadValue<TValue>()
        );
    }

    public void Serialize(ref PackWriter writer, scoped ref readonly KeyValuePair<TKey?, TValue?> value)
    {
        writer.WriteValue(value.Key);
        writer.WriteValue(value.Value);
    }
}
