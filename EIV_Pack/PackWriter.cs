using Microsoft.IO;
using System.Text;

namespace EIV_Pack;

public ref partial struct PackWriter(Encoding encoding) : IDisposable
{
    private readonly RecyclableMemoryStream recyclable = Constants.StreamManager.GetStream();
    /// <inheritdoc cref="Encoding"/>
    public readonly Encoding TextEncoding = encoding;
    private int depth = 0;
    private const int DepthLimit = 1000;

    public PackWriter() : this(Encoding.UTF8) { }


    /// <inheritdoc/>
    public readonly void Dispose()
    {
        recyclable.Dispose();
    }
}
