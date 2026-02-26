using EIV_Pack.Formatters;
using System.Collections.Concurrent;
using System.Numerics;
#if NET8_0_OR_GREATER
using System.Text;
#endif

namespace EIV_Pack;

/// <summary>
/// Provide access to cached <see cref="IFormatter{T}"/>.
/// </summary>
public static class FormatterProvider
{
    static readonly ConcurrentDictionary<Type, IFormatter> formatters = new(Environment.ProcessorCount, 150);

    static FormatterProvider()
    {
        RegisterFormatters();
    }

    /// <summary>
    /// Checks if the <typeparamref name="T"/> has been registered.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <returns><see langword="true"/> if the type is registered, otherwise <see langword="false"/>.</returns>
    public static bool IsRegistered<T>()
    {
        return Cache<T>.IsRegistered;
    }

    /// <summary>
    /// Registers a <paramref name="formatter"/> into type cache.
    /// </summary>
    /// <typeparam name="T">Type to register the formatter.</typeparam>
    /// <param name="formatter">The created formatter.</param>
    public static void Register<T>(IFormatter<T> formatter)
    {
        Cache<T>.IsRegistered = true;
        Cache<T>.Formatter = formatter;
        formatters[typeof(T)] = formatter;
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// Registers a <typeparamref name="T"/> into type cache.
    /// </summary>
    /// <typeparam name="T">Type to register.</typeparam>
    public static void Register<T>() where T : IFormatterRegister
    {
        T.RegisterFormatter();
    }
#endif

    /// <summary>
    /// Gets the <see cref="IFormatter{T}"/> from the cache.
    /// </summary>
    /// <typeparam name="T">The type to get the formatter.</typeparam>
    /// <returns>The cached <see cref="IFormatter{T}"/>.</returns>
    /// <remarks>
    /// If the <typeparamref name="T"/> type has not been registered or is a null formatter it will throw <see cref="PackException"/>.
    /// </remarks>
    public static IFormatter<T> GetFormatter<T>()
    {
        if (!IsRegistered<T>())
            PackException.ThrowNotRegisteredInProvider(typeof(T));

        var formatter = Cache<T>.Formatter;
        if (formatter == null)
            PackException.ThrowNotRegisteredInProvider(typeof(T));

        return formatter!;
    }

    public static IFormatter GetFormatter(Type type)
    {
        if (!formatters.TryGetValue(type, out var formatter))
        {
            PackException.ThrowNotRegisteredInProvider(type);
        }

        return formatter;
    }

    private static void RegisterFormatters()
    {
        RegisterToAll<SByte>();
        RegisterToAll<Byte>();
        RegisterToAll<Int16>();
        RegisterToAll<UInt16>();
        RegisterToAll<Int32>();
        RegisterToAll<UInt32>();
        RegisterToAll<Int64>();
        RegisterToAll<UInt64>();
#if NET8_0_OR_GREATER
        RegisterToAll<UInt128>();
        RegisterToAll<Int128>();
#endif
        RegisterToAll<Char>();
        RegisterToAll<Single>();
        RegisterToAll<Double>();
        RegisterToAll<Decimal>();
        RegisterToAll<Boolean>();
        RegisterToAll<IntPtr>();
        RegisterToAll<UIntPtr>();
#if NET8_0_OR_GREATER
        RegisterToAll<Rune>();
#endif
        RegisterToAll<DateTime>();
        RegisterToAll<DateTimeOffset>();
        RegisterToAll<TimeSpan>();
        RegisterToAll<Guid>();
#if NET8_0_OR_GREATER
        RegisterToAll<DateOnly>();
        RegisterToAll<TimeOnly>();
#endif
        RegisterToAll<Complex>();
        RegisterToAll<Matrix3x2>();
        RegisterToAll<Matrix4x4>();
        RegisterToAll<Plane>();
        RegisterToAll<Quaternion>();
        RegisterToAll<Vector2>();
        RegisterToAll<Vector3>();
        RegisterToAll<Vector4>();
    }

    private static void RegisterToAll<T>() where T : unmanaged
    {
        Register(new UnmanagedFormatter<T>());
        Register(new NullableFormatter<T>());
        Register(new ArrayFormatter<T>());
        Register(new ArraySegmentFormatter<T>());
        Register(new MemoryFormatter<T>());
        Register(new ReadOnlyMemoryFormatter<T>());
    }
}
