using ApiGenerator.Diagnostics;
using ApiGenerator.Packaging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;

namespace ApiClientGenerator.Configuration;

public class Configuration
{
    private static readonly Dictionary<string, bool> buildPropertiesWithBooleanDefaultValue = new Dictionary<string, bool>()
    {
        { "build_property.ACGT_GenerateClientOnBuild", true },
        { "build_property.ACGT_UseExternalAssemblyContracts", false },
        { "build_property.ACGT_UsePartialClientClasses", true },
        { "build_property.ACGT_UseInterfacesForClients", true },
        { "build_property.ACGT_UseSeparateClientFiles", false },
        { "build_property.ACGT_CreateClientProjectFileOnBuild", true },
        { "build_property.ACGT_CreateNugetPackageOnBuild", false },
        { "build_property.ACGT_UseGitVersionInformation", false },
        { "build_property.ACGT_GenerateMinimalApiClient", true },
    };

    public static IEnumerable<string> ProjectAssemblyNamespaces { get; set; } = new List<string>();
    public static string OutputPath = string.Empty;
    public static string ProjectDirectory = string.Empty;
    public static List<PackageDetails> ConfiguredPackageReferences { get; set; } = new List<PackageDetails>();

    public static bool GenerateClientOnBuild { get; set; } = true;
    public static bool UseExternalAssemblyContracts { get; set; } = false;
    public static bool UseSeparateClientFiles { get; set; } = false;
    public static bool UseInterfacesForClients { get; set; } = true;
    public static bool UsePartialClientClasses { get; set; } = true;
    public static bool CreateNugetPackageOnBuild { get; set; } = false;
    public static bool CreateClientProjectFileOnBuild { get; set; } = true;
    public static bool UseGitVersionInformation { get; set; } = true;
    public static bool GenerateMinimalApiClient { get; set; } = true;

    public static void ParseConfiguration(AnalyzerConfigOptions globalOptions)
    {
        ProjectAssemblyNamespaces = new List<string>();

        foreach (var item in new Dictionary<string, bool>(buildPropertiesWithBooleanDefaultValue))
        {
            if (globalOptions.TryGetValue(item.Key, out string value))
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                if (value != "false" && value != "true")
                {
                    DiagnosticReporter.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.GenericWarning, Location.None, $"Configuration value {item.Key} has the wrong format, so it is ignored.", "Use 'true' or 'false'."));
                    continue;
                }

                buildPropertiesWithBooleanDefaultValue[item.Key] = value.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
        }

        if (globalOptions.TryGetValue("build_property.projectdir", out string projectDirectory))
        {
            ProjectDirectory = projectDirectory;
        }

        if (globalOptions.TryGetValue("build_property.ACGT_OutputPath", out string outputPath))
        {
            OutputPath = ParseOutputPath(outputPath);
        }

        if (globalOptions.TryGetValue("build_property.ACGT_PackageReferences", out string packageReferences))
        {
            ParsePackageReferences(packageReferences);
        }

        SetFinalConfigurationValues();
    }


    private static string ParseOutputPath(string outputPath)
    {
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            return string.Empty;
        }

        if (Path.IsPathRooted(outputPath))
        {
            return outputPath;
        }

        string combinedPath = Path.Combine(ProjectDirectory, outputPath);
        return Path.GetFullPath(combinedPath);
    }

    private static void ParsePackageReferences(string packageReferences)
    {
        var packages = packageReferences.Split(',');

        foreach (var package in packages)
        {
            if (!string.IsNullOrWhiteSpace(package))
            {
                var packageSplit = package.Split(':');

                if (packageSplit.Length > 2)
                {
                    DiagnosticReporter.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.GenericWarning, Location.None, "Format of 'Package:Version' was used incorrectly: ", packageSplit));
                    throw new ArgumentException("Format of 'Package:Version' in property ACGT_PackageReferences was used incorrectly");
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

    private static void SetFinalConfigurationValues()
    {
        GenerateClientOnBuild = buildPropertiesWithBooleanDefaultValue["build_property.ACGT_GenerateClientOnBuild"];
        UseExternalAssemblyContracts = buildPropertiesWithBooleanDefaultValue["build_property.ACGT_UseExternalAssemblyContracts"];
        UseSeparateClientFiles = buildPropertiesWithBooleanDefaultValue["build_property.ACGT_UseSeparateClientFiles"];
        UseInterfacesForClients = buildPropertiesWithBooleanDefaultValue["build_property.ACGT_UseInterfacesForClients"];
        UsePartialClientClasses = buildPropertiesWithBooleanDefaultValue["build_property.ACGT_UsePartialClientClasses"];
        UseGitVersionInformation = buildPropertiesWithBooleanDefaultValue["build_property.ACGT_UseGitVersionInformation"];
        CreateNugetPackageOnBuild = buildPropertiesWithBooleanDefaultValue["build_property.ACGT_CreateNugetPackageOnBuild"];
        CreateClientProjectFileOnBuild = buildPropertiesWithBooleanDefaultValue["build_property.ACGT_CreateClientProjectFileOnBuild"];
        GenerateMinimalApiClient = buildPropertiesWithBooleanDefaultValue["build_property.ACGT_GenerateMinimalApiClient"];

        if (CreateNugetPackageOnBuild)
        {
            CreateClientProjectFileOnBuild = true;
        }
    }
}
