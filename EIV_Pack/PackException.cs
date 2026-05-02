#if !NETSTANDARD2_0
using System.Diagnostics.CodeAnalysis;
#endif

namespace EIV_Pack;

/// <summary>
/// An excepton for EIV Pack related errors.
/// </summary>
/// <param name="message"></param>
public class PackException(string message) : Exception(message)
{
    /// <summary>
    /// A simple throwing.
    /// </summary>
    /// <param name="message"></param>
    /// <exception cref="PackException"></exception>
#if !NETSTANDARD2_0
    [DoesNotReturn]
#endif
    public static void ThrowMessage(string message)
    {
        throw new PackException(message);
    }

    /// <summary>
    /// An error when a <paramref name="type"/> is not registered the provider.
    /// </summary>
    /// <param name="type"></param>
    /// <exception cref="PackException"></exception>
#if !NETSTANDARD2_0
    [DoesNotReturn]
#endif
    public static void ThrowNotRegisteredInProvider(Type type)
    {
        throw new PackException($"{type.FullName} is not registered in this provider.");
    }

    /// <summary>
    /// An error when a depth limit is reached. 
    /// </summary>
    /// <param name="type"></param>
    /// <exception cref="PackException"></exception>
#if !NETSTANDARD2_0
    [DoesNotReturn]
#endif
    public static void ThrowReachedDepthLimit(Type type)
    {
        throw new PackException($"Serializing Type '{type}' reached depth limit, maybe detect circular reference.");
    }

    /// <summary>
    /// An error when an expected and an actual header is not a same.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="expected"></param>
    /// <param name="actual"></param>
    /// <exception cref="PackException"></exception>
#if !NETSTANDARD2_0
    [DoesNotReturn]
#endif
    public static void ThrowHeaderNotSame(Type type, int expected, int actual)
    {
        throw new PackException($"{type.FullName} is failed to deserialize! Expected Header: {expected} actual: {actual}.");
    }
}
