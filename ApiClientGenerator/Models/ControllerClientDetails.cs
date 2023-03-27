using Microsoft.CodeAnalysis;
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

        this.ClientName = $"{baseName}Client";
        this.BaseRoute = string.IsNullOrWhiteSpace(route) ? string.Empty : route.Replace("[controller]", baseName);
        this.Endpoints = new List<ControllerMethodDetails>();
        this.GeneratedCodeClasses = new Dictionary<string, string>();
        this.IsMinimalApiClient = isMinimalApiClient;
        this.ReferencedAssemblyNamespaces = new Dictionary<string, string>();
        this.AdditionalUsings = new List<string>();
    }

    public string ClientName { get; }
    public string BaseRoute { get; }
    public List<ControllerMethodDetails> Endpoints { get; set; }
    public IDictionary<string, string> GeneratedCodeClasses { get; set; }
    public bool IsMinimalApiClient { get; }






    public List<string> AdditionalUsings { get; set; }
    public IDictionary<string, string> ReferencedAssemblyNamespaces { get; set; }

}
