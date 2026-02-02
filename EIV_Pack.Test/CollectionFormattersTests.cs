using EIV_Pack.Formatters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EIV_Pack.Test;

public class CollectionFormattersTests
{
    [Fact]
    public void CollectionTest()
    {
        PackWriter writer = new();
        var formatter = new CollectionFormatter<int>();
        Assert.NotNull(formatter);
        Collection<int>? ints = [555, 555, 556, 457476];
        Collection<int>? valNull = null;
        Collection<int>? col = new();
        writer.WriteValueWithFormatter(formatter, ints);
        writer.WriteValueWithFormatter(formatter, valNull);
        writer.WriteValueWithFormatter(formatter, ints);
        var bytes = writer.GetBytes();
        Assert.NotEmpty(bytes);
        PackReader reader = new(bytes);
        Assert.Equal(ints, reader.ReadValueWithFormatter(formatter));
        Assert.Null(reader.ReadValueWithFormatter(formatter));
        reader.ReadValue(ref col!, formatter);
        Assert.Equal(ints, col);
        Assert.Equal(0, reader.Remaining);
        Assert.Null(reader.ReadValueWithFormatter(formatter));
    }

    [Fact]
    public void ObservableCollectionTests()
    {
        PackWriter writer = new();
        var formatter = new ObservableCollectionFormatter<int>();
        Assert.NotNull(formatter);
        ObservableCollection<int>? ints = [555, 555, 556, 457476];
        ObservableCollection<int>? valNull = null;
        ObservableCollection<int>? col = new();
        writer.WriteValueWithFormatter(formatter, ints);
        writer.WriteValueWithFormatter(formatter, valNull);
        writer.WriteValueWithFormatter(formatter, ints);
        var bytes = writer.GetBytes();
        Assert.NotEmpty(bytes);
        PackReader reader = new(bytes);
        Assert.Equal(ints, reader.ReadValueWithFormatter(formatter));
        Assert.Null(reader.ReadValueWithFormatter(formatter));
        reader.ReadValue(ref col!, formatter);
        Assert.Equal(ints, col);
        Assert.Equal(0, reader.Remaining);
    }

    [Fact]
    public void ListTests()
    {
        PackWriter writer = new();
        var formatter = new ListFormatter<int>();
        Assert.NotNull(formatter);
        List<int>? ints = [555, 555, 556, 457476];
        List<int>? valNull = null;
        List<int>? col = new();
        writer.WriteValueWithFormatter(formatter, ints);
        writer.WriteValueWithFormatter(formatter, valNull);
        writer.WriteValueWithFormatter(formatter, ints);
        var bytes = writer.GetBytes();
        Assert.NotEmpty(bytes);
        PackReader reader = new(bytes);
        Assert.Equal(ints, reader.ReadValueWithFormatter(formatter));
        Assert.Null(reader.ReadValueWithFormatter(formatter));
        reader.ReadValue(ref col!, formatter);
        Assert.Equal(ints, col);
        Assert.Equal(0, reader.Remaining);
    }

    [Fact]
    public void LinkedListests()
    {
        PackWriter writer = new();
        var formatter = new LinkedListFormatter<int>();
        Assert.NotNull(formatter);
        LinkedList<int>? ints = new([555, 555, 556, 457476]);
        LinkedList<int>? valNull = null;
        LinkedList<int>? col = new();
        writer.WriteValueWithFormatter(formatter, ints);
        writer.WriteValueWithFormatter(formatter, valNull);
        writer.WriteValueWithFormatter(formatter, ints);
        var bytes = writer.GetBytes();
        Assert.NotEmpty(bytes);
        PackReader reader = new(bytes);
        Assert.Equal(ints, reader.ReadValueWithFormatter(formatter));
        Assert.Null(reader.ReadValueWithFormatter(formatter));
        reader.ReadValue(ref col!, formatter);
        Assert.Equal(ints, col);
        Assert.Equal(0, reader.Remaining);
    }


