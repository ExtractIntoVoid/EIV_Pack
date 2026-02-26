namespace EIV_Pack.Example;

public interface Itest
{
    public int testValeToImp { get; set; }
}

[EIV_Packable]
public partial class Test : Itest
{
    public int testValeToImp { get; set; }
    public int test;
    public int TESTProp { get; set; }
    [EIV_PackIgnore]
    public int _ignore;
}

[EIV_Packable]
public partial class TEST2 : Test
{
    public int test2;
}


[EIV_Packable]
[EIV_PackIgnoreFields([nameof(test4), nameof(test)])]
[EIV_PackIgnoreProperties([nameof(TESTProp2)])]
public partial class TEST4 : TEST2
{
    public int test4;
    public int test6;
    public int TESTProp2 { get; set; }
    public int TESTProp4 { get; set; }
}



[EIV_Packable]
public partial class MyCor
{
    public int test2;

    public List<MyCor> MyCorRec = [];

}

#if NET8_0_OR_GREATER

public interface IInitTest
{
    public int Test2 { get; init; }
}

[EIV_Packable]
public partial class InitTest : IInitTest
{
    public required int Test2 { get; init; }

    public int Test { get; set; }
    public required bool boooll;
}
#endif