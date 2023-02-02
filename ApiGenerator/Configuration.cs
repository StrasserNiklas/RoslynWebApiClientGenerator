using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;

namespace ApiGenerator;

public class Configuration
{
    private static readonly Dictionary<string, bool> buildPropertiesWithBooleanDefaultValue = new Dictionary<string, bool>()
    {
        { "build_property.ApiClientGenerator_UsePartialClientClasses", true },
        { "build_property.ApiClientGenerator_UseInterfacesForClients", true },
        { "build_property.ApiClientGenerator_SeparateClientFiles", false },
        { "build_property.ApiClientGenerator_CreateNugetPackageOnBuild", true },
        { "build_property.ApiClientGenerator_UseGitVersionInformation", true },
    };

    public bool SeparateClientFiles { get; set; } = false;

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
            SeparateClientFiles = buildPropertiesWithBooleanDefaultValue["build_property.ApiClientGenerator_SeparateClientFiles"],
            UseInterfacesForClients = buildPropertiesWithBooleanDefaultValue["build_property.ApiClientGenerator_UseInterfacesForClients"],
            UsePartialClientClasses = buildPropertiesWithBooleanDefaultValue["build_property.ApiClientGenerator_UsePartialClientClasses"],
            UseGitVersionInformation = buildPropertiesWithBooleanDefaultValue["build_property.ApiClientGenerator_UseGitVersionInformation"],
            CreateNugetPackageOnBuild = buildPropertiesWithBooleanDefaultValue["build_property.ApiClientGenerator_CreateNugetPackageOnBuild"]
        };
    }
}
