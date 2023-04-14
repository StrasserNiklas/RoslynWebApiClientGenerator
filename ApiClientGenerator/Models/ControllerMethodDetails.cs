using ApiClientGenerator.Models.ParameterDetails;
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
            this.ParameterStringWithTypes = string.Join(", ", parameters.Select(x => x.Value.ParameterStringWithTypes)).TrimEnd(',');
            this.ParameterStringWithoutTypes = string.Join(", ", parameters.Select(x => x.Value.ParameterStringWithoutTypes)).TrimEnd(',');
        }

        this.HasReturnType = returnType != null;

        if (this.HasReturnType)
        {
            this.MainReturnTypeString = returnType.CheckAndSanitizeClassString();
        }
    }
    public string MethodName { get; }
    public string Route { get; }
    public HttpMethod HttpMethod { get; }
    public bool HasRouteQueryParameters { get; }
    public bool HasParameters => this.Parameters != null && this.Parameters.Count > 0;
    public IDictionary<string, ParameterDetails> Parameters { get; }
    public string ParameterStringWithTypes { get; }
    public string ParameterStringWithoutTypes { get; }
    public bool HasReturnType { get; }
    public string MainReturnTypeString { get; }
    public IEnumerable<KeyValuePair<int, ITypeSymbol>> ReturnTypes { get; }
    public MatchCollection RouteQueryMatches { get; } // for minimal API

}
