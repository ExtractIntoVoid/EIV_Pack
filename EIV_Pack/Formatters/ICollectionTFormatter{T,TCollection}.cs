using System.Collections;

namespace EIV_Pack.Formatters;

/// <summary>
/// A formatter for <see cref="ICollection{T}"/>.
/// </summary>
/// <typeparam name="T">Any type.</typeparam>
/// <typeparam name="TCollection">Any <see cref="ICollection"/> type.</typeparam>
public abstract class ICollectionTFormatter<T, TCollection> : BaseFormatter<TCollection>
    where TCollection : ICollection<T?>?, new()
{
    /// <summary>
    /// Creates a new collection with a lenght of <paramref name="length"/>.
    /// </summary>
    /// <param name="length">The new length.</param>
    /// <returns>The new collection.</returns>
    public virtual TCollection CreateCollection(int length)
    {
        return new();
    }

    /// <inheritdoc />
    public override void Deserialize(ref PackReader reader, scoped ref TCollection? value)
    {
        if (!reader.TryReadHeader(out int len) || len == Constants.NullHeader)
        {
            value = default;
            return;
        }

        if (value == null)
        {
            value = CreateCollection(len)!;
        }
        else
        {
            value.Clear();
        }

        IFormatter<T?> formatter = FormatterProvider.GetFormatter<T?>();
        for (int i = 0; i < len; i++)
        {
            T? val = default;
            formatter.Deserialize(ref reader, ref val);
            value.Add(val);
        }
    }

    /// <inheritdoc />
    public override void Serialize(ref PackWriter writer, scoped ref readonly TCollection? value)
    {
        if (value == null)
        {
            writer.WriteHeader();
            return;
        }

        writer.WriteHeader(value.Count);
        IFormatter<T?> formatter = FormatterProvider.GetFormatter<T?>();
        foreach (var item in value)
        {
            var val = item;
            formatter.Serialize(ref writer, ref val);
        }
    }
}
