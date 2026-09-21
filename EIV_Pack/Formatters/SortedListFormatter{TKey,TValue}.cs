namespace EIV_Pack.Formatters;

/// <summary>
/// A <see cref="SortedList{TKey, TValue}"/> formatter.
/// </summary>
/// <typeparam name="TKey">Any type for key that are not null.</typeparam>
/// <typeparam name="TValue">Any type for value.</typeparam>
public class SortedListFormatter<TKey, TValue>(IComparer<TKey>? comparer) : IDictionaryFormatter<TKey, TValue, SortedList<TKey, TValue?>>
    where TKey : notnull
{
    private readonly IComparer<TKey>? comparer = comparer;

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