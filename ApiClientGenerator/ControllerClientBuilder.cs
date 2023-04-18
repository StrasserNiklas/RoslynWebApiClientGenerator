using ApiGenerator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using ApiGenerator.Models;
using System.Net.Http;
using System;
using ApiGenerator.Diagnostics;
using ApiClientGenerator.Models.ParameterDetails;

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
        var generatedClasses = new Dictionary<string, string>();
        var additionalUsings = new List<string>();

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

                if (httpMethod is null)
                {
                    DiagnosticReporter.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.GenericWarning, Location.None, $"{method.Name} is marked public but doesn´t have a HttpMethodAttribute.", "Will be ignored for generation!"));
                    continue;
                }

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

                    if (methodParameter.Type.ToString() == "System.Threading.CancellationToken")
                    {
                        continue;
                    }

                    if (!methodParameter.Type.IsSimpleType())
                    {
                        var generatedClassDetails = methodParameter.Type.GenerateClassString();
                        additionalUsings.AddRange(generatedClassDetails.AdditionalUsings);
                        generatedClasses.AddMany(generatedClassDetails.GeneratedCodeClasses);
                    }

                    var parameterDetails = this.GetParameterDetails(methodParameter, method.Name, clientInformation.ClientName.Replace("Client", "Controller"));
                    parameterMapping.Add(methodParameter.Name, parameterDetails);
                }

                var returnType = this.UnwrapReturnType(methodSymbol.ReturnType);
                var additionalReturnTypes = this.AddMethodResponseTypes(methodSymbol, returnType, generatedClasses, additionalUsings);
                var httpMethodInformation = new ControllerMethodDetails(httpMethod, returnType, additionalReturnTypes, parameterMapping, methodNameWithoutAsnyc, finalRoute);
                clientInformation.Endpoints.Add(httpMethodInformation);
            }
        }

        clientInformation.AdditionalUsings = additionalUsings;
        clientInformation.GeneratedCodeClasses = generatedClasses;
    }

    private ParameterDetails GetParameterDetails(IParameterSymbol methodParameter, string methodName, string controllerClassName)
    {
        var fromQuery = methodParameter.GetAttribute("Microsoft.AspNetCore.Mvc.FromQueryAttribute");
        var fromBody = methodParameter.GetAttribute("Microsoft.AspNetCore.Mvc.FromBodyAttribute");
        var fromHeader = methodParameter.GetAttribute("Microsoft.AspNetCore.Mvc.FromHeaderAttribute");
        var fromRoute = methodParameter.GetAttribute("Microsoft.AspNetCore.Mvc.FromRouteAttribute");
        var fromForm = methodParameter.GetAttribute("Microsoft.AspNetCore.Mvc.FromFormAttribute");
        var isSimpleType = methodParameter.Type.IsSimpleType();

        if (fromHeader is not null)
        {
            var headerKeys = this.GetHeaderValues(methodParameter, methodName, controllerClassName);
            return new HeaderParameterDetails(methodParameter, isSimpleType, headerKeys);
        }
        else if (fromQuery is not null)
        {
            return new QueryParameterDetails(methodParameter, isSimpleType);
        }
        else if (fromForm is not null)
        {
            return new FormParameterDetails(methodParameter, isSimpleType);
        }
        else if (fromBody is not null)
        {
            return new BodyParameterDetails(methodParameter);
        }
        else if (fromRoute is not null)
        {
            return new RouteQueryParameterDetails(methodParameter, isSimpleType);
        }

        // no attribute set
        // what if its a route parameter?
        return new QueryParameterDetails(methodParameter, isSimpleType);

    }

    private Dictionary<string, HeaderDetails> GetHeaderValues(IParameterSymbol methodParameter, string methodName, string controllerClassName)
    {
        var headerKeys = new Dictionary<string, HeaderDetails>();

        if (methodParameter.Type.IsSimpleType())
        {
            var fromHeaderAttribute = methodParameter.GetAttribute("Microsoft.AspNetCore.Mvc.FromHeaderAttribute");
            headerKeys.Add(this.GetHeaderName(methodParameter, fromHeaderAttribute), new HeaderDetails(methodParameter.Name, methodParameter.Type));
        }
        else
        {
            // check if at least one property has header attribute
            var members = methodParameter.Type.GetMembers();
            var allMembersHaveAttribute = true;
            var hasMemberWithAttribute = false;

            foreach (var member in members)
            {
                if (member is IPropertySymbol property)
                {
                    var fromHeaderAttribute = property.GetAttribute("Microsoft.AspNetCore.Mvc.FromHeaderAttribute");

                    if (fromHeaderAttribute is null)
                    {
                        allMembersHaveAttribute = false;
                        continue;
                    }

                    hasMemberWithAttribute = true;
                    headerKeys.Add(this.GetHeaderName(member, fromHeaderAttribute), new HeaderDetails(member.Name, property.Type));
                }
            }

            if (!hasMemberWithAttribute)
            {
                DiagnosticReporter.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.AttributeMissing, Location.None, "[FromHeader]", methodParameter.Name, methodName,  controllerClassName));
            }

            // name müsste der key werden...
            //[FromHeader(Name = "x-asw-locale")]
            // geht darum dass ned a klasse angegeben werden muss wenn nedmal alle dinger verwendet werden
            // vl sollt ich oben schon nur die einzelnen values hernehmen und ned die ganze klasse? 
            // TODO maybe
            if (!allMembersHaveAttribute)
            {
                // hier den key bzw value ändern?
            }
        }

        return headerKeys;
    }

    private string GetHeaderName(ISymbol symbol, AttributeData fromHeaderAttribute)
    {
        var headerName = symbol.Name;

        if (fromHeaderAttribute is not null)
        {
            if (fromHeaderAttribute.NamedArguments.Any())
            {
                var actualHeaderName = fromHeaderAttribute.NamedArguments.FirstOrDefault().Value.Value.ToString();

                if (!string.IsNullOrWhiteSpace(actualHeaderName))
                {
                    return actualHeaderName;
                }
            }
        }

        return headerName;
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

    private IEnumerable<KeyValuePair<int, ITypeSymbol>> AddMethodResponseTypes(IMethodSymbol methodSymbol, ITypeSymbol returnType, IDictionary<string, string> generatedClasses, List<string> additionalUsings)
    {
        var responseTypeAttributes = methodSymbol.GetAttributes("Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute");
        var additionalReturnTypes = new List<KeyValuePair<int, ITypeSymbol>>();

        foreach (var responseTypeAttribute in responseTypeAttributes)
        {
            // in this case the type is desireable, else the response status can be used anyway
            if (responseTypeAttribute.ConstructorArguments.Count() > 1)
            {
                var responseType = responseTypeAttribute.ConstructorArguments[0].Value as ITypeSymbol;
                var generatedCodeDetails = responseType.GenerateClassString();
                additionalUsings.AddRange(generatedCodeDetails.AdditionalUsings);
                generatedClasses.AddMany(generatedCodeDetails.GeneratedCodeClasses);
                var code = int.Parse(responseTypeAttribute.ConstructorArguments[1].Value.ToString());
                var exists = additionalReturnTypes.SingleOrDefault(x => x.Key == code);

                if (exists.Value == null)
                {
                    if (responseType.ToString() == "void")
                    {
                        additionalReturnTypes.Add(new KeyValuePair<int, ITypeSymbol>(code, null));
                    }
                    else
                    {
                        additionalReturnTypes.Add(new KeyValuePair<int, ITypeSymbol>(code, responseType));
                    }
                }
            }

            if (responseTypeAttribute.ConstructorArguments.Count() == 1)
            {
                additionalReturnTypes.Add(new KeyValuePair<int, ITypeSymbol>(int.Parse(responseTypeAttribute.ConstructorArguments[0].Value.ToString()), null));
            }

        }

        if (returnType != null)
        {
            var generatedCodeDetails = returnType.GenerateClassString();
            additionalUsings.AddRange(generatedCodeDetails.AdditionalUsings);
            generatedClasses.AddMany(generatedCodeDetails.GeneratedCodeClasses);

            var exists = additionalReturnTypes.SingleOrDefault(x =>
            {
                if (x.Value is not null)
                {
                    return x.Value.Name == returnType.Name;
                }

                return false;
            });

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

        var methodRouteNotNullOrWhitespace = string.IsNullOrWhiteSpace(methodRoute);

        if (!methodRouteNotNullOrWhitespace && methodRoute.StartsWith("/"))
        {
            // if the route starts with a '/', the base route will be ignored by the API
            return methodRoute.Trim('/');
        }

        return methodRouteNotNullOrWhitespace ? baseRoute : $"{baseRoute}/{methodRoute}";
    }

    private string GetMethodRoute(IMethodSymbol methodSymbol, AttributeData httpMethodAttribute)
    {
        var routeAttribute = methodSymbol.GetRouteAttribute();

        if (routeAttribute != null)
        {
            return routeAttribute.ConstructorArguments.FirstOrDefault().Value.ToString() ?? string.Empty;
        }

        return httpMethodAttribute?.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? string.Empty;
    }

    public void AddMinimalApis(SyntaxTree tree, SemanticModel semanticModel, ControllerClientDetails minimalApiController)
    {
        var root = tree.GetRoot();
        var methodInvocations = root
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(x => x.Expression is MemberAccessExpressionSyntax expression && this.minimalApiMethods.Keys.Contains(expression.Name.Identifier.Value));

        if (methodInvocations.Count() == 0)
        {
            return;
        }

        foreach (var invocation in methodInvocations)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(invocation);

            // if you really want to, you can try to find the first parameter/s (id and a object) of the second delegate parameter
            // there does not seem to be a way of doing that using semantics
            // in addition, a parameter of the delegate could easily be a service injection
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
                minimalApiController.Endpoints.Add(methodDetails);
            }
        }
    }
}
