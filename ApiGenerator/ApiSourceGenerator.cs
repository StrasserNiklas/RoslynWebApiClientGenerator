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
using System.Reflection;
using System.Resources;
using System.Text;

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
            var x1 = context.Compilation.GetMetadataReference(context.Compilation.Assembly);
            var x2 = context.Compilation.GetUsedAssemblyReferences();
            //var x3 = context.Compilation.

            var assemblyReferences = context.Compilation. GetUsedAssemblyReferences().ToList();
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

            //var apiControllers = assemblyClasses.GetApiControllers();
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

        public string GetMethodRoute(IMethodSymbol methodSymbol, AttributeData httpMethodAttribute)
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

    public static class ControllerExtensions
    {

    }

    public static class StringExtensions
    {
        public static string RemoveSuffix(this string stringWithPotentialSuffix, string suffix)
        {
            if (stringWithPotentialSuffix.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                return stringWithPotentialSuffix.Substring(0, stringWithPotentialSuffix.Length - suffix.Length);
            }

            return stringWithPotentialSuffix;
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
            this.GeneratedCodeClasses= new Dictionary<string, string>();
        }

        public string Name { get; }
        public string BaseRoute { get; }

        public bool ContainsHttpMethods => this.HttpMethods.Count() == 0;
        public List<ControllerMethodDetails> HttpMethods { get; set; }

        public IDictionary<string, string> GeneratedCodeClasses { get; set; }
    }

    public class ControllerMethodDetails
    {
        public ControllerMethodDetails(HttpMethod httpMethod, string methodName, string finalRoute)
        {
            this.HttpMethod = httpMethod;
            this.MethodName = methodName + "Async";
            this.Route = finalRoute;
        }

        public string Route { get; }

        public string MethodName { get; }

        public HttpMethod HttpMethod { get; }
        // Request class information
        // Response class information

        public INamedTypeSymbol ReturnType { get; set; }
        // SettleBet, Response, Request
    }

    public static class SymbolStringRepresentationExtensions
    {
        public static IDictionary<string, string> GenerateClassString(this ITypeSymbol symbol)
        {
            string className = symbol.Name;
            string accessibility = symbol.DeclaredAccessibility.ToString().ToLowerInvariant();
            string classModifiers = string.Empty;

            if (symbol.IsAbstract)
            {
                classModifiers += "abstract ";
            }
            if (symbol.IsSealed)
            {
                classModifiers += "sealed ";
            }

            string classType = symbol.TypeKind.ToString().ToLowerInvariant();

            var stringClassRepresentations = new Dictionary<string, string>();

            var codeBuilder = new CodeStringBuilder();
            codeBuilder.AppendFormat("{0} {1} {2} {3}", accessibility, classModifiers, classType, className);
            codeBuilder.OpenCurlyBracketLine();
            
            foreach (var member in symbol.GetMembers())
            {
                if (member is IFieldSymbol field)
                {
                    if (field.DeclaredAccessibility != Accessibility.Public)// || field.Name.StartsWith("get_") || field.Name.StartsWith("set_"))
                    {
                        continue;
                    }

                    codeBuilder.AppendFormat("{0} {1} {2};", accessibility, field.Type, field.Name);
                    codeBuilder.AppendNewLine();
                }
                else if (member is IPropertySymbol property)
                {
                    if (property.Type is INamedTypeSymbol propertyTypeSymbol && propertyTypeSymbol.TypeKind == TypeKind.Class)
                    {
                        if (!propertyTypeSymbol.IsPrimitive())
                        {
                            // TODO rename this
                            var propertyClassTypeString = GenerateClassString(propertyTypeSymbol);

                            foreach (var classStringRepresentation in propertyClassTypeString)
                            {
                                if (!stringClassRepresentations.ContainsKey(classStringRepresentation.Key))
                                {
                                    stringClassRepresentations.Add(classStringRepresentation.Key, classStringRepresentation.Value);
                                }
                            }
                        }
                    }

                    // TODO check if set is available?
                    codeBuilder.AppendFormat("{0} {1} {2} {{ get; set; }}", accessibility, property.Type.ToString().Split('.').LastOrDefault() ?? string.Empty, property.Name);
                    codeBuilder.AppendNewLine();
                }
            }

            codeBuilder.CloseCurlyBracketLine();
            codeBuilder.AppendNewLine();
            stringClassRepresentations.Add(className, codeBuilder.ToString());
            return stringClassRepresentations;
        }
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

