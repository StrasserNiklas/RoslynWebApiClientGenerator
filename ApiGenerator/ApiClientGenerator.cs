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
        //var compilation = context.Compilation;
        //var attributeType = compilation.GetTypeByMetadataName("System.Web.Http.Headers.HeaderAttribute");

        

    public void Execute(GeneratorExecutionContext context)
    {
        context.Compilation.ExternalReferences[0].Display

        #region OngoingReferenceSearch
        var x1 = context.Compilation.GetMetadataReference(context.Compilation.Assembly);
        var x2 = context.Compilation.GetUsedAssemblyReferences();
        //var x3 = context.Compilation.

        var assemblyReferences = context.Compilation.GetUsedAssemblyReferences().ToList();
        //.OfType<AssemblyMetadata>();
        #endregion
        var projectName = context.Compilation.AssemblyName;// this.GetProjectName(context.Compilation);

        // TODO config in appsettings?
        // or not use this and place in editor config -> context.AnalyzerConfigOptions.GetOptions
        var configuration = new Configuration();

        // in the future this could be done via config, e.g. whether to add a typescript client as well
        this.clientGenerators.Add(new CSharpClientGenerator(configuration, projectName));

        var controllerClientBuilder = new ControllerClientBuilder();
        var completeControllerDetailList = new List<ControllerClientDetails>();

        // TODO what about partial controller classes, must work!
        foreach (var tree in context.Compilation.SyntaxTrees)
        {
            var semanticModel = context.Compilation.GetSemanticModel(tree);

            semanticModel.Compilation.ExternalReferences.SingleOrDefault(x => x.Display == "");

            // check for minimal APIs
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

            //var semanticModel = context.Compilation.GetSemanticModel(tree);

            #region OngoingReferenceSearchWIP
            var ref1 = semanticModel.Compilation.DirectiveReferences.ToList();
            var ref2 = semanticModel.Compilation.References.ToList();
            var ref3 = semanticModel.Compilation.ExternalReferences.ToList();
            var ref4 = semanticModel.Compilation.ReferencedAssemblyNames.ToList();
            var ref5 = semanticModel.Compilation.GetMetadataReference(semanticModel.Compilation.Assembly);

            //CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            //// Use the syntax tree to find "using System;"
            //UsingDirectiveSyntax usingSystem = root.Usings[0];
            //NameSyntax systemName = usingSystem.Name;

            //// Use the semantic model for symbol information:
            //SymbolInfo nameInfo = semanticModel.GetSymbolInfo(systemName);

            //var systemSymbol = (INamespaceSymbol)nameInfo.Symbol;
            //foreach (INamespaceSymbol ns in systemSymbol.GetNamespaceMembers())
            //{
            //    //Console.WriteLine(ns);
            //}

            //INamespaceSymbol x;
            //x.ContainingAssembly;
            //semanticModel.SyntaxTree.GetRoot().

            #endregion

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

    private string GetProjectName(Compilation compilation)
    {
        if (compilation.SyntaxTrees.Count() != 0)
        {
            return Path.GetFileName(Path.GetDirectoryName(compilation.SyntaxTrees.First().FilePath));
        }

        return string.Empty;
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
                var authorizeAttribute = classSymbol.GetAttribute("Microsoft.AspNetCore.Authorization.AuthorizeAttribute");
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
                var authorizeAttribute = methodSymbol.GetAttribute("Microsoft.AspNetCore.Authorization.AuthorizeAttribute");
                

                (var httpMethod, var httpMethodAttribute) = methodSymbol.GetHttpMethodWithAtrributeData();
                var methodRoute = this.GetMethodRoute(methodSymbol, httpMethodAttribute);
                var methodNameWithoutAsnyc = methodSymbol.Name.RemoveSuffix("Async");
                var finalRoute = this.BuildFinalMethodRoute(methodNameWithoutAsnyc, clientInformation.BaseRoute, methodRoute);

                var methodParameters = methodSymbol.Parameters;
                var parameterMapping = new Dictionary<string, ParameterDetails>();

                foreach (var methodParameter in methodParameters)
                {
                    // filter out from services attribute as its value is not needed for the api call
                    if (methodParameter.GetAttribute("Microsoft.AspNetCore.Mvc.FromServicesAttribute") is not null || methodParameter.ToString() == "System.Threading.CancellationToken")
                    {
                        continue;
                    }

                    var lame = methodParameter.GetAttributes();
                    var fromQuery = methodParameter.GetAttribute("Microsoft.AspNetCore.Mvc.FromQueryAttribute");
                    var fromBody = methodParameter.GetAttribute("Microsoft.AspNetCore.Mvc.FromBodyAttribute");
                    //var fromForm = methodParameter.GetAttribute("Microsoft.AspNetCore.Mvc.FromFormAttribute");
                    //var fromHeader = methodParameter.GetAttribute("Microsoft.AspNetCore.Mvc.FromHeaderAttribute");
                    //var fromRoute = methodParameter.GetAttribute("Microsoft.AspNetCore.Mvc.FromRouteAttribute");

                    if (!methodParameter.Type.IsPrimitive())
                    {
                        generatedClasses.AddMany(methodParameter.Type.GenerateClassString());
                    }

                    // TODO solve this in a more elegant way (fromBody etc.)
                    parameterMapping.Add(methodParameter.Name, new ParameterDetails(methodParameter, methodParameter.Type.IsPrimitive(), fromQuery is not null, fromBody is not null));
                }

                var returnType = this.UnwrapReturnType(methodSymbol.ReturnType);
                var additionalReturnTypes = this.AddMethodResponseTypes(methodSymbol, returnType, generatedClasses);
                var httpMethodInformation = new ControllerMethodDetails(httpMethod, additionalReturnTypes, parameterMapping, methodNameWithoutAsnyc, finalRoute);
                clientInformation.HttpMethods.Add(httpMethodInformation);
            }
        }

        clientInformation.GeneratedCodeClasses = generatedClasses;
    }

    private ITypeSymbol UnwrapReturnType(ITypeSymbol returnType)
    {
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

        return returnType;
    }

    private List<KeyValuePair<int, ITypeSymbol>> AddMethodResponseTypes(IMethodSymbol methodSymbol, ITypeSymbol returnType, IDictionary<string, string> generatedClasses)
    {
        var responseTypeAttributes = methodSymbol.GetAttributes("Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute");
        var additionalReturnTypes = new List<KeyValuePair<int, ITypeSymbol>>();

        foreach (var responseTypeAttribute in responseTypeAttributes)
        {
            // in this case the type is desireable, else the response status can be used anyway
            if (responseTypeAttribute.ConstructorArguments.Count() > 1)
            {
                var responseType = responseTypeAttribute.ConstructorArguments[0].Value as ITypeSymbol;
                generatedClasses.AddMany(responseType.GenerateClassString());
                var code = int.Parse(responseTypeAttribute.ConstructorArguments[1].Value.ToString());
                var exists = additionalReturnTypes.SingleOrDefault(x => x.Key == code);

                if (exists.Value == null)
                {
                    additionalReturnTypes.Add(new KeyValuePair<int, ITypeSymbol>(code, responseType));
                }
            }
        }

        if (returnType != null)
        {
            generatedClasses.AddMany(returnType.GenerateClassString());

            var exists = additionalReturnTypes.SingleOrDefault(x => x.Value.Name == returnType.Name);

            if (exists.Value != null)
            {
                additionalReturnTypes.Remove(exists);
                additionalReturnTypes.Insert(0, exists);
            }
            else
            {
                var keyExists = additionalReturnTypes.SingleOrDefault(x => x.Key == 200);

                if (keyExists.Key != 0)
                {
                    additionalReturnTypes.Remove(keyExists);
                }

                additionalReturnTypes.Insert(0, new KeyValuePair<int, ITypeSymbol>(200, returnType));
            }
        }

        return additionalReturnTypes;
    }

    private string BuildFinalMethodRoute(string methodName, string baseRoute, string methodRoute)
    {
        if (baseRoute.Contains("[action]"))
        {
            baseRoute = baseRoute.Replace("[action]", methodName);
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
