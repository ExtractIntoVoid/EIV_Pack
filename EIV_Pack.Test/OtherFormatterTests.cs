using EIV_Pack.Formatters;

namespace EIV_Pack.Test;

public class OtherFormatterTests
{
    [Fact]
    public void LazyTest()
    {
        PackWriter writer = new();
        var formatter = new LazyFormatter<int>();
        Assert.NotNull(formatter);
        Lazy<int> val = new(6);
        Lazy<int>? vallNull = null;
        writer.WriteValueWithFormatter(formatter, val);
        writer.WriteValueWithFormatter(formatter, vallNull);
        var bytes = writer.GetBytes();
        Assert.NotEmpty(bytes);
        PackReader reader = new(bytes);
        Assert.Equal(val.Value, reader.ReadValueWithFormatter(formatter)?.Value);
        Assert.Null(reader.ReadValueWithFormatter(formatter));
        Assert.Equal(0, reader.Remaining);
        Assert.Null(reader.ReadValueWithFormatter(formatter));
    }


    [Fact]
    public void ThrowErrorTest()
    {
        var formatter = new ErrorFormatter<int>();
        Assert.Throws<PackException>(() =>
        {
            PackWriter writer = new();
            writer.WriteValueWithFormatter(formatter, 0);
        });
        Assert.Throws<PackException>(() =>
        {
            PackReader reader = new();
            reader.ReadValueWithFormatter(formatter);
        });
    }
}
