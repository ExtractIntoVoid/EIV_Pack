using EIV_Pack.Formatters;
using System.Runtime.CompilerServices;

namespace EIV_Pack;

public ref partial struct PackWriter : IDisposable
{
    /// <summary>
    /// Writes <see cref="IPackable{T}"/> value.
    /// </summary>
    /// <typeparam name="T">Any registered value.</typeparam>
    /// <param name="value">The value to write.</param>
    /// <exception cref="PackException">Throws when reaching <see cref="DepthLimit"/> or type not registered.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WritePackable<T>(scoped in T? value)
        where T : IPackable<T>
    {
        depth++;
        if (depth == DepthLimit)
            throw new PackException($"Serializing Type '{typeof(T).FullName}' reached depth limit, maybe detect circular reference.");

#if !NETSTANDARD2_0
        T.SerializePackable(ref this, ref Unsafe.AsRef(in value));
#else
        IFormatter<T> formatter = FormatterProvider.GetFormatter<T>();
        formatter.Serialize(ref this, ref Unsafe.AsRef(in value));
#endif

        depth--;
    }

    /// <summary>
    /// Writes <typeparamref name="T"/> value.
    /// </summary>
    /// <typeparam name="T">Any registered value.</typeparam>
    /// <param name="value">The value to write.</param>
    /// <exception cref="PackException">Throws when reaching <see cref="DepthLimit"/> or type not registered.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteValue<T>(scoped in T? value)
    {
        depth++;
        if (depth == DepthLimit)
            throw new PackException($"Serializing Type '{typeof(T).FullName}' reached depth limit, maybe detect circular reference.");

        IFormatter<T> formatter = FormatterProvider.GetFormatter<T>();
        formatter.Serialize(ref this, ref Unsafe.AsRef(in value));
        depth--;
    }

    /// <summary>
    /// Writes a value using the specified formatter.
    /// </summary>
    /// <typeparam name="TFormatter">The formatter used to serialize values of type T.</typeparam>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="formatter">The formatter to use for serialization.</param>
    /// <param name="value">The value to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteValueWithFormatter<TFormatter, T>(TFormatter formatter, scoped in T? value)
        where TFormatter : IFormatter<T>
    {
        depth++;
        formatter.Serialize(ref this, ref Unsafe.AsRef(in value));
        depth--;
    }

    /// <summary>
    /// Writes a value using the specified formatter.
    /// </summary>
    /// <param name="formatter">The formatter to use for serialization.</param>
    /// <param name="value">The value to write.</param>
    public void WriteValueWithFormatter(IFormatter formatter, scoped in object? value)
    {
        depth++;
        formatter.Serialize(ref this, ref Unsafe.AsRef(in value));
        depth--;
    }
}
