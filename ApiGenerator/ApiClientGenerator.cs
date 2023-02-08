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
using System.IO;
using System.Linq;

namespace ApiGenerator;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ApiClientGenerator : DiagnosticAnalyzer
{
    private List<ClientGeneratorBase> clientGenerators = new List<ClientGeneratorBase>();

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        DiagnosticDescriptors.NoControllersDetected,
        DiagnosticDescriptors.NoClientGenerated);

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

        var projectName = context.Compilation.AssemblyName;
        var csprojFilePath = PackageUtilities.GetApiProjectName(context.Compilation);
        var projectInformation = XmlUtilities.ParseClientProjectFilePackageReferences(csprojFilePath);
        var globalNamespaces = GetNamespaces(context.Compilation.GlobalNamespace).ToList();

        foreach (var assemblyName in projectInformation.PackageReferences)
        {
            var namespaces = globalNamespaces.Where(x => x.StartsWith(assemblyName.PackageName));

            if (namespaces.Count() != 0)
            {
                assemblyName.Namespaces = namespaces;
            }
        }

        var projectAssemblyNamespaces = globalNamespaces.Where(x => x.StartsWith(projectName));
        Configuration.ProjectAssemblyNamespaces = projectAssemblyNamespaces;

        // in the future this could be done via config, e.g. whether to add a typescript client as well
        this.clientGenerators.Add(new CSharpClientGenerator(projectName));

        var completeControllerDetailList = this.ExtractControllerClients(context);

        if (completeControllerDetailList.Count == 0)
        {
            DiagnosticReporter.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.NoControllersDetected, Location.None));
            return;
        }

        // TODO testing
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

        var version = projectInformation.Version;

        if (Configuration.UseGitVersionInformation)
        {
            var gitVersionInformation = PackageUtilities.GetProjectVersionInformation(Path.GetDirectoryName(csprojFilePath));

            if (!string.IsNullOrWhiteSpace(gitVersionInformation))
            {
                version = gitVersionInformation;
            }
        }

        var allAdditionalUsings = completeControllerDetailList.Select(x => x.AdditionalUsings).Aggregate((a, b) => a.Union(b).ToList());
        var finalReferences = new List<PackageDetails>();

        foreach (var packageDetail in projectInformation.PackageReferences)
        {
            if (Configuration.ConfiguredPackageReferences.Contains(packageDetail.PackageName))
            {
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