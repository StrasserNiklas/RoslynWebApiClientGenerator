﻿using ApiGenerator.ClientGenerators;
using ApiGenerator.Diagnostics;
using ApiGenerator.Models;
using ApiGenerator.Packaging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace ApiGenerator;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ApiClientGenerator : DiagnosticAnalyzer
{
    private List<ClientGeneratorBase> clientGenerators = new List<ClientGeneratorBase>();

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        DiagnosticDescriptors.NoControllersDetected,
        DiagnosticDescriptors.NoClientGenerated,
        DiagnosticDescriptors.NuGetGenerationFailed,
        DiagnosticDescriptors.NoSyntaxTreesFound,
        DiagnosticDescriptors.GenericWarning);

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

        var csprojFilePath = PackageUtilities.GetApiProjectName(context.Compilation);
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

        // TODO testing
        // add this to config?
        var fileDirectory = "C:\\Workspace\\TestingApp\\TestingApp\\Test\\";

        var outDirectory = PackageUtilities.FindProjectFileDirectory(context.Compilation.SyntaxTrees.First().FilePath) + "\\out";

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

        XmlUtilities.CreateProjectFile(finalReferences, fileDirectory, $"{projectName}.csproj", version);

        if (Configuration.CreateNugetPackageOnBuild)
        {
            PackageUtilities.CreateNugetPackage(fileDirectory);
        }
    }

    private List<ControllerClientDetails> ExtractControllerClients(CompilationAnalysisContext context)
    {
        var controllerClientBuilder = new ControllerClientBuilder();
        var completeControllerDetailList = new List<ControllerClientDetails>();

        foreach (var tree in context.Compilation.SyntaxTrees)
        {
            var semanticModel = context.Compilation.GetSemanticModel(tree);

            // check for minimal APIs
            // TODO not added right now, fix it
            controllerClientBuilder.AddMinimalApis(tree, semanticModel, completeControllerDetailList);

            var classNodes = semanticModel.SyntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>();

            // usually there should only one single controller be present in a file (possible diagnostic warning)
            var controllerClients = controllerClientBuilder.GetControllerClientDetails(classNodes, semanticModel);

            foreach (var controllerClient in controllerClients)
            {
                // dont add a partial controller again, find a better way to do this
                if (completeControllerDetailList.SingleOrDefault(x => x.Name == controllerClient.Name) is null)
                {
                    completeControllerDetailList.Add(controllerClient);
                }
            }

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
            var configuredPackage = Configuration.ConfiguredPackageReferences.FirstOrDefault(x => x.PackageName.Contains(packageDetail.PackageName));

            if (configuredPackage is not null)
            {
                if (configuredPackage.VersionInfo != "latest")
                {
                    packageDetail.VersionInfo = configuredPackage.VersionInfo;
                }

                if (!finalReferences.Contains(packageDetail))
                {
                    finalReferences.Add(packageDetail);
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


    public override void Initialize(AnalysisContext context)
    {
        //Debugger.Launch();

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationAction(Execute);
    }
}