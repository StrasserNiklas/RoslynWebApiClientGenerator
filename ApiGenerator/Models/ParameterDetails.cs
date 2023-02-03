using ApiGenerator.Extensions;
using Microsoft.CodeAnalysis;
using System.Text;

namespace ApiGenerator.Models;

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

                foreach (var member in members)
                {
                    if (member is IPropertySymbol property)
                    {
                        if (property.Type is INamedTypeSymbol)
                        {
                            queryStringBuilder.Append($$"""
                                {{member.Name}}={Uri.EscapeDataString({{parameterSymbol.Name}}.{{member.Name}}.ToString())}&
                                """);
                        }
                    }
                }

                this.QueryString = queryStringBuilder.ToString().TrimEnd('&');
            }
        }
    }

    public string QueryString { get; }
    public bool IsRouteQueryParameter { get; set; }
    public string ParameterTypeString { get; }
    public bool HasExplizitDefaultValue { get; }
    public IParameterSymbol ParameterSymbol { get; }
    public bool IsPrimitive { get; }
    public bool IsQueryParameter { get; }
    public bool HasBody { get; }
}
