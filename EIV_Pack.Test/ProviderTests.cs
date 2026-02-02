using System;
using System.Collections.Generic;
using System.Text;

namespace EIV_Pack.Test;

public class ProviderTests
{
    class Test;

    [Fact]
    public void ProviderTest()
    {
        Assert.Throws<PackException>(() => FormatterProvider.GetFormatter<Test>());

        FormatterProvider.Register<Test>(null);

        Assert.Throws<PackException>(() => FormatterProvider.GetFormatter<Test>());
    }

    [Fact]
    public void CustomTypeTest()
    {
        CustomType customType = new()
        { 
            Value = "sdfsf"
        };

        FormatterProvider.Register<CustomType>();

        var bytes = Serializer.Serialize(customType);
        Assert.NotEmpty(bytes);

        var deser = Serializer.Deserialize<CustomType>(bytes);
        Assert.NotNull(deser);
        Assert.Equal(customType.Value, deser.Value);
    }
}
