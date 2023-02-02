﻿using ApiGenerator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using ApiGenerator.Models;

namespace ApiGenerator;

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