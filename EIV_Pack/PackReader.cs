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
    /// Initializes a new instance of the <see cref="PackReader"/> struct.
    /// </summary>
    /// <param name="sequence">The sequnece to read.</param>
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
    /// Initializes a new instance of the <see cref="PackReader"/> struct.
    /// </summary>
    /// <param name="buffer">The buffer to read.</param>
    public PackReader(ReadOnlySpan<byte> buffer)
    {
        bufferSource = new ReadOnlySequence<byte>(buffer.ToArray());
        currentBuffer = buffer;
        Length = buffer.Length;
        TextEncoding = Encoding.UTF8;
    }

    /// <summary>
    /// Gets the length of consumed data.
    /// </summary>
    public readonly int Consumed => consumed;

    /// <summary>
    /// Gets the length remaining data.
    /// </summary>
    public readonly long Remaining => Length - consumed;

    /// <summary>
    /// Gets the length of the data.
    /// </summary>
    public readonly long Length { get; }

    /// <summary>
    /// Gets the text encoding for strings.
    /// </summary>
    public readonly Encoding TextEncoding { get; }

    /// <summary>
    /// Advance the byte with <paramref name="count"/>.
    /// </summary>
    /// <param name="count">The count to advance.</param>
    /// <exception cref="InvalidOperationException">Thrown when remaining is more than we can advance.</exception>
    public void Advance(int count)
    {
        if (count == 0)
        {
            return;
        }

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
        {
            return;
        }

        consumed = inConsumed;

#if NETSTANDARD
        currentBuffer = bufferSource.Slice(consumed, Remaining).First.Span;
#else
        currentBuffer = bufferSource.Slice(consumed, Remaining).FirstSpan;
#endif
    }
}
