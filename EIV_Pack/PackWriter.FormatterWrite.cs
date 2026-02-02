using EIV_Pack.Formatters;
using System.Runtime.CompilerServices;

namespace EIV_Pack;

public ref partial struct PackWriter : IDisposable
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WritePackable<T>(scoped in T? value)
        where T : IPackable<T>
    {
        depth++;
        if (depth == DepthLimit)
            PackException.ThrowReachedDepthLimit(typeof(T));

        T.SerializePackable(ref this, ref Unsafe.AsRef(in value));
        depth--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteValue<T>(scoped in T? value)
    {
        depth++;
        if (depth == DepthLimit)
            PackException.ThrowReachedDepthLimit(typeof(T));

        var formatter = FormatterProvider.GetFormatter<T>();
        formatter.Serialize(ref this, ref Unsafe.AsRef(in value));
        depth--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteValueWithFormatter<TFormatter, T>(TFormatter formatter, scoped in T? value)
        where TFormatter : IFormatter<T>
    {
        depth++;
        formatter.Serialize(ref this, ref Unsafe.AsRef(in value));
        depth--;
    }

}
