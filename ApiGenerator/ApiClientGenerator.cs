﻿using ApiGenerator.ClientGenerators;
using ApiGenerator.Extensions;
using ApiGenerator.Models;
using ApiGenerator.Packaging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ApiGenerator;

[Generator]
public class ApiClientGenerator : ISourceGenerator
{
    private List<ClientGeneratorBase> clientGenerators = new List<ClientGeneratorBase>();

    public void Execute(GeneratorExecutionContext context)
    {
        //context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("12345", "title", "messageformat", "category", DiagnosticSeverity.Error, true), Location.None, DiagnosticSeverity.Error));
        var controllerClientBuilder = new ControllerClientBuilder();
        var completeControllerDetailList = new List<ControllerClientDetails>();
        var projectName = context.Compilation.AssemblyName;
        var csprojFilePath = PackageUtilities.GetApiProjectName(context.Compilation);
        var configuration = Configuration.ParseConfiguration(context.AnalyzerConfigOptions.GlobalOptions);

        // in the future this could be done via config, e.g. whether to add a typescript client as well
        this.clientGenerators.Add(new CSharpClientGenerator(configuration, projectName));

        foreach (var tree in context.Compilation.SyntaxTrees)
        {
            var semanticModel = context.Compilation.GetSemanticModel(tree);

            // check for minimal APIs
            // TODO not added right now, fix it
            controllerClientBuilder.AddMinimalApis(tree, semanticModel, completeControllerDetailList);

            // TODO right now is always true because the metadata is there wtf...
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
            completeControllerDetailList.AddRange(controllerClients);
        }

        // TODO error when there are no clients (possible diagnostic warning)

        foreach (var clientGenerator in this.clientGenerators)
        {
            if (configuration.SeparateClientFiles)
            {
                foreach (var controllerDetail in completeControllerDetailList)
                {
                    clientGenerator.GenerateClient(controllerDetail);
                }
            }
            else
            {
                clientGenerator.GenerateClient(completeControllerDetailList);
            }
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif
    }
}