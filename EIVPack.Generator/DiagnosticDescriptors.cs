using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace EIVPack.Generator;

internal static class DiagnosticDescriptors
{
    const string Category = "GenerateEIVPack";

    public static readonly DiagnosticDescriptor MustBePartial = new(
        id: "EIVACK001",
        title: "EIVPackable object must be partial",
        messageFormat: "The EIVPackable object '{0}' must be partial",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
