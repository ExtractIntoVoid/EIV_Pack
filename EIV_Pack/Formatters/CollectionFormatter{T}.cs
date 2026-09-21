using System.Collections.ObjectModel;

namespace EIV_Pack.Formatters;

/// <summary>
/// A <see cref="Collection{T}"/> formatter.
/// </summary>
/// <typeparam name="T">Any type.</typeparam>
public class CollectionFormatter<T> : ICollectionTFormatter<T?, Collection<T?>>;
