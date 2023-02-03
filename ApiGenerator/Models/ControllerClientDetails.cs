using ApiGenerator.Extensions;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace ApiGenerator.Models;

public class ControllerClientDetails
{
    public ControllerClientDetails(string symbolName, AttributeData routeAttributeData, bool isMinimalApiClient = false)
    {
        var baseName = symbolName.EndsWith("Controller") ?
            symbolName.Substring(0, symbolName.Length - "Controller".Length) :
            symbolName;

        var route = routeAttributeData?.ConstructorArguments.FirstOrDefault().Value?.ToString();

        this.Name = $"{baseName}Client";
        this.BaseRoute = string.IsNullOrWhiteSpace(route) ? string.Empty : route.Replace("[controller]", baseName);
        this.HttpMethods = new List<ControllerMethodDetails>();
        this.GeneratedCodeClasses = new Dictionary<string, string>();
        this.IsMinimalApiClient = isMinimalApiClient;
    }

    public string Name { get; }
    public string BaseRoute { get; }

    public List<ControllerMethodDetails> HttpMethods { get; set; }

    public IDictionary<string, string> GeneratedCodeClasses { get; set; }
    public bool IsMinimalApiClient { get; }

    // minimalClient. //Route<T
}

public class ControllerMethodDetails
{
    public ControllerMethodDetails(HttpMethod httpMethod, List<KeyValuePair<int, ITypeSymbol>> returnTypes, Dictionary<string, ParameterDetails> parameters, string methodName, string finalRoute)
    {
        this.HttpMethod = httpMethod;
        ReturnTypes = returnTypes;
        this.MethodName = methodName + "Async";
        this.Route = finalRoute;

        var matches = Regex.Matches(finalRoute, @"\{(.*?)\}");

        if (matches.Count > 0)
        {
            foreach (Match match in matches)
            {
                if (parameters.TryGetValue(match.Groups[1].Value, out var parameterDetails))
                {
                    parameterDetails.IsRouteQueryParameter = true;
                }
            }

            this.HasRouteQueryParameters = true;
        }

        this.Parameters = parameters;

        if (parameters != null)
        {
            this.ParameterString = string.Join(", ", parameters.Select(x => $"{x.Value.ParameterTypeString}  {x.Key}")).TrimEnd(',');
        }

        this.ReturnType = returnTypes?.FirstOrDefault().Value;

        // TODO check if we can just hand it over in the ctor
        if (this.ReturnType != null)
        {
            this.ReturnTypeString = this.ReturnType.SanitizeClassTypeString();
        }
    }

    public bool HasRouteQueryParameters { get; }

    public string Route { get; }

    public string MethodName { get; }

    public HttpMethod HttpMethod { get; }
    public List<KeyValuePair<int, ITypeSymbol>> ReturnTypes { get; }

    public bool HasParameters => this.Parameters != null && this.Parameters.Count > 0;

    public Dictionary<string, ParameterDetails> Parameters { get; } 

    public string ParameterString { get; }

    public bool HasReturnType => this.ReturnType != null;

    public string ReturnTypeString { get; }

    public ITypeSymbol ReturnType { get; }
}

public class ParameterDetails
{
    public ParameterDetails(IParameterSymbol parameterSymbol, bool isPrimitive, bool isQueryParameter, bool hasBody)
    {
        this.HasExplizitDefaultValue = parameterSymbol.HasExplicitDefaultValue;
        this.ParameterSymbol = parameterSymbol;
        this.IsPrimitive = isPrimitive;
        this.IsQueryParameter = isQueryParameter;
        this.HasBody = hasBody;
        this.ParameterTypeString = parameterSymbol.Type.SanitizeClassTypeString();

        if (isQueryParameter)
        {
            if (isPrimitive)
            {
                this.QueryString = $$"""
                    "?{{parameterSymbol.Name}}={Uri.EscapeDataString({{parameterSymbol.Name}}.ToString())}"
                    """;
            }
            // extract members
            else
            {
                var members = parameterSymbol.Type.GetMembers();
                var queryStringBuilder = new StringBuilder("?");

                foreach ( var member in members)
                {
                    if (member is IPropertySymbol property)
                    {
                        if (property.Type is INamedTypeSymbol propertyTypeSymbol)
                        {
                            queryStringBuilder.Append($$"""
                                {{member.Name}}={Uri.EscapeDataString({{parameterSymbol.Name}}.{{member.Name}}.ToString())}&
                                """);
                        }

                        //var outputString = property.Type.ToString().SanitizeClassTypeString();
                    }
                }

                this.QueryString = queryStringBuilder.ToString().TrimEnd('&');
            }
        }
    }

    public string QueryString { get;  }

    public bool IsRouteQueryParameter { get; set; }
    //public string Name { get; set; }
    public string ParameterTypeString { get; }
    public bool HasExplizitDefaultValue { get; }
    public IParameterSymbol ParameterSymbol { get; }
    public bool IsPrimitive { get; }
    public bool IsQueryParameter { get; }
    public bool HasBody { get; }
}
