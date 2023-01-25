using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace ApiGenerator.Models;

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
        this.GeneratedCodeClasses = new Dictionary<string, string>();
    }

    public string Name { get; }
    public string BaseRoute { get; }

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

        // TODO this is obviously based on what is there
        this.HasParameters = false;

        // TODO same here
        this.HasReturnType = false;
    }

    public string Route { get; }

    public string MethodName { get; }

    public HttpMethod HttpMethod { get; }
    // Request class information
    // Response class information

    public bool HasParameters { get;  }

    // TODO might just check null on the return type?
    public bool HasReturnType { get; }

    public INamedTypeSymbol ReturnType { get; set; }
    // SettleBet, Response, Request
}
