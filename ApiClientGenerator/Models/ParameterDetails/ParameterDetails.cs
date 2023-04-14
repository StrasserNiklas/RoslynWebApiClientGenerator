using ApiGenerator.Extensions;
using Microsoft.CodeAnalysis;

namespace ApiClientGenerator.Models.ParameterDetails;

public abstract class ParameterDetails
{
    public ParameterDetails(IParameterSymbol parameterSymbol)
    {
        this.ParameterName = parameterSymbol.Name;
        this.IsNullable = parameterSymbol.Type.IsNullable();
        this.ParameterTypeString = parameterSymbol.Type.CheckAndSanitizeClassString();
        this.ParameterStringWithTypes = $"{this.ParameterTypeString} {this.ParameterName}";
        this.ParameterStringWithoutTypes = this.ParameterName;
    }

    public string ParameterName { get; }
    public bool IsNullable { get; protected set; }
    public string ParameterTypeString { get; protected set; }
    public string ParameterStringWithTypes { get; protected set; }
    public string ParameterStringWithoutTypes { get; protected set; }
    public bool IsRouteQueryParameter { get; set; }
}
