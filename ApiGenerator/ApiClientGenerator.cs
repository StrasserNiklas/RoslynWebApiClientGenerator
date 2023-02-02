using ApiGenerator.Extensions;
using ApiGenerator.ClientGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using ApiGenerator.Models;
using System.IO;
using System;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ApiGenerator;

[Generator]
public class ApiClientGenerator : ISourceGenerator
{
    private List<ClientGeneratorBase> clientGenerators = new List<ClientGeneratorBase>();
    private IDictionary<string, HttpMethod> minimalApiMethods = new Dictionary<string, HttpMethod>()
    { 
        { "MapGet", HttpMethod.Get} , { "MapPost" , HttpMethod.Post }, { "MapPut" , HttpMethod.Put }, { "MapDelete" , HttpMethod.Delete } 
    };//, "Map", "MapWhen" }; MapMethods

    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif


    }

    public void Execute(GeneratorExecutionContext context)
    {
        //context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("12345", "title", "messageformat", "category", DiagnosticSeverity.Error, true), Location.None, DiagnosticSeverity.Error));

        var filePath = context.Compilation.ExternalReferences[0].Display;

        #region OngoingReferenceSearch
        var x1 = context.Compilation.GetMetadataReference(context.Compilation.Assembly);
        var x2 = context.Compilation.GetUsedAssemblyReferences();
        //var x3 = context.Compilation.

        var assemblyReferences = context.Compilation.GetUsedAssemblyReferences().ToList();
        //.OfType<AssemblyMetadata>();
        #endregion
        var projectName = context.Compilation.AssemblyName;// this.GetProjectName(context.Compilation);

        var proj = PackageUtilities.GetApiProjectName(context.Compilation);

        var configuration = Configuration.ParseConfiguration(context.AnalyzerConfigOptions.GlobalOptions);

        // in the future this could be done via config, e.g. whether to add a typescript client as well
        this.clientGenerators.Add(new CSharpClientGenerator(configuration, projectName));

        var controllerClientBuilder = new ControllerClientBuilder();
        var completeControllerDetailList = new List<ControllerClientDetails>();

        foreach (var tree in context.Compilation.SyntaxTrees)
        {
            var semanticModel = context.Compilation.GetSemanticModel(tree);

            // check for minimal APIs
            // TODO remove this and make it everywhere
            if (tree.FilePath.Contains("Program.cs") || tree.FilePath.Contains("Startup.cs"))
            {
                var controllerClientDetails = new ControllerClientDetails("Simple", null, true);

                var root = tree.GetRoot();
                var methodInvocations = root
                    .DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Where(x => x.Expression is MemberAccessExpressionSyntax expression && this.minimalApiMethods.Keys.Contains(expression.Name.Identifier.Value));

                foreach (var invocation in methodInvocations)
                {
                    var symbolInfo = semanticModel.GetSymbolInfo(invocation);

                    // TODO if you really want to, you can try to find the first parameter/s (id and a object) of the second delegate parameter
                    if (symbolInfo.Symbol is not null && symbolInfo.Symbol is IMethodSymbol methodSymbol)
                    {
                        var parameters = methodSymbol.Parameters;

                        if (parameters.Count() != 2)
                        {
                            continue;
                        }
                    }

                    if (invocation.ArgumentList.Arguments.First().Expression is LiteralExpressionSyntax literal)
                    {
                        var route = literal.Token.ValueText;
                        this.minimalApiMethods.TryGetValue(((MemberAccessExpressionSyntax)invocation.Expression).Name.Identifier.Value as string, out var httpMethod);
                        var methodDetails = new ControllerMethodDetails(httpMethod, null, null, $"{httpMethod.Method}_{route.Replace("/", "")}", route);
                        controllerClientDetails.HttpMethods.Add(methodDetails);
                    }
                }

                // TODO before minimal APIs can be handled in the generation, you have to enable the option to declare the intended request and response type
                // probably a combination too or can we find this out from
                //  Task<TOut> GET_weatherforecastAsync<TIn, TOut>(TIn inn, CancellationToken cancellationToken)

                // TODO if it works, uncomment this line
                //completeControllerDetailList.Add(controllerClientDetails);
            }

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
}