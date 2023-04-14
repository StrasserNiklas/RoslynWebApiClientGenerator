using Microsoft.CodeAnalysis;
using System.Text;

namespace ApiClientGenerator.Models.ParameterDetails;

public class QueryParameterDetails : ParameterDetails
{
    public QueryParameterDetails(IParameterSymbol parameterSymbol, bool isSimpleType)
        : base(parameterSymbol)
    {
        if (isSimpleType)
        {
            this.QueryString = $$"""
                    ?{{parameterSymbol.Name}}={Uri.EscapeDataString({{parameterSymbol.Name}}.ToString())}
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

    public string QueryString { get; }

}
