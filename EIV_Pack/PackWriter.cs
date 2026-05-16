using Microsoft.IO;
using System.Text;

namespace EIV_Pack;

/// <summary>
/// Writer that serialize data.
/// </summary>
/// <param name="encoding">The encoding to write strings.</param>
public ref partial struct PackWriter(Encoding encoding) : IDisposable
{
    private readonly RecyclableMemoryStream recyclable = Constants.StreamManager.GetStream();
    /// <inheritdoc cref="Encoding"/>
    public readonly Encoding TextEncoding = encoding;
    private int depth = 0;
    /// <summary>
    /// Maximum allowed depth for recursive writing.
    /// </summary>
    public const int DepthLimit = 1000;

    /// <summary>
    /// Writer that serialize data.
    /// </summary>
    public PackWriter() : this(Encoding.UTF8) { }


    /// <inheritdoc />
    public readonly void Dispose()
    {
        recyclable.Dispose();
    }
}
