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

    public bool GenerateClientOnBuild { get; set; } = true;

    // TODO use this!
    public bool UseExternalAssemblyContracts { get; set; } = true;
    public bool UseSeparateClientFiles { get; set; } = false;
    public bool UseInterfacesForClients { get; set; } = true;
    public bool UsePartialClientClasses { get; set; } = true;
    public bool CreateNugetPackageOnBuild { get; set; } = true;
    public bool UseGitVersionInformation { get; set; } = true;

    public static Configuration ParseConfiguration(AnalyzerConfigOptions globalOptions)
    {
        foreach (var item in buildPropertiesWithBooleanDefaultValue)
        {
            if (globalOptions.TryGetValue(item.Key, out var value))
            {
                buildPropertiesWithBooleanDefaultValue[item.Key] = value.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
        }

        return new Configuration()
        {
            GenerateClientOnBuild = buildPropertiesWithBooleanDefaultValue["build_property.ApiClientGenerator_GenerateClientOnBuild"],
            UseExternalAssemblyContracts = buildPropertiesWithBooleanDefaultValue["build_property.ApiClientGenerator_UseExternalAssemblyContracts"],
            UseSeparateClientFiles = buildPropertiesWithBooleanDefaultValue["build_property.ApiClientGenerator_UseSeparateClientFiles"],
            UseInterfacesForClients = buildPropertiesWithBooleanDefaultValue["build_property.ApiClientGenerator_UseInterfacesForClients"],
            UsePartialClientClasses = buildPropertiesWithBooleanDefaultValue["build_property.ApiClientGenerator_UsePartialClientClasses"],
            UseGitVersionInformation = buildPropertiesWithBooleanDefaultValue["build_property.ApiClientGenerator_UseGitVersionInformation"],
            CreateNugetPackageOnBuild = buildPropertiesWithBooleanDefaultValue["build_property.ApiClientGenerator_CreateNugetPackageOnBuild"]
        };
    }
}
