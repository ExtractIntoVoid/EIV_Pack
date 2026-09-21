namespace EIV_Pack.Formatters;

/// <summary>
/// A <see cref="Dictionary{TKey, TValue}"/> formatter.
/// </summary>
/// <typeparam name="TKey">Any type for key that are not null.</typeparam>
/// <typeparam name="TValue">Any type for value.</typeparam>
public class DictionaryFormatter<TKey, TValue>(IEqualityComparer<TKey>? comparer) : IDictionaryFormatter<TKey, TValue, Dictionary<TKey, TValue?>>
    where TKey : notnull
{
    private readonly IEqualityComparer<TKey>? comparer = comparer;

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
