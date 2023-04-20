using Microsoft.CodeAnalysis;
using System.Text;

namespace ApiClientGenerator.Models.ParameterDetails;

public class RouteQueryParameterDetails : ParameterDetails
{
    public RouteQueryParameterDetails(IParameterSymbol parameterSymbol, bool isSimpleType)
        : base(parameterSymbol)
    {
        

        if (isSimpleType)
        {
            this.RouteQueryManipulationString = $$"""
                                routeBuilder.Replace("{{{parameterSymbol.Name}}}", Uri.EscapeDataString({{parameterSymbol.Name}}.ToString()));
                        """;
        }
        // extract members
        else
        {
            var members = parameterSymbol.Type.GetMembers();
            var routeQueryManipulationStringBuilder = new StringBuilder();

            foreach (var member in members)
            {
                if (member is IPropertySymbol property)
                {
                    if (property.Type is INamedTypeSymbol)
                    {
                        routeQueryManipulationStringBuilder.Append($$"""
                                routeBuilder.Replace("{{{member.Name}}}", Uri.EscapeDataString({{parameterSymbol.Name}}.{{member.Name}}.ToString()));
                                """);
                    }
                }
            }

            this.RouteQueryManipulationString = routeQueryManipulationStringBuilder.ToString();
        }
    }

    public string RouteQueryManipulationString { get; }
}
