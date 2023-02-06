using ApiGenerator.ClientGenerators;
using ApiGenerator.Extensions;
using ApiGenerator.Models;
using ApiGenerator.Packaging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ApiGenerator;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ApiClientGenerator : DiagnosticAnalyzer
{
    private List<ClientGeneratorBase> clientGenerators = new List<ClientGeneratorBase>();

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticDescriptors.NoControllersDetected);

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

    public void Execute(CompilationAnalysisContext context)
    {
        //var x = context.Compilation.DirectiveReferences;
        //var y = context.Compilation.GetUsedAssemblyReferences();
        //var z = context.Compilation.ReferencedAssemblyNames;
        //var a = context.Compilation.References;
        //var b = context.Compilation.SourceModule;
        //var c = context.Compilation.Assembly.NamespaceNames;


        Configuration.ParseConfiguration(context.Options.AnalyzerConfigOptionsProvider.GlobalOptions);

        if (!Configuration.GenerateClientOnBuild)
        {
            return;
        }

        //context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("12345", "title", "messageformat", "category", DiagnosticSeverity.Error, true), Location.None, DiagnosticSeverity.Error));
        var completeControllerDetailList = new List<ControllerClientDetails>();
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
        var controllerClientBuilder = new ControllerClientBuilder();

        // in the future this could be done via config, e.g. whether to add a typescript client as well
        this.clientGenerators.Add(new CSharpClientGenerator(projectName));

        foreach (var tree in context.Compilation.SyntaxTrees)
        {
            var semanticModel = context.Compilation.GetSemanticModel(tree);
            //emanticModel.get
            var lol = semanticModel.LookupNamespacesAndTypes(0);

            var hmm = tree.GetRoot().DescendantNodes().OfType<NamespaceDeclarationSyntax>().ToList();

            // check for minimal APIs
            // TODO not added right now, fix it
            controllerClientBuilder.AddMinimalApis(tree, semanticModel, completeControllerDetailList);

            // TODO right now is always true because the metadata is there...
            // maybe just remove it altogether
            if (!semanticModel.ContainsControllerTypes())
            {
                continue;
            }

            var classNodes = semanticModel.SyntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>();

            // usually there should only one single controller be present in a file (possible diagnostic warning)
            var controllerClients = controllerClientBuilder.GetControllerClientDetails(classNodes, semanticModel);

            foreach (var controllerClient in controllerClients)
            {
                // TODO quick fix for partial controllers
                if (completeControllerDetailList.SingleOrDefault(x => x.Name == controllerClient.Name) is null)
                {
                    completeControllerDetailList.Add(controllerClient);
                }
            }

        }

        if (completeControllerDetailList.Count == 0 || context.Compilation.SyntaxTrees.Count() == 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.NoControllersDetected, Location.None));
            return;
        }


        // TODO filepath?
        var fileDirectory = "C:\\Workspace\\TestingApp\\TestingApp\\Test\\";
        //var fileDirectory = "C:\\Masterarbeit\\testProjFolder\\NugetTest";

        // TODO put this in final versions
        var outDirectory = PackageUtilities.FindProjectFileDirectory(context.Compilation.SyntaxTrees.First().FilePath);

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

        var allUsings = completeControllerDetailList.Select(x => x.AdditionalUsings).Aggregate((a, b) => a.Union(b).ToList());
        var finalReferences = new List<PackageDetails>();

        foreach (var packageDetail in projectInformation.PackageReferences)
        {
            foreach (var singleUsing in allUsings)
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


    public override void Initialize(AnalysisContext context)
    {
        // enable this to allow for local debugging
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationAction(Execute);
    }
}