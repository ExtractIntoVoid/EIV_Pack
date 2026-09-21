namespace EIV_Pack.Formatters;

/// <summary>
/// A <see cref="Queue{T}"/> formatter.
/// </summary>
/// <typeparam name="T">Any type.</typeparam>
public class QueueFormatter<T> : ICollectionFormatter<T?, Queue<T?>>
{
    /// <inheritdoc />
    public override Queue<T?> CreateCollection(int length)
    {
        return new(length);
    }

    /// <inheritdoc />
    public override void Clear(scoped ref Queue<T?> collection, int length)
    {
        if (collection == null)
        {
            return;
        }

        collection.Clear();
#if !NETSTANDARD2_0
        collection.EnsureCapacity(length);
#endif
    }

    /// <inheritdoc />
    public override void Add(scoped ref Queue<T?> collection, T? value)
    {
        if (collection == null)
        {
            return;
        }

        collection.Enqueue(value);
    }

    /// <inheritdoc />
    public override T? GetValue(scoped ref readonly Queue<T?> collection, int index)
    {
        if (collection == null)
        {
            return default;
        }

        return collection.Dequeue();
    }
}
