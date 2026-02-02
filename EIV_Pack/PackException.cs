using System.Diagnostics.CodeAnalysis;

namespace EIV_Pack;

public class PackException(string message) : Exception(message)
{
    [DoesNotReturn]
    public static void ThrowMessage(string message)
    {
        throw new PackException(message);
    }

    [DoesNotReturn]
    public static void ThrowNotRegisteredInProvider(Type type)
    {
        throw new PackException($"{type.FullName} is not registered in this provider.");
    }

    [DoesNotReturn]
    public static void ThrowReachedDepthLimit(Type type)
    {
        throw new PackException($"Serializing Type '{type}' reached depth limit, maybe detect circular reference.");
    }

    [DoesNotReturn]
    public static void ThrowHeaderNotSame(Type type, int expected, int actual)
    {
        throw new PackException($"{type.FullName} is failed to deserialize! Expected Header: {expected} actual: {actual}.");
    }
}
