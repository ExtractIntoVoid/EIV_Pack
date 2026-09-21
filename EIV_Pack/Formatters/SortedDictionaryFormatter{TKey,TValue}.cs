namespace EIV_Pack.Formatters;

/// <summary>
/// A <see cref="SortedDictionary{TKey, TValue}"/> formatter.
/// </summary>
/// <typeparam name="TKey">Any type for key that are not null.</typeparam>
/// <typeparam name="TValue">Any type for value.</typeparam>
public class SortedDictionaryFormatter<TKey, TValue>(IComparer<TKey>? comparer) : IDictionaryFormatter<TKey, TValue, SortedDictionary<TKey, TValue?>>
    where TKey : notnull
{
    private readonly IComparer<TKey>? comparer = comparer;

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
