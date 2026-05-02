using System.Buffers;
using System.Text;

namespace EIV_Pack;

/// <summary>
/// A reader struct for reading a packed data.
/// </summary>
public ref partial struct PackReader
{
    private readonly ReadOnlySequence<byte> bufferSource;
    private ReadOnlySpan<byte> currentBuffer;
    private int consumed;
    /// <summary>
    /// The text encoding for strings.
    /// </summary>
    public readonly Encoding TextEncoding;

    /// <summary>
    /// The length of the data.
    /// </summary>
    public readonly long Length;

    /// <summary>
    /// The length of consumed data.
    /// </summary>
    public readonly int Consumed => consumed;

    /// <summary>
    /// The length remaining data.
    /// </summary>
    public readonly long Remaining => Length - consumed;

    /// <summary>
    /// Creates a pack reader from <paramref name="sequence"/>.
    /// </summary>
    /// <param name="sequence"></param>
    public PackReader(in ReadOnlySequence<byte> sequence)
    {
        bufferSource = sequence;
#if NETSTANDARD
        currentBuffer = sequence.First.Span;
#else
        currentBuffer = sequence.FirstSpan;
#endif
        Length = sequence.Length;
        TextEncoding = Encoding.UTF8;
    }

    /// <summary>
    /// Creates a pack reader from the <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer"></param>
    public PackReader(ReadOnlySpan<byte> buffer)
    {
        bufferSource = new ReadOnlySequence<byte>(buffer.ToArray());
        currentBuffer = buffer;
        Length = buffer.Length;
        TextEncoding = Encoding.UTF8;
    }

    /// <summary>
    /// Advance the byte with <paramref name="count"/>.
    /// </summary>
    /// <param name="count">The count to advance.</param>
    /// <exception cref="InvalidOperationException">Thrown when remaining is more than we can advance.</exception>
    public void Advance(int count)
    {
        if (count == 0)
            return;

        if (Remaining < count)
        {
            throw new InvalidOperationException("Remaining bytes cannot read this type!");
        }

#if NETSTANDARD
        currentBuffer = bufferSource.Slice(consumed + count, Remaining - count).First.Span;
#else
        currentBuffer = bufferSource.Slice(consumed + count, Remaining - count).FirstSpan;
#endif
        consumed += count;
    }

    /// <summary>
    /// Sets the new <see cref="Consumed"/> value.
    /// </summary>
    /// <param name="inConsumed">The new value.</param>
    public void SetConsumed(int inConsumed)
    {
        if (inConsumed > Length)
            return;

        consumed = inConsumed;

#if NETSTANDARD
        currentBuffer = bufferSource.Slice(consumed, Remaining).First.Span;
#else
        currentBuffer = bufferSource.Slice(consumed, Remaining).FirstSpan;
#endif
    }
}
