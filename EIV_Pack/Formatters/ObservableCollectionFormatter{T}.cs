using System.Collections.ObjectModel;

namespace EIV_Pack.Formatters;

/// <summary>
/// A <see cref="ObservableCollection{T}"/> formatter.
/// </summary>
/// <typeparam name="T">Any type.</typeparam>
public class ObservableCollectionFormatter<T> : ICollectionTFormatter<T?, ObservableCollection<T?>>;
