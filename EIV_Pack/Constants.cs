using Microsoft.IO;

namespace EIV_Pack;

/// <summary>
/// Collection of readonly and constants values.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Empty collection of span.
    /// </summary>
    public static ReadOnlySpan<byte> EmptyCollection => new byte[4];
    internal static readonly RecyclableMemoryStreamManager StreamManager = new()
    {
        Settings =
        {
            BlockSize = 1024,
            LargeBufferMultiple = 1024 * 1024,
            MaximumBufferSize = 1024 * 1024 * 16,
        }
    };

    /// <summary>
    /// A header indicating a null value.
    /// </summary>
    public const int NullHeader = -1;

    /// <summary>
    /// A small header indicating a null value.
    /// </summary>
    public const byte SmallNullHeader = 0;

    /// <summary>
    /// A context switch to register default formatters.
    /// </summary>
    /// <remarks>
    /// Check out <see cref="FormatterProvider.RegisterFormatters"/> what it will be registered.
    /// </remarks>
    public static bool IsRegisterDefaultFormatters { get; } =
        !AppContext.TryGetSwitch("EIV_Pack.FormatterProvider.IsRegisterDefaultFormatters", out var isEnabled) || isEnabled;
}
