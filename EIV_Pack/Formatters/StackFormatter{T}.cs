namespace EIV_Pack.Formatters;

/// <summary>
/// A <see cref="Stack{T}"/> formatter.
/// </summary>
/// <typeparam name="T">Any type.</typeparam>
public class StackFormatter<T> : ICollectionFormatter<T?, Stack<T?>>
{
    /// <inheritdoc />
    public override Stack<T?> CreateCollection(int length)
    {
        return new(length);
    }

    /// <inheritdoc />
    public override void Add(scoped ref Stack<T?> collection, T? value)
    {
        if (collection == null)
        {
            return;
        }

        collection.Push(value);
    }

    /// <inheritdoc />
    public override void Clear(scoped ref Stack<T?> collection, int length)
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
    public override T? GetValue(scoped ref readonly Stack<T?> collection, int index)
    {
        return collection.Reverse().ElementAt(index);
    }
}
