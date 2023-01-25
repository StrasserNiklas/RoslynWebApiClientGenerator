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
        var x1 = context.Compilation.GetMetadataReference(context.Compilation.Assembly);
        var x2 = context.Compilation.GetUsedAssemblyReferences();
        //var x3 = context.Compilation.

        var assemblyReferences = context.Compilation.GetUsedAssemblyReferences().ToList();
        //.OfType<AssemblyMetadata>();

        //var assemblyClasses = context.Compilation .SyntaxTrees.SelectMany(
        //    x => x.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>());

        var apiClients = new List<ControllerClientDetails>();

        foreach (var tree in context.Compilation.SyntaxTrees)
        {
            var semanticModel = context.Compilation.GetSemanticModel(tree);

            var ref1 = semanticModel.Compilation.DirectiveReferences.ToList();
            var ref2 = semanticModel.Compilation.References.ToList();
            var ref3 = semanticModel.Compilation.ExternalReferences.ToList();
            var ref4 = semanticModel.Compilation.ReferencedAssemblyNames.ToList();
            var ref5 = semanticModel.Compilation.GetMetadataReference(semanticModel.Compilation.Assembly);

            if (!semanticModel.ContainsControllerTypes())
            {
                // TODO return or yield break or continue
                //yield break;
                continue;
            }

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

            var classNodes = semanticModel.SyntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>();


            var apiClientList = this.GetApiClients(classNodes, semanticModel);

            var dictionaries = apiClientList.Select(client => client.GeneratedCodeClasses);

            var mergedDictionary = dictionaries.Aggregate((d1, d2) => d1.Concat(d2).ToDictionary(k => k.Key, v => v.Value));


            apiClients.AddRange(apiClientList);
        }

        var tek = apiClients.Where(client => client.ContainsHttpMethods);



        foreach (var clientGenerator in this.clientGenerators)
        {
            clientGenerator.GenerateClient();
        }
    }

    private IEnumerable<ControllerClientDetails> GetApiClients(IEnumerable<ClassDeclarationSyntax> classNodes, SemanticModel semanticModel)
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
                this.AddControllerMethods(classSymbol.GetMembers(), clientInformation);

                // TODO check if there are even methods inside, if not, dont add it!

                yield return clientInformation;
            }
        }
    }

    public void AddControllerMethods(IEnumerable<ISymbol> methods, ControllerClientDetails clientInformation)
    {
        // TODO make this clearer by extracting methods...

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

    // TODO move methods like these somewhere?
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


public class StringRepresentation
{
    public string GetStringRepresentation(ControllerMethodDetails endpoint)
    {
        return string.Empty;
    }
}



