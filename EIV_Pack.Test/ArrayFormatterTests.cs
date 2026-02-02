using System;
using System.Buffers;
using EIV_Pack.Formatters;

namespace EIV_Pack.Test;

public class ArrayFormatterTests
{
    [Fact]
    public void ArrayTest()
    {
        PackWriter writer = new();
        var intArrayFormatter = new ArrayFormatter<int>();
        Assert.NotNull(intArrayFormatter);
        int[] ints = [555, 555, 556, 457476];
        writer.WriteValueWithFormatter(intArrayFormatter, ints);
        var bytes = writer.GetBytes();
        Assert.NotEmpty(bytes);
        PackReader reader = new(bytes);
        Assert.Equal(ints, reader.ReadValueWithFormatter<int[]>(intArrayFormatter));
        Assert.Equal(0, reader.Remaining);
    }

    [Fact]
    public void ArraySegmentTest()
    {
        PackWriter writer = new();
        var intArrayFormatter = new ArraySegmentFormatter<int>();
        Assert.NotNull(intArrayFormatter);
        ArraySegment<int> ints = new([555, 555, 556, 457476]);
        writer.WriteValueWithFormatter(intArrayFormatter, ints);
        var bytes = writer.GetBytes();
        Assert.NotEmpty(bytes);
        PackReader reader = new(bytes);
        Assert.Equal(ints, reader.ReadValueWithFormatter(intArrayFormatter));
        Assert.Equal(0, reader.Remaining);
    }

    [Fact]
    public void MemoryTest()
    {
        PackWriter writer = new();
        var intArrayFormatter = new MemoryFormatter<int>();
        Assert.NotNull(intArrayFormatter);
        Memory<int> ints = new([555, 555, 556, 457476]);
        writer.WriteValueWithFormatter(intArrayFormatter, ints);
        var bytes = writer.GetBytes();
        Assert.NotEmpty(bytes);
        PackReader reader = new(bytes);
        Assert.Equal(ints, reader.ReadValueWithFormatter(intArrayFormatter));
        Assert.Equal(0, reader.Remaining);
    }

    [Fact]
    public void ReadOnlyMemoryTest()
    {
        PackWriter writer = new();
        var intArrayFormatter = new ReadOnlyMemoryFormatter<int>();
        Assert.NotNull(intArrayFormatter);
        ReadOnlyMemory<int> ints = new([555, 555, 556, 457476]);
        writer.WriteValueWithFormatter(intArrayFormatter, ints);
        var bytes = writer.GetBytes();
        Assert.NotEmpty(bytes);
        PackReader reader = new(bytes);
        Assert.Equal(ints, reader.ReadValueWithFormatter(intArrayFormatter));
        Assert.Equal(0, reader.Remaining);
    }
}
