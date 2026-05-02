namespace EIV_Pack;

/// <summary>
/// Packable generator options.
/// </summary>
public enum GenerateType
{
    /// <summary>
    /// No generate option.
    /// </summary>
    None,
    /// <summary>
    /// Generate a version tolerant version.
    /// </summary>
    VersionTolerant,
    /// <summary>
    /// Nothing will be generated.
    /// </summary>
    NoGenerate,
}

/// <summary>
/// An attribute to generate Packable on the target.
/// </summary>
/// <remarks>
/// Creats a
/// </remarks>
/// <param name="type"></param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class EIV_PackableAttribute(GenerateType type = GenerateType.None) : Attribute
{
    /// <summary>
    /// A type to generate the packable target.
    /// </summary>
    public GenerateType GenerateType { get; } = type;
}

/// <summary>
/// An attribute to ignore a property or a field for generation.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class EIV_PackIgnoreAttribute : Attribute;

/// <summary>
/// Selected name of the fields to ignore during generation.
/// </summary>
/// <param name="fields"></param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class EIV_PackIgnoreFieldsAttribute(params string[] fields) : Attribute
{
    /// <summary>
    /// The fields to ignore.
    /// </summary>
    public string[] Fields { get; } = fields;
}

/// <summary>
/// Selected name of properties to ignore during generation.
/// </summary>
/// <param name="properties"></param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class EIV_PackIgnorePropertiesAttribute(params string[] properties) : Attribute
{
    /// <summary>
    /// The properties to ignore.
    /// </summary>
    public string[] Properties { get; } = properties;
}

/// <summary>
/// An order to generate the pack.
/// </summary>
/// <param name="order">The order</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class EIV_PackOrderAttribute(int order) : Attribute
{
    /// <summary>
    /// The field or property order.
    /// </summary>
    public int Order { get; } = order;
}