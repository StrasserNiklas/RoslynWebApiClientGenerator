﻿using Microsoft.CodeAnalysis;

namespace ApiGenerator.Diagnostics;

public class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor GenericWarning = new(id: "APIGEN000",
                                                                                              title: "Warning",
                                                                                              messageFormat: "'{0} {1}",
                                                                                              category: "ApiClientGenerator",
                                                                                              DiagnosticSeverity.Warning,
                                                                                              isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NoSyntaxTreesFound = new(id: "APIGEN001",
                                                                                              title: "No syntax trees were found",
                                                                                              messageFormat: "There was no client generated because there were no syntax trees (files) found in the API project",
                                                                                              category: "ApiClientGenerator",
                                                                                              DiagnosticSeverity.Error,
                                                                                              isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NoControllersDetected = new(id: "APIGEN002",
                                                                                              title: "No controllers were detected",
                                                                                              messageFormat: "There was no client generated because there were no controllers detected in the API project",
                                                                                              category: "ApiClientGenerator",
                                                                                              DiagnosticSeverity.Error,
                                                                                              isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NoClientGenerated = new(id: "APIGEN003",
                                                                                              title: "No clients were detected",
                                                                                              messageFormat: "There was no client generated because it was turned of by configuration",
                                                                                              category: "ApiClientGenerator",
                                                                                              DiagnosticSeverity.Warning,
                                                                                              isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NuGetGenerationFailed = new(id: "APIGEN004",
                                                                                          title: "NuGet client could not be generated",
                                                                                          messageFormat: "NuGet client generation failed with error '{0}' ",
                                                                                          category: "ApiClientGenerator",
                                                                                          DiagnosticSeverity.Warning,
                                                                                          isEnabledByDefault: true);
}
