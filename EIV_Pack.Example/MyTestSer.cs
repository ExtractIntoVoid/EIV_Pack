namespace EIV_Pack.Example;

[EIV_Packable]
public partial class MyTestClass
{
    public string? strrr;
    public RuntimeFieldHandle rfh;
    public KeyValuePair<int, int> fsdfsf;
    public int yeet;
    public int[]? yeetArray;
    public int Yeet { get; set; }

    public List<int> test = [];
}

[EIV_Packable]
public partial record class MyTestRecordClass
{
    public int yeet;
    public int Yeet { get; set; }
}

[EIV_Packable]
public partial record struct MyTestRecordStruct
{
    public int yeet;
    public int Yeet { get; set; }
}

[EIV_Packable]
public partial struct MyTestStruct
{
    public int yeet;
    public int Yeet { get; set; }
}

[EIV_Packable]
internal partial class MyTestClassInternal
{
    public int yeet;
    public int Yeet { get; set; }
}

[EIV_Packable]
internal partial record class MyTestRecordClassInternal
{
    public int yeet;
    public int Yeet { get; set; }
}

[EIV_Packable]
internal partial record struct MyTestRecordStructInternal
{
    public int yeet;
    public int Yeet { get; set; }
}

[EIV_Packable]
internal partial struct MyTestStructInternal
{
    public int yeet;
    public int Yeet { get; set; }
}