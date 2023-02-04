using ApiGenerator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using ApiGenerator.Models;
using System.Net.Http;
using System;

namespace ApiGenerator;

public class ControllerClientBuilder
{
    private IDictionary<string, HttpMethod> minimalApiMethods = new Dictionary<string, HttpMethod>()
    {
        { "MapGet", HttpMethod.Get} , { "MapPost" , HttpMethod.Post }, { "MapPut" , HttpMethod.Put }, { "MapDelete" , HttpMethod.Delete }
    };//, "Map", "MapWhen" }; MapMethods

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

                    var fromQuery = methodParameter.GetAttribute("Microsoft.AspNetCore.Mvc.FromQueryAttribute");
                    var fromBody = methodParameter.GetAttribute("Microsoft.AspNetCore.Mvc.FromBodyAttribute");
                    var fromHeader = methodParameter.GetAttribute("Microsoft.AspNetCore.Mvc.FromHeaderAttribute");

                    //var fromForm = methodParameter.GetAttribute("Microsoft.AspNetCore.Mvc.FromFormAttribute");
                    //var fromRoute = methodParameter.GetAttribute("Microsoft.AspNetCore.Mvc.FromRouteAttribute");

                    var headerKeys = new List<string>();

                    if (fromHeader is not null)
                    {
                        headerKeys = this.GetHeaderValues(methodParameter, method.Name, clientInformation.Name.Replace("Client", "Controller"));
                    }

                    if (!methodParameter.Type.IsPrimitive())
                    {
                        generatedClasses.AddMany(methodParameter.Type.GenerateClassString());
                    }

                    var parameterAttributeDetails = new ParameterAttributeDetails(fromBody, fromQuery, fromHeader);
                    parameterMapping.Add(methodParameter.Name, new ParameterDetails(methodParameter, methodParameter.Type.IsPrimitive(), parameterAttributeDetails, headerKeys));
                }

                var returnType = this.UnwrapReturnType(methodSymbol.ReturnType);
                var additionalReturnTypes = this.AddMethodResponseTypes(methodSymbol, returnType, generatedClasses);
                var httpMethodInformation = new ControllerMethodDetails(httpMethod, returnType, additionalReturnTypes, parameterMapping, methodNameWithoutAsnyc, finalRoute);
                clientInformation.HttpMethods.Add(httpMethodInformation);
            }
        }

        clientInformation.GeneratedCodeClasses = generatedClasses;
    }

    private List<string> GetHeaderValues(IParameterSymbol methodParameter, string methodName, string controllerClassName)
    {
        var headerKeys = new List<string>();

        if (methodParameter.Type.IsPrimitive())
        {
            headerKeys.Add(methodParameter.Name);
        }
        else
        {
            // check if every property has header attribute
            var members = methodParameter.Type.GetMembers();

            foreach (var member in members)
            {
                if (member is IPropertySymbol property)
                {
                    var fromHeaderAttribute = property.GetAttribute("Microsoft.AspNetCore.Mvc.FromHeaderAttribute");

                    if (fromHeaderAttribute is null)
                    {
                        var errorMessage = $"""
                                            Error with method {methodName} in {controllerClassName} class.
                                            Uses a [FromHeader] attribute on a class that doesn´t annotate all its properties with the [FromHeader] attribute!
                                            """;

                        throw new ArgumentException(errorMessage, methodParameter.Name);
                    }

                    headerKeys.Add(member.Name);
                }
            }
        }

        return headerKeys;
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

            if (responseTypeAttribute.ConstructorArguments.Count() == 1)
            {
                additionalReturnTypes.Add(new KeyValuePair<int, ITypeSymbol>(int.Parse(responseTypeAttribute.ConstructorArguments[0].Value.ToString()), null));
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

        var trimmed = methodRoute.Trim('\\');

        return string.IsNullOrWhiteSpace(methodRoute) ? baseRoute : $"{baseRoute}/{trimmed}";
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

    public void AddMinimalApis(SyntaxTree tree, SemanticModel semanticModel, List<ControllerClientDetails> completeControllerDetailList)
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

            // if you really want to, you can try to find the first parameter/s (id and a object) of the second delegate parameter
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
                var methodDetails = new ControllerMethodDetails(httpMethod, null, null, null, $"{httpMethod.Method}_{route.Replace("/", "")}", route);
                controllerClientDetails.HttpMethods.Add(methodDetails);
            }
        }

        // TODO before minimal APIs can be handled in the generation, you have to enable the option to declare the intended request and response type
        // probably a combination too or can we find this out from
        //  Task<TOut> GET_weatherforecastAsync<TIn, TOut>(TIn inn, CancellationToken cancellationToken)

        // TODO if it works, uncomment this line
        //completeControllerDetailList.Add(controllerClientDetails);
    }
}
