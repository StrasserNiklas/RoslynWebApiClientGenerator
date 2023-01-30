using ApiGenerator.Extensions;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

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
}

public class ControllerMethodDetails
{
    public ControllerMethodDetails(HttpMethod httpMethod, ITypeSymbol returnType, string methodName, string finalRoute)
    {
        this.HttpMethod = httpMethod;
        this.MethodName = methodName + "Async";
        this.Route = finalRoute;

        // TODO this is obviously based on what is there
        this.HasParameters = false;

        this.ReturnType = returnType;

        // TODO check if we can just hand it over in the ctor
        this.ReturnTypeString = (returnType as INamedTypeSymbol).ToString().SanitizeClassTypeString();
    }

    public string Route { get; }

    public string MethodName { get; }

    public HttpMethod HttpMethod { get; }
    // Request class information
    // Response class information

    public bool HasParameters { get;  }

    // TODO might just check null on the return type?
    public bool HasReturnType => this.ReturnType != null;

    public string ReturnTypeString { get; }

    public ITypeSymbol ReturnType { get; }

    // SettleBet, Response, Request
}
