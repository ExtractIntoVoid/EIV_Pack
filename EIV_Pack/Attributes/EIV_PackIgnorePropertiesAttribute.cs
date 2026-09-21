namespace EIV_Pack;

/// <summary>
/// Selected name of properties to ignore during generation.
/// </summary>
/// <param name="properties">The name of the properties to ignore.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class EIV_PackIgnorePropertiesAttribute(params string[] properties) : Attribute
{
    /// <summary>
    /// Gets the properties to ignore.
    /// </summary>
    public string[] Properties { get; } = properties;
}
