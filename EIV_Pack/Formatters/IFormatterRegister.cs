namespace EIV_Pack.Formatters;

/// <summary>
/// Formatter register.
/// </summary>
/// <remarks>
/// Only available for NET8+.
/// </remarks>
#pragma warning disable CA1040
public interface IFormatterRegister
{
#if !NETSTANDARD2_0
    /// <summary>
    /// Register the formatter.
    /// </summary>
    static abstract void RegisterFormatter();
#endif
}
#pragma warning restore CA1040