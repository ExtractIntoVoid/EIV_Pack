namespace EIV_Pack.Formatters;

public sealed class KeyValuePairFormatter<TKey, TValue> : BaseFormatter<KeyValuePair<TKey?, TValue?>>
{
    public override void Deserialize(ref PackReader reader, scoped ref KeyValuePair<TKey?, TValue?> value)
    {
        value = new KeyValuePair<TKey?, TValue?>(
            reader.ReadValue<TKey>(),
            reader.ReadValue<TValue>()
        );
    }

    public override void Serialize(ref PackWriter writer, scoped ref readonly KeyValuePair<TKey?, TValue?> value)
    {
        writer.WriteValue(value.Key);
        writer.WriteValue(value.Value);
    }
}
