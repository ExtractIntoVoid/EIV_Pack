using Microsoft.IO;
using System.Text;

namespace EIV_Pack;

/// <summary>
/// Writer that serialize data.
/// </summary>
/// <param name="encoding">The encoding to write strings.</param>
public ref partial struct PackWriter(Encoding encoding) : IDisposable
{
    /// <summary>
    /// Maximum allowed depth for recursive writing.
    /// </summary>
    public const int DepthLimit = 1000;

    private readonly RecyclableMemoryStream recyclable = Constants.StreamManager.GetStream();

    private int depth = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="PackWriter"/> struct.
    /// </summary>
    public PackWriter()
        : this(Encoding.UTF8)
    {
    }

    /// <inheritdoc cref="Encoding"/>
    public readonly Encoding TextEncoding { get; } = encoding;

    /// <inheritdoc />
    public readonly void Dispose()
    {
        recyclable.Dispose();
    }
}
