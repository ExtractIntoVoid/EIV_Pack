namespace EIV_Pack.Formatters;

/// <summary>
/// A <see cref="SortedSet{T}"/> formatter.
/// </summary>
/// <typeparam name="T">Any type.</typeparam>
public class SortedSetFormatter<T>(IComparer<T?>? equalityComparer) : ICollectionTFormatter<T?, SortedSet<T?>>
{
    private readonly IComparer<T?>? equalityComparer = equalityComparer;

    /// <inheritdoc />
    public SortedSetFormatter()
        : this(null)
    {
    }

    /// <inheritdoc />
    public override SortedSet<T?> CreateCollection(int length)
    {
        return new(equalityComparer);
    }
}
