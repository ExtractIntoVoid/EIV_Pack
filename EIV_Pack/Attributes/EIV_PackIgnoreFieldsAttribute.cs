namespace EIV_Pack;

/// <summary>
/// Selected name of the fields to ignore during generation.
/// </summary>
/// <param name="fields">The name of the fields to ignore.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class EIV_PackIgnoreFieldsAttribute(params string[] fields) : Attribute
{
    /// <summary>
    /// Gets the fields to ignore.
    /// </summary>
    public string[] Fields { get; } = fields;
}
