namespace EIV_Pack;

/// <summary>
/// An order to generate the pack.
/// </summary>
/// <param name="order">The order.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class EIV_PackOrderAttribute(int order) : Attribute
{
    /// <summary>
    /// Gets the field or property order.
    /// </summary>
    public int Order { get; } = order;
}