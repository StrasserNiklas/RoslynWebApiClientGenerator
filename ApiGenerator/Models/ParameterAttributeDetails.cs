using Microsoft.CodeAnalysis;

namespace ApiGenerator.Models;

public class ParameterAttributeDetails
{
    public ParameterAttributeDetails(AttributeData fromBodyAttribute, AttributeData fromQueryAttribute, AttributeData fromHeaderAttribute)
    {
        this.HasBodyAttribute = fromBodyAttribute is not null;
        this.HasQueryAttribute = fromQueryAttribute is not null;
        this.HasHeaderAttribute = fromHeaderAttribute is not null;
    }

    public bool HasHeaderAttribute { get; }
    public bool HasBodyAttribute { get; }
    public bool HasQueryAttribute { get; }
}
