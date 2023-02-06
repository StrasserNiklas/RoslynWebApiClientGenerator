using Microsoft.CodeAnalysis;

namespace ApiGenerator;

public class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor NoControllersDetected = new(id: "APIGEN001",
                                                                                              title: "No controllers were detected",
                                                                                              messageFormat: "There was no client generated because there were no controllers detected in the API project",
                                                                                              category: "ApiClientGenerator",
                                                                                              DiagnosticSeverity.Error,
                                                                                              isEnabledByDefault: true);
}
