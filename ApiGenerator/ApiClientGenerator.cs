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

namespace ApiGenerator;

[Generator]
public class ApiClientGenerator : ISourceGenerator
{
    private List<IClientGenerator> clientGenerators = new List<IClientGenerator>();

    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif

        // in the future this could be done via config, e.g. whether to add a typescript client as well
        this.clientGenerators.Add(new CSharpClientGenerator());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        #region OngoingReferenceSearch
        var x1 = context.Compilation.GetMetadataReference(context.Compilation.Assembly);
        var x2 = context.Compilation.GetUsedAssemblyReferences();
        //var x3 = context.Compilation.

        var assemblyReferences = context.Compilation.GetUsedAssemblyReferences().ToList();
        //.OfType<AssemblyMetadata>();
        #endregion

        var controllerClientBuilder = new ControllerClientBuilder();
        var completeControllerDetailList = new List<ControllerClientDetails>();

        // TODO what about partial controller classes, must work!
        foreach (var tree in context.Compilation.SyntaxTrees)
        {
            var semanticModel = context.Compilation.GetSemanticModel(tree);

            #region OngoingReferenceSearchWIP
            var ref1 = semanticModel.Compilation.DirectiveReferences.ToList();
            var ref2 = semanticModel.Compilation.References.ToList();
            var ref3 = semanticModel.Compilation.ExternalReferences.ToList();
            var ref4 = semanticModel.Compilation.ReferencedAssemblyNames.ToList();
            var ref5 = semanticModel.Compilation.GetMetadataReference(semanticModel.Compilation.Assembly);

            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            // Use the syntax tree to find "using System;"
            UsingDirectiveSyntax usingSystem = root.Usings[0];
            NameSyntax systemName = usingSystem.Name;

            // Use the semantic model for symbol information:
            SymbolInfo nameInfo = semanticModel.GetSymbolInfo(systemName);

            var systemSymbol = (INamespaceSymbol)nameInfo.Symbol;
            foreach (INamespaceSymbol ns in systemSymbol.GetNamespaceMembers())
            {
                //Console.WriteLine(ns);
            }

            //INamespaceSymbol x;
            //x.ContainingAssembly;
            //semanticModel.SyntaxTree.GetRoot().

            #endregion

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

        // TODO config if each client in respective file (config in appsettings?)
        foreach (var clientGenerator in this.clientGenerators)
        {
            clientGenerator.GenerateClient(completeControllerDetailList);
        }
    }
}

public class ControllerClientBuilder
{
    public IEnumerable<ControllerClientDetails> GetControllerClientDetails(IEnumerable<ClassDeclarationSyntax> classNodes, SemanticModel semanticModel)
    {
        foreach (var classNode in classNodes)
        {
            var classSymbol = semanticModel.GetDeclaredSymbol(classNode);

            if (classSymbol is null)
            {
                continue;
            }

            if (classSymbol.IsApiController(semanticModel))
            {
                var routeAttribute = classSymbol.GetRouteAttribute();
                var clientInformation = new ControllerClientDetails(classSymbol.Name, routeAttribute);

                var methods = classSymbol.GetMembers();

                if (methods.Count() == 0)
                {
                    continue;
                }

                this.AddControllerMethods(methods, clientInformation);
                yield return clientInformation;
            }
        }
    }

    private void AddControllerMethods(IEnumerable<ISymbol> methods, ControllerClientDetails clientInformation)
    {
        // TODO make this whole method clearer by extracting methods...

        IDictionary<string, string> generatedClasses = new Dictionary<string, string>();

        foreach (var method in methods)
        {
            if (method is IMethodSymbol
                {
                    DeclaredAccessibility: Accessibility.Public,
                    MethodKind: MethodKind.Ordinary,
                    IsAbstract: false
                } methodSymbol)
            {
                var httpMethodAttribute = methodSymbol.GetAttribute("Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute");

                // TODO check if this is even there (public methods that do nothing maybe)

                var httpMethod = httpMethodAttribute?.AttributeClass?.Name switch
                {
                    "HttpGetAttribute" => HttpMethod.Get,
                    "HttpPutAttribute" => HttpMethod.Put,
                    "HttpPostAttribute" => HttpMethod.Post,
                    "HttpDeleteAttribute" => HttpMethod.Delete,
                    _ => HttpMethod.Get // TODO does this work / do we even want it
                };

                // yeah, if we even want this, see above
                // else continue here...

                var methodRoute = this.GetMethodRoute(methodSymbol, httpMethodAttribute);
                var methodNameWithoutAsnyc = methodSymbol.Name.RemoveSuffix("Async");
                var finalRoute = this.BuildFinalMethodRoute(methodNameWithoutAsnyc, clientInformation.BaseRoute, methodRoute);


                var methodParameters = methodSymbol.Parameters;

                foreach (var methodParameter in methodParameters)
                {
                    // filter out from services attribute
                    // Microsoft.AspNetCore.Mvc.FromServicesAttribute
                    var parameterAttributes = methodParameter.GetAttributes();
                }

                var returnType = methodSymbol.ReturnType;

                // Unwrap Task<T>
                if (returnType is INamedTypeSymbol taskType && taskType.OriginalDefinition.ToString() == "System.Threading.Tasks.Task<TResult>")
                {
                    returnType = taskType.TypeArguments.First();
                }

                if (returnType is INamedTypeSymbol actionResultType && actionResultType.OriginalDefinition.ToString() == "Microsoft.AspNetCore.Mvc.ActionResult<TValue>")
                {
                    returnType = actionResultType.TypeArguments.First();
                }

                if (returnType.OriginalDefinition.ToString() == "Microsoft.AspNetCore.Mvc.IActionResult" || returnType.OriginalDefinition.ToString() == "Microsoft.AspNetCore.Mvc.ActionResult")
                {
                    returnType = null;
                }

                var test = returnType.GenerateClassString();

                foreach (var item in test)
                {
                    if (!generatedClasses.ContainsKey(item.Key))
                    {
                        generatedClasses.Add(item.Key, item.Value);
                    }
                }

                var httpMethodInformation = new ControllerMethodDetails(httpMethod, methodNameWithoutAsnyc, finalRoute);
                clientInformation.HttpMethods.Add(httpMethodInformation);
            }
        }

        clientInformation.GeneratedCodeClasses = generatedClasses;
    }

    private string BuildFinalMethodRoute(string methodName, string baseRoute, string methodRoute)
    {
        if (baseRoute.Contains("[action]"))
        {
            return baseRoute.Replace("[action]", methodName);
        }

        return string.IsNullOrWhiteSpace(methodRoute) ? baseRoute : $"{baseRoute}/{methodRoute}";
    }

    private string GetMethodRoute(IMethodSymbol methodSymbol, AttributeData httpMethodAttribute)
    {
        var routeAttribute = methodSymbol.GetRouteAttribute();

        if (routeAttribute != null)
        {
            return routeAttribute.ConstructorArguments.FirstOrDefault().Value.ToString() ?? string.Empty;
        }

        // TODO think about if route name has any meaning 
        return httpMethodAttribute?.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? string.Empty;
    }
}
