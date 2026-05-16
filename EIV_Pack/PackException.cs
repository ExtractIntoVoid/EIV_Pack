#if !NETSTANDARD2_0
using System.Diagnostics.CodeAnalysis;
#endif

namespace EIV_Pack;

/// <summary>
/// An excepton for EIV Pack related errors.
/// </summary>
/// <param name="message"></param>
public class PackException(string message) : Exception(message);