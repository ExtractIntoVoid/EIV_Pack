#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace EIV_Pack;

public class PackException(string message) : Exception(message)
{
#if NET8_0_OR_GREATER
    [DoesNotReturn]
#endif
    public static void ThrowMessage(string message)
    {
        throw new PackException(message);
    }

#if NET8_0_OR_GREATER
    [DoesNotReturn]
#endif
    public static void ThrowNotRegisteredInProvider(Type type)
    {
        throw new PackException($"{type.FullName} is not registered in this provider.");
    }

#if NET8_0_OR_GREATER
    [DoesNotReturn]
#endif
    public static void ThrowReachedDepthLimit(Type type)
    {
        throw new PackException($"Serializing Type '{type}' reached depth limit, maybe detect circular reference.");
    }

#if NET8_0_OR_GREATER
    [DoesNotReturn]
#endif
    public static void ThrowHeaderNotSame(Type type, int expected, int actual)
    {
        throw new PackException($"{type.FullName} is failed to deserialize! Expected Header: {expected} actual: {actual}.");
    }
}
