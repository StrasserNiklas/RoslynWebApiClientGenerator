using ApiGenerator.ClientGenerators;
using ApiGenerator.Extensions;
using ApiGenerator.Models;
using ApiGenerator.Packaging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
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

    private static readonly DiagnosticDescriptor FailureDescriptor = new DiagnosticDescriptor("META001", "CMS Meta Failure", "{0}",
            "Failure",
            DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "The CMS Meta Generation failed.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(FailureDescriptor);

    public void Execute(CompilationAnalysisContext context)
    {
        var configuration = Configuration.ParseConfiguration(context.Options.AnalyzerConfigOptionsProvider.GlobalOptions);

        if (!configuration.GenerateClientOnBuild)
        {
            return;
        }

        //context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("12345", "title", "messageformat", "category", DiagnosticSeverity.Error, true), Location.None, DiagnosticSeverity.Error));
        var controllerClientBuilder = new ControllerClientBuilder();
        var completeControllerDetailList = new List<ControllerClientDetails>();
        var projectName = context.Compilation.AssemblyName;

        // in the future this could be done via config, e.g. whether to add a typescript client as well
        this.clientGenerators.Add(new CSharpClientGenerator(configuration, projectName));

        foreach (var tree in context.Compilation.SyntaxTrees)
        {
            var semanticModel = context.Compilation.GetSemanticModel(tree);

            // check for minimal APIs
            // TODO not added right now, fix it
            controllerClientBuilder.AddMinimalApis(tree, semanticModel, completeControllerDetailList);

            // TODO right now is always true because the metadata is there wtf...
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

        // TODO error when there are no clients (possible diagnostic warning)

        
        var fileDirectory = "C:\\Masterarbeit\\testProjFolder\\NugetTest";

        foreach (var clientGenerator in this.clientGenerators)
        {
            if (configuration.UseSeparateClientFiles)
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

        var csprojFilePath = PackageUtilities.GetApiProjectName(context.Compilation);
        var projectInformation = XmlUtilities.ParseClientProjectFilePackageReferences(csprojFilePath);

        var version = projectInformation.Version;

        if (configuration.UseGitVersionInformation)
        {
            var gitVersionInformation = PackageUtilities.GetProjectVersionInformation(Path.GetDirectoryName(csprojFilePath));

            if (!string.IsNullOrWhiteSpace(gitVersionInformation))
            {
                version = gitVersionInformation;
            }
        }

        XmlUtilities.CreateProjectFile(projectInformation.PackageReferences, fileDirectory, $"{projectName}.csproj", version);

        if (configuration.CreateNugetPackageOnBuild)
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