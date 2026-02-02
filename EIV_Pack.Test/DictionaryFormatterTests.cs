using System;
using EIV_Pack.Formatters;

namespace EIV_Pack.Test;

public class DictionaryFormatterTests
{
    [Fact]
    public void DictionaryTest()
    {
        PackWriter writer = new();
        var formatter = new DictionaryFormatter<int, int>();
        Assert.NotNull(formatter);
        Dictionary<int, int> kv = new()
        {
            [555] = 3464564,
            [436] = 636366,
        };
        Dictionary<int, int>? nullkv = null;
        Dictionary<int, int> toClearKV = [];
        writer.WriteValueWithFormatter(formatter, kv);
        writer.WriteValueWithFormatter(formatter, nullkv);
        writer.WriteValueWithFormatter(formatter, kv);
        var bytes = writer.GetBytes();
        Assert.NotEmpty(bytes);
        PackReader reader = new(bytes);
        Assert.Equal(kv, reader.ReadValueWithFormatter(formatter));
        Assert.Null(reader.ReadValueWithFormatter(formatter));
        reader.ReadValue(ref toClearKV!, formatter);
        Assert.Equal(kv, toClearKV);
        Assert.Equal(0, reader.Remaining);
        Assert.Null(reader.ReadValueWithFormatter(formatter));
    }

    [Fact]
    public void SortedDictionaryTest()
    {
        PackWriter writer = new();
        var formatter = new SortedDictionaryFormatter<int, int>();
        Assert.NotNull(formatter);
        SortedDictionary<int, int> kv = new()
        {
            [555] = 3464564,
            [436] = 636366,
        };
        SortedDictionary<int, int>? nullkv = null;
        SortedDictionary<int, int> toClearKV = [];
        writer.WriteValueWithFormatter(formatter, kv);
        writer.WriteValueWithFormatter(formatter, nullkv);
        writer.WriteValueWithFormatter(formatter, kv);
        var bytes = writer.GetBytes();
        Assert.NotEmpty(bytes);
        PackReader reader = new(bytes);
        Assert.Equal(kv, reader.ReadValueWithFormatter(formatter));
        Assert.Null(reader.ReadValueWithFormatter(formatter));
        reader.ReadValue(ref toClearKV!, formatter);
        Assert.Equal(kv, toClearKV);
        Assert.Equal(0, reader.Remaining);
        Assert.Null(reader.ReadValueWithFormatter(formatter));
    }

    [Fact]
    public void SortedListTest()
    {
        PackWriter writer = new();
        var formatter = new SortedListFormatter<int, int>();
        Assert.NotNull(formatter);
        SortedList<int, int> kv = new()
        {
            [555] = 3464564,
            [436] = 636366,
        };
        SortedList<int, int>? nullkv = null;
        SortedList<int, int> toClearKV = [];
        writer.WriteValueWithFormatter(formatter, kv);
        writer.WriteValueWithFormatter(formatter, nullkv);
        writer.WriteValueWithFormatter(formatter, kv);
        var bytes = writer.GetBytes();
        Assert.NotEmpty(bytes);
        PackReader reader = new(bytes);
        Assert.Equal(kv, reader.ReadValueWithFormatter(formatter));
        Assert.Null(reader.ReadValueWithFormatter(formatter));
        reader.ReadValue(ref toClearKV!, formatter);
        Assert.Equal(kv, toClearKV);
        Assert.Equal(0, reader.Remaining);
        Assert.Null(reader.ReadValueWithFormatter(formatter));
    }

    [Fact]
    public void KVTest()
    {
        PackWriter writer = new();
        var formatter = new KeyValuePairFormatter<int, int>();
        Assert.NotNull(formatter);
        KeyValuePair<int, int> kv = new(555, 64566);
        KeyValuePair<int, int> toClearKV = new();
        writer.WriteValueWithFormatter(formatter, kv);
        writer.WriteValueWithFormatter(formatter, kv);
        var bytes = writer.GetBytes();
        Assert.NotEmpty(bytes);
        PackReader reader = new(bytes);
        Assert.Equal(kv, reader.ReadValueWithFormatter(formatter));
        reader.ReadValue(ref toClearKV!, formatter);
        Assert.Equal(kv, toClearKV);
        Assert.Equal(0, reader.Remaining);
    }
}
