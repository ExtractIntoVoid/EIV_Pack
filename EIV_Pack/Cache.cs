using EIV_Pack.Formatters;

namespace EIV_Pack;

internal static class Cache<T>
{
    public static bool IsRegistered { get; internal set; }

    public static IFormatter<T>? Formatter { get; internal set; }
}
