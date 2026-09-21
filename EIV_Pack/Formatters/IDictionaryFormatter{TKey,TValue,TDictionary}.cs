namespace EIV_Pack.Formatters;

/// <summary>
/// A <see cref="IDictionary{TKey, TValue}"/> formatter.
/// </summary>
/// <typeparam name="TKey">Any type for key that are not null.</typeparam>
/// <typeparam name="TValue">Any type for value.</typeparam>
/// <typeparam name="TDictionary">Any <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
public abstract class IDictionaryFormatter<TKey, TValue, TDictionary> : BaseFormatter<TDictionary>
    where TDictionary : IDictionary<TKey, TValue?>?, new()
    where TKey : notnull
{
    /// <summary>
    /// Creates a new dictionary with a length of <paramref name="length"/>.
    /// </summary>
    /// <param name="length">The new length.</param>
    /// <returns>The new dictionary.</returns>
    public abstract TDictionary CreateDictionary(int length);

    /// <inheritdoc />
    public override void Deserialize(ref PackReader reader, scoped ref TDictionary? value)
    {
        if (!reader.TryReadHeader(out int len) || len == Constants.NullHeader)
        {
            value = default;
            return;
        }

        if (value == null)
        {
            value = CreateDictionary(len)!;
        }
        else
        {
            value.Clear();
        }

        IFormatter<TKey?> keyformatter = FormatterProvider.GetFormatter<TKey?>();
        IFormatter<TValue?> valueformatter = FormatterProvider.GetFormatter<TValue?>();
        for (int i = 0; i < len; i++)
        {
            TKey? key = default;
            TValue? val = default;
            keyformatter.Deserialize(ref reader, ref key);
            valueformatter.Deserialize(ref reader, ref val);
            value.Add(key!, val);
        }
    }

    /// <inheritdoc />
    public override void Serialize(ref PackWriter writer, scoped ref readonly TDictionary? dictionary)
    {
        if (dictionary == null)
        {
            writer.WriteHeader();
            return;
        }

        writer.WriteHeader(dictionary.Count);
        IFormatter<TKey?> keyformatter = FormatterProvider.GetFormatter<TKey?>();
        IFormatter<TValue?> valueformatter = FormatterProvider.GetFormatter<TValue?>();
        foreach (var item in dictionary)
        {
#if !NETSTANDARD2_0
            item.Deconstruct(out var key, out var value);
#else
            var key = item.Key;
            var value = item.Value;
#endif

            keyformatter.Serialize(ref writer, ref key);
            valueformatter.Serialize(ref writer, ref value);
        }
    }
}
