using ApiGenerator.Extensions;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace ApiGenerator.Models;

public class ControllerMethodDetails
{
    public ControllerMethodDetails(HttpMethod httpMethod, ITypeSymbol returnType, IEnumerable<KeyValuePair<int, ITypeSymbol>> returnTypes, Dictionary<string, ParameterDetails> parameters, string methodName, string finalRoute)
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
                var value = match.Value;

                if (parameters is not null)
                {
                    if (parameters.TryGetValue(match.Groups[1].Value, out var parameterDetails))
                    {
                        parameterDetails.IsRouteQueryParameter = true;
                    }
                }

                // needed for minimal api at the moment to avoid a method name like 'Name{id}' which would break the generated code
                if (methodName.Contains(value))
                {
                    this.MethodName = this.MethodName.Replace(value, "");
                }
            }

            this.HasRouteQueryParameters = true;
            this.RouteQueryMatches = matches;
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
    public MatchCollection RouteQueryMatches { get; }
    public string Route { get; }
    public string MethodName { get; }
    public HttpMethod HttpMethod { get; }
    public IEnumerable<KeyValuePair<int, ITypeSymbol>> ReturnTypes { get; }
    public bool HasParameters => this.Parameters != null && this.Parameters.Count > 0;
    public IDictionary<string, ParameterDetails> Parameters { get; }
    public string ParameterString { get; }
    public bool HasReturnType => this.ReturnType != null;
    public string ReturnTypeString { get; }
    public ITypeSymbol ReturnType { get; }
}
