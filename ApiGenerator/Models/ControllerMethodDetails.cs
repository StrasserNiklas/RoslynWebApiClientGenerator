using ApiGenerator.Extensions;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace ApiGenerator.Models;

public class ControllerMethodDetails
{
    public ControllerMethodDetails(HttpMethod httpMethod, ITypeSymbol returnType, List<KeyValuePair<int, ITypeSymbol>> returnTypes, Dictionary<string, ParameterDetails> parameters, string methodName, string finalRoute)
    {
        this.HttpMethod = httpMethod;
        this.ReturnTypes = returnTypes;
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

        this.ReturnType = returnType;

        if (this.ReturnType != null)
        {
            this.ReturnTypeString = this.ReturnType.CheckAndSanitizeClassString();
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
