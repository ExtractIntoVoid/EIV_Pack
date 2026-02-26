#if NET8_0_OR_GREATER
namespace EIV_Pack.Formatters;

public sealed class EIV_Formatter<T> : BaseFormatter<T> where T : IPackable<T>
{
    public override void Deserialize(ref PackReader reader, scoped ref T? value)
    {
        T.DeserializePackable(ref reader, ref value);
    }

    public override void Serialize(ref PackWriter writer, scoped ref readonly T? value)
    {
        T.SerializePackable(ref writer, in value);
    }
}
#endif