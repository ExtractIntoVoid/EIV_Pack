namespace EIV_Pack;

/// <summary>
/// An attribute to ignore a property or a field for generation.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class EIV_PackIgnoreAttribute : Attribute;
