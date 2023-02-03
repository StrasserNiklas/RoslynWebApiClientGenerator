﻿using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

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
