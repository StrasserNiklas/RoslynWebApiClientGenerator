using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Resources;

namespace ApiGenerator
{
    [Generator]
    public class ApiSourceGenerator : ISourceGenerator
    {
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
            var assemblyClasses = context.Compilation .SyntaxTrees.SelectMany(
                x => x.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>());

            var apiClients = new List<ControllerClientDetails>();

            foreach (var tree in context.Compilation.SyntaxTrees)
            {
                var semanticModel = context.Compilation.GetSemanticModel(tree);

                if (!semanticModel.ContainsControllerTypes())
                {
                    // TODO return or yield break or continue
                    //yield break;
                    continue;
                }

                var classNodes = semanticModel.SyntaxTree
                    .GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>();

                apiClients.AddRange(this.GetApiClients(classNodes, semanticModel));
            }

            var tek = apiClients.Where(client => client.ContainsHttpMethods);

            var apiControllers = assemblyClasses.GetApiControllers();
            //this.Run(apiControllers);
        }

        public IEnumerable<ControllerClientDetails> GetApiClients(IEnumerable<ClassDeclarationSyntax> classNodes, SemanticModel semanticModel)
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
                    var controllerMethods = this.GetControllerMethods(classSymbol.GetMembers(), clientInformation.BaseRoute);
                    clientInformation.HttpMethods.AddRange(controllerMethods);
                    yield return clientInformation;
                }
            }
        }

        public IEnumerable<ControllerMethodDetails> GetControllerMethods(IEnumerable<ISymbol> methods, string baseRoute)
        {
            // get http method
            // get request and return types
            // beware off Task<Response>

            foreach (var method in methods)
            {
                if (method is IMethodSymbol
                    {
                        DeclaredAccessibility: Accessibility.Public,
                        MethodKind: MethodKind.Ordinary,
                        IsAbstract: false
                    } methodSymbol)
                {
                    var attribute = methodSymbol.GetAttribute("Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute");

                    var httpMethod = attribute?.AttributeClass?.Name switch
                    {
                        "HttpGetAttribute" => HttpMethod.Get,
                        "HttpPutAttribute" => HttpMethod.Put,
                        "HttpPostAttribute" => HttpMethod.Post,
                        "HttpDeleteAttribute" => HttpMethod.Delete,
                        _ => HttpMethod.Get // TODO does this work / do we even want it
                    };


                    var httpMethodAttributeRoute = attribute?.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? string.Empty;

                    var routeAttr = methodSymbol.GetRouteAttribute();

                    if (routeAttr != null)
                    {
                        httpMethodAttributeRoute = routeAttr.ConstructorArguments.FirstOrDefault().Value.ToString() ?? string.Empty;
                    }

                    var httpMethodInformation = new ControllerMethodDetails(httpMethod);
                    yield return httpMethodInformation;
                }

                Debug.WriteLine("---------------------------------------------");
                Debug.WriteLine(method.Name);
                Debug.WriteLine("Is abstract: " + method.IsAbstract);
                Debug.WriteLine(method.GetAttributes().FirstOrDefault()?.ToString());
            }
        }
    }

    public class StringRepresentation
    {
        public string GetStringRepresentation(ControllerMethodDetails endpoint)
        {
            return string.Empty;
        }
    }

    public class ControllerClientDetails
    {
        public ControllerClientDetails(string symbolName, AttributeData routeAttributeData)
        {
            var baseName = symbolName.EndsWith("Controller") ?
                symbolName.Substring(0, symbolName.Length - "Controller".Length) :
                symbolName;

            var route = routeAttributeData?.ConstructorArguments.FirstOrDefault().Value?.ToString();
            
            this.Name = $"{baseName}Client";
            this.BaseRoute = string.IsNullOrWhiteSpace(route) ? string.Empty : route.Replace("[controller]", baseName);
            this.HttpMethods = new List<ControllerMethodDetails>();
        }

        public string Name { get; }
        public string BaseRoute { get; }

        public bool ContainsHttpMethods => this.HttpMethods.Count() == 0;
        public List<ControllerMethodDetails> HttpMethods { get; set; }
    }

    public class ControllerMethodDetails
    {
        public ControllerMethodDetails(HttpMethod httpMethod)
        {
            this.HttpMethod = httpMethod;
        }

        public string Route { get; } //also via constructor

        public string MethodName { get; } // get from symbol + "Async"

        public HttpMethod HttpMethod { get; }
        // Request class information
        // Response class information

        // SettleBet, Response, Request
    }

    public class OldCode
    {
        public static void Run(IEnumerable<ClassDeclarationSyntax> apiControllers, IEnumerable<ClassDeclarationSyntax> assemblyClasses)
        {
            foreach (var classDeclaration in apiControllers)
            {
                //classDeclaration

                var classAttributes = classDeclaration.AttributeLists.SelectMany(al => al.Attributes).ToList();

                //var baseRoute = classDeclaration.GetBaseRoute(classAttributes);

                var authorizeAttribute = classAttributes.SingleOrDefault(a => a.Name.ToString() == "Authorize");

                var parameters = classDeclaration.TypeParameterList;
                var tree = classDeclaration.SyntaxTree;
                //IEnumerable<(TypeSyntax requestType, TypeSyntax responseType)> endpoints = Analyz.FindEndpoints(tree.GetRoot());

                //var syntaxGenerator = SyntaxGenerator.GetGenerator(new AdhocWorkspace(), LanguageNames.CSharp);
                //var generatedCode = syntaxGenerator.GenerateTypeDeclaration(classDeclaration);

                var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>().GetPublicMethods();
                var route = classDeclaration.GetBaseRoute(classAttributes);

                Debug.WriteLine("---------------------------------------------");
                Debug.WriteLine("Class: " + classDeclaration.Identifier.ToFullString());
                Debug.WriteLine("Route: " + route);
                Debug.WriteLine("Methods:");

                foreach (var method in methods)
                {
                    Debug.WriteLine("***********");

                    // Get the request and response types for the endpoint
                    var methodParameters = method.ParameterList.Parameters;

                    var parameterStringified = string.Join(string.Empty, methodParameters.Select(parameter => parameter.Type.ToFullString() + parameter.Identifier.ValueText));

                    TypeSyntax requestType = method.ParameterList.Parameters
                        .Select(p => p.Type)
                        .FirstOrDefault();
                    TypeSyntax responseType = method.ReturnType;

                    // Add the endpoint to the list
                    //_endpoints.Add((requestType, responseType));

                    Debug.WriteLine($"public {responseType.ToFullString().Trim()} {method.Identifier.ToFullString().Trim()}({parameterStringified.Trim()})");

                    Debug.WriteLine("Request class(es) used:");

                    foreach (var parameter in methodParameters)
                    {
                        var parameterClass = assemblyClasses.SingleOrDefault(classInstance => classInstance.Identifier.ValueText == parameter.Identifier.ValueText);
                        var propertyTypes = classDeclaration?
                            .DescendantNodes().OfType<PropertyDeclarationSyntax>()
                            .Where(p => p.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                            .Select(p => p.Type)
                            .ToList() ?? new List<TypeSyntax>();

                        Debug.WriteLine(@"

");

                    }

                    Debug.WriteLine("Response class(es) used:");

                    var res = method.ReturnType;
                    //res.

                }

                //    foreach (var method in publicMethods)
                //    {
                //        var routeAttribute = method.AttributeLists.SelectMany(al => al.Attributes)
                //.SingleOrDefault(a => a.Name.ToString() == "Route")?.ArgumentList?.Arguments.FirstOrDefault();
                //        // Get the request and response types for the endpoint
                //        TypeSyntax requestType = method.ParameterList.Parameters
                //            .Select(p => p.Type)
                //            .FirstOrDefault();
                //        TypeSyntax responseType = method.ReturnType;

                //        // Add the endpoint to the list
                //        _endpoints.Add((requestType, responseType));
                //    }
            }
        }
    }
}

