using Microsoft.CodeAnalysis;
using System;

namespace ApiGenerator.Diagnostics;

public static class DiagnosticReporter
{
    public static Action<Diagnostic> ReportDiagnostic { get; set; }
}
