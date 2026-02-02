using System.Diagnostics.CodeAnalysis;

namespace EIV_Pack.Formatters;

public sealed class LazyFormatter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T> : IFormatter<Lazy<T?>>
{
    public void Deserialize(ref PackReader reader, scoped ref Lazy<T?>? value)
    {
        if (!reader.TryReadSmallHeader(out byte count) || count == Constants.SmallNullHeader || count != 1)
        {
            value = null;
            return;
        }

        T? v = reader.ReadValue<T>();
        value = new Lazy<T?>(v);
    }

    public void Serialize(ref PackWriter writer, scoped ref readonly Lazy<T?>? value)
    {
        if (value == null)
        {
            writer.WriteSmallHeader();
            return;
        }

        writer.WriteSmallHeader(1);
        writer.WriteValue(value.Value);
    }
}
