using System.Collections;
using System.Collections.ObjectModel;

namespace EIV_Pack.Formatters;

/// <summary>
/// A formatter for <see cref="ICollection"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TCollection"></typeparam>
public abstract class ICollectionFormatter<T, TCollection> : BaseFormatter<TCollection> where TCollection : ICollection?, new()
{
    /// <summary>
    /// Creates a new collection with a lenght of <paramref name="length"/>.
    /// </summary>
    /// <param name="length">The new length.</param>
    /// <returns>The new collection.</returns>
    public abstract TCollection CreateCollection(int length);

    /// <summary>
    /// Clears the <paramref name="collection"/> and check if <paramref name="length"/> item can be added.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="length">The length of to ensure items can fit.</param>
    public abstract void Clear(scoped ref TCollection collection, int length);

    /// <summary>
    /// Add <paramref name="value"/> to the <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="value">The item to add.</param>
    public abstract void Add(scoped ref TCollection collection, T? value);

    /// <summary>
    /// Gets the item from the <paramref name="index"/> position from the <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="index">The index to get the item.</param>
    /// <returns>The got item or <see langword="null"/>.</returns>
    public abstract T? GetValue(scoped ref readonly TCollection collection, int index);

    /// <inheritdoc />
    public override void Deserialize(ref PackReader reader, scoped ref TCollection? value)
    {
        if (!reader.TryReadHeader(out int len) || len == Constants.NullHeader)
        {
            value = default;
            return;
        }

        if (value == null)
            value = CreateCollection(len)!;
        else
            Clear(ref value, len);

        IFormatter<T?> formatter = FormatterProvider.GetFormatter<T?>();
        for (int i = 0; i < len; i++)
        {
            T? val = default;
            formatter.Deserialize(ref reader, ref val);
            Add(ref value, val);
        }
    }

    /// <inheritdoc />
    public override void Serialize(ref PackWriter writer, scoped ref readonly TCollection? collection)
    {
        if (collection == null)
        {
            writer.WriteHeader();
            return;
        }

        writer.WriteHeader(collection.Count);
        IFormatter<T?> formatter = FormatterProvider.GetFormatter<T?>();
        for (int i = 0; i < collection.Count; i++)
        {
            T? val = GetValue(in collection, i);
            formatter.Serialize(ref writer, ref val);
        }
    }
}

/// <summary>
/// A formatter for <see cref="ICollection{T}"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TCollection"></typeparam>
public abstract class ICollectionTFormatter<T, TCollection> : BaseFormatter<TCollection> where TCollection : ICollection<T?>?, new()
{
    /// <inheritdoc />
    public virtual TCollection CreateCollection(int length)
    {
        return new();
    }

    /// <inheritdoc />
    public override void Deserialize(ref PackReader reader, scoped ref TCollection? value)
    {
        if (!reader.TryReadHeader(out int len) || len == Constants.NullHeader)
        {
            value = default;
            return;
        }

        if (value == null)
            value = CreateCollection(len)!;
        else
            value.Clear();

        IFormatter<T?> formatter = FormatterProvider.GetFormatter<T?>();
        for (int i = 0; i < len; i++)
        {
            T? val = default;
            formatter.Deserialize(ref reader, ref val);
            value.Add(val);
        }
    }

    /// <inheritdoc />
    public override void Serialize(ref PackWriter writer, scoped ref readonly TCollection? value)
    {
        if (value == null)
        {
            writer.WriteHeader();
            return;
        }

        writer.WriteHeader(value.Count);
        IFormatter<T?> formatter = FormatterProvider.GetFormatter<T?>();
        foreach (var item in value)
        {
            var val = item;
            formatter.Serialize(ref writer, ref val);
        }
    }
}

/// <summary>
/// A <see cref="Collection{T}"/> formatter.
/// </summary>
/// <typeparam name="T"></typeparam>
public class CollectionFormatter<T> : ICollectionTFormatter<T?, Collection<T?>>;

/// <summary>
/// A <see cref="ObservableCollection{T}"/> formatter.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObservableCollectionFormatter<T> : ICollectionTFormatter<T?, ObservableCollection<T?>>;

/// <summary>
/// A <see cref="List{T}"/> formatter.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ListFormatter<T> : ICollectionTFormatter<T?, List<T?>>;

/// <summary>
/// A <see cref="LinkedList{T}"/> formatter.
/// </summary>
/// <typeparam name="T"></typeparam>
public class LinkedListFormatter<T> : ICollectionTFormatter<T?, LinkedList<T?>>;

/// <summary>
/// A <see cref="HashSet{T}"/> formatter.
/// </summary>
/// <typeparam name="T"></typeparam>
public class HashSetFormatter<T>(IEqualityComparer<T?>? equalityComparer) : ICollectionTFormatter<T?, HashSet<T?>>
{
    readonly IEqualityComparer<T?>? equalityComparer = equalityComparer;

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

/// <summary>
/// A <see cref="SortedSet{T}"/> formatter.
/// </summary>
/// <typeparam name="T"></typeparam>
public class SortedSetFormatter<T>(IComparer<T?>? equalityComparer) : ICollectionTFormatter<T?, SortedSet<T?>>
{
    readonly IComparer<T?>? equalityComparer = equalityComparer;

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

/// <summary>
/// A <see cref="Queue{T}"/> formatter.
/// </summary>
/// <typeparam name="T"></typeparam>
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
        collection.Clear();
#if !NETSTANDARD2_0
        collection.EnsureCapacity(length);
#endif
    }

    /// <inheritdoc />
    public override void Add(scoped ref Queue<T?> collection, T? value)
    {
        collection.Enqueue(value);
    }

    /// <inheritdoc />
    public override T? GetValue(scoped ref readonly Queue<T?> collection, int index)
    {
        return collection.Dequeue();
    }
}

/// <summary>
/// A <see cref="Stack{T}"/> formatter.
/// </summary>
/// <typeparam name="T"></typeparam>
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
        collection.Push(value);
    }

    /// <inheritdoc />
    public override void Clear(scoped ref Stack<T?> collection, int length)
    {
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