    [Fact]
    public void HashSetListests()
    {
        PackWriter writer = new();
        var formatter = new HashSetFormatter<int>();
        Assert.NotNull(formatter);
        HashSet<int>? ints = new([555, 555, 556, 457476]);
        HashSet<int>? valNull = null;
        HashSet<int>? col = new();
        writer.WriteValueWithFormatter(formatter, ints);
        writer.WriteValueWithFormatter(formatter, valNull);
        writer.WriteValueWithFormatter(formatter, ints);
        var bytes = writer.GetBytes();
        Assert.NotEmpty(bytes);
        PackReader reader = new(bytes);
        Assert.Equal(ints, reader.ReadValueWithFormatter(formatter));
        Assert.Null(reader.ReadValueWithFormatter(formatter));
        reader.ReadValue(ref col!, formatter);
        Assert.Equal(ints, col);
        Assert.Equal(0, reader.Remaining);
    }

    [Fact]
    public void SortedSetListests()
    {
        PackWriter writer = new();
        var formatter = new SortedSetFormatter<int>();
        Assert.NotNull(formatter);
        SortedSet<int>? ints = new([555, 555, 556, 457476]);
        SortedSet<int>? valNull = null;
        SortedSet<int>? col = new();
        writer.WriteValueWithFormatter(formatter, ints);
        writer.WriteValueWithFormatter(formatter, valNull);
        writer.WriteValueWithFormatter(formatter, ints);
        var bytes = writer.GetBytes();
        Assert.NotEmpty(bytes);
        PackReader reader = new(bytes);
        Assert.Equal(ints, reader.ReadValueWithFormatter(formatter));
        Assert.Null(reader.ReadValueWithFormatter(formatter));
        reader.ReadValue(ref col!, formatter);
        Assert.Equal(ints, col);
        Assert.Equal(0, reader.Remaining);
    }

    [Fact]
    public void QueueListests()
    {
        PackWriter writer = new();
        var formatter = new QueueFormatter<int>();
        Assert.NotNull(formatter);
        Queue<int>? ints = new();
        ints.Enqueue(int.MaxValue);
        Queue<int>? valNull = null;
        Queue<int>? col = new();
        writer.WriteValueWithFormatter(formatter, ints);
        writer.WriteValueWithFormatter(formatter, valNull);
        writer.WriteValueWithFormatter(formatter, ints);
        var bytes = writer.GetBytes();
        Assert.NotEmpty(bytes);
        //Assert.Equal("x", Convert.ToHexString(bytes));
        PackReader reader = new(bytes);
        Queue<int>? readQueue = reader.ReadValueWithFormatter(formatter);
        Assert.NotNull(readQueue);
        Assert.Single(readQueue);
        Assert.Equal([int.MaxValue], readQueue.ToList());
        Assert.Null(reader.ReadValueWithFormatter(formatter));
        reader.ReadValue(ref col!, formatter);
        Assert.Equal(ints, col);
        Assert.Equal(0, reader.Remaining);
    }

    [Fact]
    public void StackListests()
    {
        PackWriter writer = new();
        var formatter = new StackFormatter<int>();
        Assert.NotNull(formatter);
        Stack<int>? ints = new([555, 555, 556, 457476]);
        Stack<int>? valNull = null;
        Stack<int>? col = new();
        writer.WriteValueWithFormatter(formatter, ints);
        writer.WriteValueWithFormatter(formatter, valNull);
        writer.WriteValueWithFormatter(formatter, ints);
        var bytes = writer.GetBytes();
        Assert.NotEmpty(bytes);
        PackReader reader = new(bytes);
        Assert.Equal(ints, reader.ReadValueWithFormatter(formatter));
        Assert.Null(reader.ReadValueWithFormatter(formatter));
        reader.ReadValue(ref col!, formatter);
        Assert.Equal(ints, col);
        Assert.Equal(0, reader.Remaining);
        Assert.Null(reader.ReadValueWithFormatter(formatter));
    }
}
