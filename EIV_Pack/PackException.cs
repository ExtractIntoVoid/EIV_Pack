#if !NETSTANDARD2_0
using System.Diagnostics.CodeAnalysis;
#endif

namespace EIV_Pack;

/// <summary>
/// An excepton for EIV Pack related errors.
/// </summary>
public class PackException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PackException"/> class.
    /// </summary>
    public PackException()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PackException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public PackException(string message)
    : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PackException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public PackException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}