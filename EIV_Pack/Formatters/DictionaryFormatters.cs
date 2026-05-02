namespace EIV_Pack.Formatters;

/// <summary>
/// A <see cref="IDictionary{TKey, TValue}"/> formatter.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TDictionary"></typeparam>
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
            value = CreateDictionary(len)!;
        else
            value.Clear();

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

/// <summary>
/// A <see cref="Dictionary{TKey, TValue}"/> formatter.
/// </summary>
public class DictionaryFormatter<TKey, TValue>(IEqualityComparer<TKey>? comparer) : IDictionaryFormatter<TKey, TValue, Dictionary<TKey, TValue?>> where TKey : notnull
{
    readonly IEqualityComparer<TKey>? comparer = comparer;

    /// <inheritdoc />
    public DictionaryFormatter()
        : this(null)
    {

    }

    /// <inheritdoc />
    public override Dictionary<TKey, TValue?> CreateDictionary(int length)
    {
        return new(length, comparer);
    }
}

/// <summary>
/// A <see cref="SortedDictionary{TKey, TValue}"/> formatter.
/// </summary>
public class SortedDictionaryFormatter<TKey, TValue>(IComparer<TKey>? comparer) : IDictionaryFormatter<TKey, TValue, SortedDictionary<TKey, TValue?>> where TKey : notnull
{
    readonly IComparer<TKey>? comparer = comparer;

    /// <inheritdoc />
    public SortedDictionaryFormatter()
        : this(null)
    {

    }

    /// <inheritdoc />
    public override SortedDictionary<TKey, TValue?> CreateDictionary(int length)
    {
        return new(comparer);
    }
}

/// <summary>
/// A <see cref="SortedList{TKey, TValue}"/> formatter.
/// </summary>
public class SortedListFormatter<TKey, TValue>(IComparer<TKey>? comparer) : IDictionaryFormatter<TKey, TValue, SortedList<TKey, TValue?>> where TKey : notnull
{
    readonly IComparer<TKey>? comparer = comparer;

    /// <inheritdoc />
    public SortedListFormatter()
        : this(null)
    {

    }

    /// <inheritdoc />
    public override SortedList<TKey, TValue?> CreateDictionary(int length)
    {
        return new(length, comparer);
    }
}