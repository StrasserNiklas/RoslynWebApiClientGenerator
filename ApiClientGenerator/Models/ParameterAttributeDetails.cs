using Microsoft.CodeAnalysis;

namespace ApiGenerator.Models;

public class ParameterAttributeDetails
{
    public ParameterAttributeDetails(AttributeData fromBodyAttribute, AttributeData fromQueryAttribute, AttributeData fromHeaderAttribute, AttributeData fromRouteAttribute, AttributeData fromForm)
    {
        this.HasBodyAttribute = fromBodyAttribute is not null;
        this.HasQueryAttribute = fromQueryAttribute is not null;
        this.HasHeaderAttribute = fromHeaderAttribute is not null;
        this.HasRouteAttribute = fromRouteAttribute is not null;
        this.HasFormAttribute = fromForm is not null;
    }

    public bool HasNoAttributes => !this.HasBodyAttribute && !this.HasHeaderAttribute && !this.HasQueryAttribute && !this.HasRouteAttribute && !this.HasFormAttribute;
    public bool HasHeaderAttribute { get; }
    public bool HasBodyAttribute { get; }
    public bool HasQueryAttribute { get; }
    public bool HasRouteAttribute { get; }
    public bool HasFormAttribute { get; }
}
