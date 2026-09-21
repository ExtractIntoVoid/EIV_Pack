namespace EIV_Pack;

/// <summary>
/// An attribute to generate Packable on the target.
/// </summary>
/// <param name="type">The generate type.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class EIV_PackableAttribute(GenerateType type = GenerateType.None) : Attribute
{
    /// <summary>
    /// Gets a type to generate the packable target.
    /// </summary>
    public GenerateType Type { get; } = type;
}
