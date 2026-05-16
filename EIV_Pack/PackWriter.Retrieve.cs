using Microsoft.IO;
using System.Buffers;

namespace EIV_Pack;

public ref partial struct PackWriter : IDisposable
{
    /// <summary>
    /// Returns a sequence containing the contents of the writer.
    /// </summary>
    /// <returns>A <see cref="ReadOnlySequence{T}"/> of bytes.</returns>
    public readonly ReadOnlySequence<byte> GetReadOnlySequence()
    {
        return recyclable.GetReadOnlySequence();
    }

    /// <summary>
    /// Returns a segment containing the contents of the writer
    /// </summary>
    /// <returns>A <see cref="ArraySegment{T}"/> of bytes.</returns>
    public readonly ArraySegment<byte> GetAsSegment()
    {
        recyclable.TryGetBuffer(out ArraySegment<byte> buffer);
        return buffer;
    }

    /// <summary>
    /// Returns the buffer of the writer.
    /// </summary>
    /// <returns>An <see cref="Array"/> of bytes.</returns>
    public readonly byte[] GetBytes()
    {
        int len = (int)recyclable.Length;
#if !NETSTANDARD2_0
        return recyclable.GetBuffer()[..len];
#else
        return [.. recyclable.GetBuffer().Take(len)];
#endif
    }
}
