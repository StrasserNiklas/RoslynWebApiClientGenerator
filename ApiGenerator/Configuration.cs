using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;

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

    public static List<string> ConfiguredPackageReferences { get; set; } = new List<string>();

    public static bool GenerateClientOnBuild { get; set; } = true;
    public static bool UseExternalAssemblyContracts { get; set; } = true;
    public static bool UseSeparateClientFiles { get; set; } = false;
    public static bool UseInterfacesForClients { get; set; } = true;
    public static bool UsePartialClientClasses { get; set; } = true;
    public static bool CreateNugetPackageOnBuild { get; set; } = true;
    public static bool UseGitVersionInformation { get; set; } = true;

    public static void ParseConfiguration(AnalyzerConfigOptions globalOptions)
    {
        foreach (var item in new Dictionary<string, bool>(buildPropertiesWithBooleanDefaultValue))
        {
            if (globalOptions.TryGetValue(item.Key, out var value))
            {
                buildPropertiesWithBooleanDefaultValue[item.Key] = value.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
        }

        if (globalOptions.TryGetValue("build_property.ApiClientGenerator_PackageReferences", out string packageReferences))
        {
            var packages = packageReferences.Split(',');

            foreach (var package in packages)
            {
                if (!string.IsNullOrWhiteSpace(package))
                {
                    ConfiguredPackageReferences.Add(package.Trim());
                    continue;
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
