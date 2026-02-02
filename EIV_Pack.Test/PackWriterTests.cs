using System;
using System.Collections.Generic;
using System.Text;

namespace EIV_Pack.Test;

public class PackWriterTests
{

    [Fact]
    public void MainTest()
    {
        CustomType.RegisterFormatter();
        PackWriter writer = new();
        CustomType tt = new()
        {
            Value = "fsdfs"
        };
        writer.WriteHeader();
        writer.WriteString(string.Empty);
        writer.WritePackable(tt);
        writer.WritePackable(tt);
        writer.WriteString(null);

        Assert.NotEqual(0, writer.GetAsSegment().Count);
        Assert.NotEqual(0, writer.GetReadOnlySequence().Length);

        PackReader reader = new(writer.GetReadOnlySequence());
        reader.Advance(0);
        Assert.True(reader.TryPeekHeader(out var len));
        Assert.Equal(-1, len);
        Assert.True(reader.PeekIsNullOrEmpty());
        reader.ReadHeader();
        reader.ReadString();
        reader.ReadPackable<CustomType>();
        reader.ReadPackable(ref tt!);
        Assert.Throws<InvalidOperationException>(() =>
        {
            PackReader reader1 = new();
            reader1.Advance(55);
        });
        Assert.Throws<InvalidOperationException>(() =>
        {
            PackReader reader1 = new();
            reader1.ReadUnmanaged<int>();
        });
        PackReader reader1 = new();
        Assert.True(reader1.PeekIsNullOrEmpty());
        reader1.ReadString();
        reader1.ReadArray<int>();


        PackWriter writer1 = new();
        writer1.WriteArray([666, 666, 666, 666, 6666666]);
        writer1.WriteArray([666, 666, 666, 666, 6666666]);
        writer1.WriteArray([666]);
        writer1.WriteArray([666]);
        PackReader reader2 = new(writer1.GetReadOnlySequence());
        int[]? ints = new int[4];
        int[] ? intsNull = null;
        reader2.ReadArray<int>(ref ints);
        reader2.ReadArray<int>(ref intsNull);
        reader2.ReadArray<int>(ref ints, 0);
        reader2.ReadArray<int>(ref ints, 1);
        intsNull = null;
        reader2.ReadArray<int>(ref intsNull, 1);
    }
}
