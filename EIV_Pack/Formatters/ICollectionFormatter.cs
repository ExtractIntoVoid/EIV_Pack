using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace EIV_Pack.Formatters;

/// <summary>
/// A formatter for <see cref="ICollection"/>.
/// </summary>
/// <typeparam name="T">Any type.</typeparam>
/// <typeparam name="TCollection">Any <see cref="ICollection"/> type.</typeparam>
public abstract class ICollectionFormatter<T, TCollection> : BaseFormatter<TCollection>
    where TCollection : ICollection?, new()
{
    /// <summary>
    /// Creates a new collection with a lenght of <paramref name="length"/>.
    /// </summary>
    /// <param name="length">The new length.</param>
    /// <returns>The new collection.</returns>
    public abstract TCollection CreateCollection(int length);

    /// <summary>
    /// Clears the <paramref name="collection"/> and check if <paramref name="length"/> item can be added.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="length">The length of to ensure items can fit.</param>
    public abstract void Clear(scoped ref TCollection collection, int length);

    /// <summary>
    /// Add <paramref name="value"/> to the <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="value">The item to add.</param>
    public abstract void Add(scoped ref TCollection collection, T? value);

    /// <summary>
    /// Gets the item from the <paramref name="index"/> position from the <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="index">The index to get the item.</param>
    /// <returns>The got item or <see langword="null"/>.</returns>
    public abstract T? GetValue(scoped ref readonly TCollection collection, int index);

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
            Clear(ref value, len);
        }

        IFormatter<T?> formatter = FormatterProvider.GetFormatter<T?>();
        for (int i = 0; i < len; i++)
        {
            T? val = default;
            formatter.Deserialize(ref reader, ref val);
            Add(ref value, val);
        }
    }

    /// <inheritdoc />
    public override void Serialize(ref PackWriter writer, scoped ref readonly TCollection? collection)
    {
        if (collection == null)
        {
            writer.WriteHeader();
            return;
        }

        writer.WriteHeader(collection.Count);
        IFormatter<T?> formatter = FormatterProvider.GetFormatter<T?>();
        for (int i = 0; i < collection.Count; i++)
        {
            T? val = GetValue(in collection, i);
            formatter.Serialize(ref writer, ref val);
        }
    }
}
