using System.Buffers;

namespace EIV_Pack;

public ref partial struct PackWriter : IDisposable
{
    public readonly ReadOnlySequence<byte> GetReadOnlySequence()
    {
        return recyclable.GetReadOnlySequence();
    }

    public readonly ArraySegment<byte> GetAsSegment()
    {
        recyclable.TryGetBuffer(out ArraySegment<byte> buffer);
        return buffer;
    }

    public readonly byte[] GetBytes()
    {
        int len = (int)recyclable.Length;
#if NET8_0_OR_GREATER
        return recyclable.GetBuffer()[..len];
#else
        return [.. recyclable.GetBuffer().Take(len)];
#endif
    }
}
