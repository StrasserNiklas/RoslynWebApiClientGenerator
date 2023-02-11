using ApiGenerator.Diagnostics;
using ApiGenerator.Packaging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiGenerator;

public class Configuration
{
    private static readonly Dictionary<string, bool> buildPropertiesWithBooleanDefaultValue = new Dictionary<string, bool>()
    {
        { "build_property.ApiClientGenerator_GenerateClientOnBuild", true },
        { "build_property.ApiClientGenerator_UseExternalAssemblyContracts", true },
        { "build_property.ApiClientGenerator_UsePartialClientClasses", true },
        { "build_property.ApiClientGenerator_UseInterfacesForClients", true },
        { "build_property.ApiClientGenerator_UseSeparateClientFiles", false },
        { "build_property.ApiClientGenerator_CreateNugetPackageOnBuild", false },
        { "build_property.ApiClientGenerator_UseGitVersionInformation", false },
    };

    public static IEnumerable<string> ProjectAssemblyNamespaces { get; set; } = new List<string>();

    public static List<PackageDetails> ConfiguredPackageReferences { get; set; } = new List<PackageDetails>();

    public static bool GenerateClientOnBuild { get; set; } = true;
    public static bool UseExternalAssemblyContracts { get; set; } = true;
    public static bool UseSeparateClientFiles { get; set; } = false;
    public static bool UseInterfacesForClients { get; set; } = true;
    public static bool UsePartialClientClasses { get; set; } = true;
    public static bool CreateNugetPackageOnBuild { get; set; } = true;
    public static bool UseGitVersionInformation { get; set; } = true;

    public static void ParseConfiguration(AnalyzerConfigOptions globalOptions)
    {
        ProjectAssemblyNamespaces = new List<string>();

        foreach (var item in new Dictionary<string, bool>(buildPropertiesWithBooleanDefaultValue))
        {
            if (globalOptions.TryGetValue(item.Key, out var value))
            {
                buildPropertiesWithBooleanDefaultValue[item.Key] = value.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
        }

        if (globalOptions.TryGetValue("build_property.ApiClientGenerator_PackageReferences", out string packageReferences))
        {
            DiagnosticReporter.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.GenericWarning, Location.None, "References: ", packageReferences));

            var packages = packageReferences.Split(';');

            foreach (var package in packages)
            {
                if (!string.IsNullOrWhiteSpace(package))
                {
                    var packageSplit = package.Split(':');

                    if (packageSplit.Length > 2)
                    {
                        DiagnosticReporter.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.GenericWarning, Location.None, "Format of 'Package:Version' was used incorrectly: ", packageSplit));
                        throw new ArgumentException("Format of 'Package:Version' in property ApiClientGenerator_PackageReferences was used incorrectly");
                    }

                    if (packageSplit.Length == 1 && !string.IsNullOrWhiteSpace(packageSplit[0]))
                    {
                        ConfiguredPackageReferences.Add(new PackageDetails(packageSplit[0]));
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(packageSplit[0]) && !string.IsNullOrWhiteSpace(packageSplit[1]))
                    {
                        ConfiguredPackageReferences.Add(new PackageDetails(packageSplit[0], packageSplit[1]));
                    }
                }
            }
        }

        GenerateClientOnBuild = buildPropertiesWithBooleanDefaultValue["build_property.ApiClientGenerator_GenerateClientOnBuild"];
        UseExternalAssemblyContracts = buildPropertiesWithBooleanDefaultValue["build_property.ApiClientGenerator_UseExternalAssemblyContracts"];
        UseSeparateClientFiles = buildPropertiesWithBooleanDefaultValue["build_property.ApiClientGenerator_UseSeparateClientFiles"];
        UseInterfacesForClients = buildPropertiesWithBooleanDefaultValue["build_property.ApiClientGenerator_UseInterfacesForClients"];
        UsePartialClientClasses = buildPropertiesWithBooleanDefaultValue["build_property.ApiClientGenerator_UsePartialClientClasses"];
        UseGitVersionInformation = buildPropertiesWithBooleanDefaultValue["build_property.ApiClientGenerator_UseGitVersionInformation"];
        CreateNugetPackageOnBuild = buildPropertiesWithBooleanDefaultValue["build_property.ApiClientGenerator_CreateNugetPackageOnBuild"];
    }
}
