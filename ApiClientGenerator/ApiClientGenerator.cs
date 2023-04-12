using ApiClientGenerator.Configuration;
using ApiGenerator.ClientGenerators;
using ApiGenerator.Diagnostics;
using ApiGenerator.Models;
using ApiGenerator.Packaging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ApiGenerator;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ApiClientGenerator : DiagnosticAnalyzer
{
    private List<ClientGeneratorBase> clientGenerators = new List<ClientGeneratorBase>();

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        DiagnosticDescriptors.GenericWarning,
        DiagnosticDescriptors.NoSyntaxTreesFound,
        DiagnosticDescriptors.NoControllersDetected,
        DiagnosticDescriptors.NoClientGenerated,
        DiagnosticDescriptors.NuGetGenerationFailed,
        DiagnosticDescriptors.PackageVersionNotFound,
        DiagnosticDescriptors.AttributeMissing);

    public override void Initialize(AnalysisContext context)
    {
        #if DEBUG
        if (Debugger.IsAttached)
        {
            Debugger.Launch();
        }
        #endif

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationAction(Execute);
    }

    public void Execute(CompilationAnalysisContext context)
    {
        DiagnosticReporter.ReportDiagnostic = context.ReportDiagnostic;

        if (context.Compilation.SyntaxTrees.Count() == 0)
        {
            DiagnosticReporter.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.NoSyntaxTreesFound, Location.None));
            return;
        }

        Configuration.ParseConfiguration(context.Options.AnalyzerConfigOptionsProvider.GlobalOptions);

        if (!Configuration.GenerateClientOnBuild)
        {
            DiagnosticReporter.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.NoClientGenerated, Location.None));
            return;
        }

        var csprojFilePath = Configuration.ProjectDirectory != string.Empty ? PackageUtilities.FindProjectFilePath(Configuration.ProjectDirectory) : PackageUtilities.GetApiProjectFilePath(context.Compilation);
        var projectDetails = XmlUtilities.ParseClientProjectFilePackageReferences(csprojFilePath);
        var globalNamespaces = this.GetNamespaces(context.Compilation.GlobalNamespace).ToList();
        this.MapNamespacesToPackages(projectDetails, globalNamespaces);
        var projectName = context.Compilation.AssemblyName;
        Configuration.ProjectAssemblyNamespaces = globalNamespaces.Where(x => x.StartsWith(projectName));

        // in the future this could be done via config, e.g. whether to add a typescript client as well
        this.clientGenerators.Add(new CSharpClientGenerator(projectName));

        var completeControllerDetailList = this.ExtractControllerClients(context);

        if (completeControllerDetailList.Count == 0)
        {
            DiagnosticReporter.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.NoControllersDetected, Location.None));
            return;
        }

        var fileDirectory = Configuration.OutputPath;

        if (fileDirectory == string.Empty)
        {
            fileDirectory = $"{Configuration.ProjectDirectory}\\out";
        }

        if (!Directory.Exists(fileDirectory))
        {
            Directory.CreateDirectory(fileDirectory);
        }

        foreach (var clientGenerator in this.clientGenerators)
        {
            if (Configuration.UseSeparateClientFiles)
            {
                foreach (var controllerDetail in completeControllerDetailList)
                {
                    clientGenerator.GenerateClient(controllerDetail, fileDirectory);
                }
            }
            else
            {
                clientGenerator.GenerateClient(completeControllerDetailList, fileDirectory);
            }
        }

        var version = this.GetPackageVersion(projectDetails, csprojFilePath);
        var allAdditionalUsings = completeControllerDetailList.Select(x => x.AdditionalUsings).Aggregate((a, b) => a.Union(b).ToList());
        var finalReferences = this.CompileFinalPackageReferences(projectDetails, allAdditionalUsings);

        if (Configuration.CreateClientProjectFileOnBuild)
        {
            XmlUtilities.CreateProjectFile(finalReferences, fileDirectory, $"{projectName}.csproj", version);

            if (Configuration.CreateNugetPackageOnBuild)
            {
                PackageUtilities.CreateNugetPackage(fileDirectory);
            }
        }
    }

    private List<ControllerClientDetails> ExtractControllerClients(CompilationAnalysisContext context)
    {
        var controllerClientBuilder = new ControllerClientBuilder(); 
        var minimalApiClient = new ControllerClientDetails("MinimalApi", null, true);
        var completeControllerDetailList = new List<ControllerClientDetails>();

        foreach (var tree in context.Compilation.SyntaxTrees)
        {
#pragma warning disable RS1030 // Do not invoke Compilation.GetSemanticModel() method within a diagnostic analyzer
            var semanticModel = context.Compilation.GetSemanticModel(tree);
#pragma warning restore RS1030 // Do not invoke Compilation.GetSemanticModel() method within a diagnostic analyzer

            if (Configuration.GenerateMinimalApiClient)
            {
                controllerClientBuilder.AddMinimalApis(tree, semanticModel, minimalApiClient);
            }

            var classNodes = semanticModel.SyntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>();

            // usually there should only one single controller be present in a file (possible diagnostic warning)
            var controllerClients = controllerClientBuilder.GetControllerClientDetails(classNodes, semanticModel);

            foreach (var controllerClient in controllerClients)
            {
                // dont add a partial controller again, find a better way to do this
                if (completeControllerDetailList.SingleOrDefault(x => x.ClientName == controllerClient.ClientName) is null)
                {
                    if (controllerClient.Endpoints.Any())
                    {
                        completeControllerDetailList.Add(controllerClient);
                    }
                }
            }
        }

        if (minimalApiClient.Endpoints.Any())
        {
            completeControllerDetailList.Add(minimalApiClient);
        }

        return completeControllerDetailList;
    }

    private string GetPackageVersion(ProjectDetails projectInformation, string csprojFilePath)
    {
        var version = projectInformation.Version;

        if (Configuration.UseGitVersionInformation)
        {
            var gitVersionInformation = PackageUtilities.GetProjectVersionInformation(Path.GetDirectoryName(csprojFilePath));

            if (!string.IsNullOrWhiteSpace(gitVersionInformation))
            {
                version = gitVersionInformation;
            }
        }

        return version;
    }

    private void MapNamespacesToPackages(ProjectDetails projectDetails, List<string> globalNamespaces)
    {
        foreach (var assemblyName in projectDetails.PackageReferences)
        {
            var namespaces = globalNamespaces.Where(x => x.StartsWith(assemblyName.PackageName));

            if (namespaces.Count() != 0)
            {
                assemblyName.Namespaces = namespaces;
            }
        }
    }



    private List<PackageDetails> CompileFinalPackageReferences(ProjectDetails projectDetails, List<string> allAdditionalUsings)
    {
        var finalReferences = new List<PackageDetails>();

        foreach (var packageDetail in projectDetails.PackageReferences)
        {
            var configuredPackage = Configuration.ConfiguredPackageReferences.FirstOrDefault(configuredPackage => configuredPackage.PackageName == packageDetail.PackageName);

            if (configuredPackage is not null)
            {
                if (configuredPackage.VersionInfo != string.Empty)
                {
                    packageDetail.VersionInfo = configuredPackage.VersionInfo;
                }

                if (!finalReferences.Contains(packageDetail))
                {
                    finalReferences.Add(packageDetail);
                    Configuration.ConfiguredPackageReferences.Remove(configuredPackage);
                }
            }

            foreach (var singleUsing in allAdditionalUsings)
            {
                if (packageDetail.Namespaces.Contains(singleUsing))
                {
                    if (!finalReferences.Contains(packageDetail))
                    {
                        finalReferences.Add(packageDetail);
                    }

                }
            }
        }

        foreach (var configuredPackage in Configuration.ConfiguredPackageReferences)
        {
            if (configuredPackage.VersionInfo == string.Empty)
            {
                DiagnosticReporter.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.PackageVersionNotFound, Location.None, configuredPackage.PackageName));
            }
        }

        return finalReferences;
    }

    private IEnumerable<string> GetNamespaces(INamespaceSymbol namespaceSymbol)
    {
        yield return namespaceSymbol.ToString();

        foreach (var childNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            foreach (var name in GetNamespaces(childNamespace))
            {
                yield return name;
            }
        }
    }
}