using Microsoft.CodeAnalysis;

namespace ApiClientGenerator.Models.ParameterDetails;

public class BodyParameterDetails : ParameterDetails
{
    public BodyParameterDetails(IParameterSymbol parameterSymbol)
        : base(parameterSymbol)
    {
    }
}
