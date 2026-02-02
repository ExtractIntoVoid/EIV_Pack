using EIV_Pack.Formatters;

namespace EIV_Pack.Test;

public class SerializerTest
{
    [Fact]
    public void SerNormalTest()
    {
        int? i = null;
        byte[] nullSer = Serializer.Serialize(i);
        Assert.NotNull(nullSer);

        int? val = Serializer.Deserialize<int?>(nullSer);
        Assert.Equal(i.HasValue, val.HasValue);
    }

    [Fact]
    public void SerArrayTest()
    {
        int[]? i = null;
        byte[] nullSer = Serializer.SerializeArray(i);
        Assert.NotNull(nullSer);

        nullSer = Serializer.SerializeArray<int>([]);
        Assert.NotNull(nullSer);

        int?[]? x = Serializer.DeserializeArray<int?>([]);
        Assert.Null(x);

        x = Serializer.DeserializeArray<int?>(Constants.EmptyCollection.ToArray());
        Assert.NotNull(x);
        Assert.Empty(x);

        byte[] valid = Serializer.SerializeArray<int>([666]);

        Assert.NotNull(valid);

        int[]? res = Serializer.DeserializeArray<int>(valid);
        Assert.NotNull(res);

        Assert.Equal([666], res);
    }

}
