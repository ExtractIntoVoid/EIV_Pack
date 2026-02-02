using System;
using System.Collections.Generic;
using System.Text;

namespace EIV_Pack.Test;

public class ThrowTests
{

    [Fact]
    public void TestAllThrow()
    {
        Assert.Throws<PackException>(() => PackException.ThrowMessage("test"));
        Assert.Throws<PackException>(() => PackException.ThrowNotRegisteredInProvider(typeof(int)));
        Assert.Throws<PackException>(() => PackException.ThrowReachedDepthLimit(typeof(int)));
        Assert.Throws<PackException>(() => PackException.ThrowHeaderNotSame(typeof(int), 5, 4));
    }
}
