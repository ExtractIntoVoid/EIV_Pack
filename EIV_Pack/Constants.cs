using Microsoft.IO;

namespace EIV_Pack;

public static class Constants
{
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

    public const int NullHeader = -1;
    public const byte SmallNullHeader = 0;
}
