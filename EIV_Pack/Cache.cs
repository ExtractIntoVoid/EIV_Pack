using EIV_Pack.Formatters;

namespace EIV_Pack;

/// <summary>
/// Provides a formatter cache.
/// </summary>
/// <typeparam name="T">Any type.</typeparam>
internal static class Cache<T>
{
    /// <summary>
    /// Gets or sets a value indicating whether the <typeparamref name="T"/> is registered.
    /// </summary>
    public static bool IsRegistered { get; internal set; }

    /// <summary>
    /// Gets or sets the formatter for <typeparamref name="T"/>.
    /// </summary>
    public static IFormatter<T>? Formatter { get; internal set; }
}
