namespace EIV_Pack.Formatters;

public abstract class IDictionaryFormatter<TKey, TValue, TDictionary> : IFormatter<TDictionary>
    where TDictionary : IDictionary<TKey, TValue?>?, new()
    where TKey : notnull
{

    public abstract TDictionary CreateDictionary(int length);

    public void Deserialize(ref PackReader reader, scoped ref TDictionary? value)
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

        var keyformatter = FormatterProvider.GetFormatter<TKey?>();
        var valueformatter = FormatterProvider.GetFormatter<TValue?>();
        for (int i = 0; i < len; i++)
        {
            TKey? key = default;
            TValue? val = default;
            keyformatter.Deserialize(ref reader, ref key);
            valueformatter.Deserialize(ref reader, ref val);
            value.Add(key!, val);
        }
    }

    public void Serialize(ref PackWriter writer, scoped ref readonly TDictionary? dictionary)
    {
        if (dictionary == null)
        {
            writer.WriteHeader();
            return;
        }

        writer.WriteHeader(dictionary.Count);
        var keyformatter = FormatterProvider.GetFormatter<TKey?>();
        var valueformatter = FormatterProvider.GetFormatter<TValue?>();
        foreach (var item in dictionary)
        {
            item.Deconstruct(out var key, out var value);
            keyformatter.Serialize(ref writer, ref key);
            valueformatter.Serialize(ref writer, ref value);
        }
    }
}

public class DictionaryFormatter<TKey, TValue>(IEqualityComparer<TKey>? comparer) : IDictionaryFormatter<TKey, TValue, Dictionary<TKey, TValue?>> where TKey : notnull
{
    readonly IEqualityComparer<TKey>? comparer = comparer;

    public DictionaryFormatter()
        : this(null)
    {

    }

    public override Dictionary<TKey, TValue?> CreateDictionary(int length)
    {
        return new(length, comparer);
    }
}

public class SortedDictionaryFormatter<TKey, TValue>(IComparer<TKey>? comparer) : IDictionaryFormatter<TKey, TValue, SortedDictionary<TKey, TValue?>> where TKey : notnull
{
    readonly IComparer<TKey>? comparer = comparer;

    public SortedDictionaryFormatter()
        : this(null)
    {

    }

    public override SortedDictionary<TKey, TValue?> CreateDictionary(int length)
    {
        return new(comparer);
    }
}

public class SortedListFormatter<TKey, TValue>(IComparer<TKey>? comparer) : IDictionaryFormatter<TKey, TValue, SortedList<TKey, TValue?>> where TKey : notnull
{
    readonly IComparer<TKey>? comparer = comparer;

    public SortedListFormatter()
        : this(null)
    {

    }

    public override SortedList<TKey, TValue?> CreateDictionary(int length)
    {
        return new(length, comparer);
    }
}