namespace EIV_Pack.Formatters;

/// <summary>
/// A <see cref="HashSet{T}"/> formatter.
/// </summary>
/// <typeparam name="T">Any type.</typeparam>
public class HashSetFormatter<T>(IEqualityComparer<T?>? equalityComparer) : ICollectionTFormatter<T?, HashSet<T?>>
{
    private readonly IEqualityComparer<T?>? equalityComparer = equalityComparer;

    /// <inheritdoc />
    public HashSetFormatter()
        : this(null)
    {
    }

    /// <inheritdoc />
    public override HashSet<T?> CreateCollection(int length)
    {
#if !NETSTANDARD2_0
        return new(length, equalityComparer);
#else
        return new(equalityComparer);
#endif
    }
}
