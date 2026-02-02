using System.Collections;
using System.Collections.ObjectModel;

namespace EIV_Pack.Formatters;

public abstract class ICollectionFormatter<T, TCollection> : IFormatter<TCollection> where TCollection : ICollection?, new()
{
    public abstract TCollection CreateCollection(int length);

    public abstract void Clear(scoped ref TCollection collection, int length);

    public abstract void Add(scoped ref TCollection collection, T? value);

    public abstract T? GetValue(scoped ref readonly TCollection collection, int index);

    public void Deserialize(ref PackReader reader, scoped ref TCollection? value)
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

        var formatter = FormatterProvider.GetFormatter<T?>();
        for (int i = 0; i < len; i++)
        {
            T? val = default;
            formatter.Deserialize(ref reader, ref val);
            Add(ref value, val);
        }
    }

    public void Serialize(ref PackWriter writer, scoped ref readonly TCollection? collection)
    {
        if (collection == null)
        {
            writer.WriteHeader();
            return;
        }

        writer.WriteHeader(collection.Count);
        var formatter = FormatterProvider.GetFormatter<T?>();
        for (int i = 0; i < collection.Count; i++)
        {
            T? val = GetValue(in collection, i);
            formatter.Serialize(ref writer, ref val);
        }
    }
}

public abstract class ICollectionTFormatter<T, TCollection> : IFormatter<TCollection> where TCollection : ICollection<T?>?, new()
{
    public virtual TCollection CreateCollection(int length)
    {
        return new();
    }

    public void Deserialize(ref PackReader reader, scoped ref TCollection? value)
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

        var formatter = FormatterProvider.GetFormatter<T?>();
        for (int i = 0; i < len; i++)
        {
            T? val = default;
            formatter.Deserialize(ref reader, ref val);
            value.Add(val);
        }
    }

    public void Serialize(ref PackWriter writer, scoped ref readonly TCollection? value)
    {
        if (value == null)
        {
            writer.WriteHeader();
            return;
        }

        writer.WriteHeader(value.Count);
        var formatter = FormatterProvider.GetFormatter<T?>();
        foreach (var item in value)
        {
            var val = item;
            formatter.Serialize(ref writer, ref val);
        }
    }
}

public class CollectionFormatter<T> : ICollectionTFormatter<T?, Collection<T?>>;

public class ObservableCollectionFormatter<T> : ICollectionTFormatter<T?, ObservableCollection<T?>>;

public class ListFormatter<T> : ICollectionTFormatter<T?, List<T?>>;

public class LinkedListFormatter<T> : ICollectionTFormatter<T?, LinkedList<T?>>;

public class HashSetFormatter<T>(IEqualityComparer<T?>? equalityComparer) : ICollectionTFormatter<T?, HashSet<T?>>
{
    readonly IEqualityComparer<T?>? equalityComparer = equalityComparer;

    public HashSetFormatter()
        : this(null)
    {
    }

    public override HashSet<T?> CreateCollection(int length)
    {
        return new(length, equalityComparer);
    }
}


public class SortedSetFormatter<T>(IComparer<T?>? equalityComparer) : ICollectionTFormatter<T?, SortedSet<T?>>
{
    readonly IComparer<T?>? equalityComparer = equalityComparer;

    public SortedSetFormatter()
        : this(null)
    {
    }

    public override SortedSet<T?> CreateCollection(int length)
    {
        return new(equalityComparer);
    }
}
public class QueueFormatter<T> : ICollectionFormatter<T?, Queue<T?>>
{
    public override Queue<T?> CreateCollection(int length)
    {
        return new(length);
    }

    public override void Clear(scoped ref Queue<T?> collection, int length)
    {
        collection.Clear();
        collection.EnsureCapacity(length);
    }

    public override void Add(scoped ref Queue<T?> collection, T? value)
    {
        collection.Enqueue(value);
    }

    public override T? GetValue(scoped ref readonly Queue<T?> collection, int index)
    {
        return collection.Dequeue();
    }
}

public class StackFormatter<T> : ICollectionFormatter<T?, Stack<T?>>
{
    public override Stack<T?> CreateCollection(int length)
    {
        return new(length);
    }

    public override void Add(scoped ref Stack<T?> collection, T? value)
    {
        collection.Push(value);
    }

    public override void Clear(scoped ref Stack<T?> collection, int length)
    {
        collection.Clear();
        collection.EnsureCapacity(length);
    }

    public override T? GetValue(scoped ref readonly Stack<T?> collection, int index)
    {
        return collection.Reverse().ElementAt(index);
    }
}
