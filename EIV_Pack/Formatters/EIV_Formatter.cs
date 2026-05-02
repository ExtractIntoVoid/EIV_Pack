#if !NETSTANDARD2_0
namespace EIV_Pack.Formatters;

/// <summary>
/// An <see cref="IPackable{T}"/> formatter.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class EIV_Formatter<T> : BaseFormatter<T> where T : IPackable<T>
{
    /// <inheritdoc />
    public override void Deserialize(ref PackReader reader, scoped ref T? value)
    {
        T.DeserializePackable(ref reader, ref value);
    }

    /// <inheritdoc />
    public override void Serialize(ref PackWriter writer, scoped ref readonly T? value)
    {
        T.SerializePackable(ref writer, in value);
    }
}
#endif