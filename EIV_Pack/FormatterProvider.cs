using EIV_Pack.Formatters;
using System.Numerics;
using System.Text;

namespace EIV_Pack;

/// <summary>
/// Provide access to cached <see cref="IFormatter{T}"/>.
/// </summary>
public static class FormatterProvider
{
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
    }

    /// <summary>
    /// Registers a <typeparamref name="T"/> into type cache.
    /// </summary>
    /// <typeparam name="T">Type to register.</typeparam>
    public static void Register<T>() where T : IFormatterRegister
    {
        T.RegisterFormatter();
    }

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
        RegisterToAll<UInt128>();
        RegisterToAll<Int128>();
        RegisterToAll<Char>();
        RegisterToAll<Single>();
        RegisterToAll<Double>();
        RegisterToAll<Decimal>();
        RegisterToAll<Boolean>();
        RegisterToAll<IntPtr>();
        RegisterToAll<UIntPtr>();
        RegisterToAll<Rune>();
        RegisterToAll<DateTime>();
        RegisterToAll<DateTimeOffset>();
        RegisterToAll<TimeSpan>();
        RegisterToAll<Guid>();
        RegisterToAll<DateOnly>();
        RegisterToAll<TimeOnly>();
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
