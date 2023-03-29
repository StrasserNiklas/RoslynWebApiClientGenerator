using ApiGenerator.Extensions;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiGenerator.Models;

public class ParameterDetails
{
    public ParameterDetails(IParameterSymbol parameterSymbol, bool isPrimitive, ParameterAttributeDetails parameterAttributeDetails, Dictionary<string, HeaderDetails> headerKeyValues)
    {
        this.Name = parameterSymbol.Name;
        //this.HasExplizitDefaultValue = parameterSymbol.HasExplicitDefaultValue;
        //this.ParameterSymbol = parameterSymbol;
        //this.IsPrimitive = isPrimitive;
        this.IsNullable = parameterSymbol.Type.IsNullable();
        this.AttributeDetails = parameterAttributeDetails;
        this.HeaderKeyValues = new Dictionary<string, string>();
        this.ComplexTypeString = parameterSymbol.Type.CheckAndSanitizeClassString();
        this.ParameterStringWithTypes = $"{this.ComplexTypeString} {this.Name}";
        this.ParameterStringWithoutTypes = this.Name;

        if (parameterAttributeDetails.HasHeaderAttribute)
        {
            // should you also check if the symbol even has the attribute?
            var memberCount = parameterSymbol.Type.GetMembers().Count(s => s is IPropertySymbol);
            
            if (isPrimitive)
            {
                this.HeaderKeyValues.Add(headerKeyValues.First().Key, headerKeyValues.First().Value.Name);
            }
            else if (memberCount != headerKeyValues.Count && headerKeyValues.Count > 0)
            {
                // set the parameter different
                this.IsNullable = false;
                this.ParameterStringWithTypes = string.Empty;
                this.ParameterStringWithoutTypes = string.Empty;

                foreach (var header in headerKeyValues)
                {
                    var name = char.ToLowerInvariant(header.Value.Name[0]) + header.Value.Name.Substring(1);
                    this.ParameterStringWithTypes += $"{header.Value.TypeInformation.CheckAndSanitizeClassString()} {name}, ";
                    this.ParameterStringWithoutTypes += $"{name}, ";
                    this.HeaderKeyValues.Add(header.Key, name);
                }

                this.ParameterStringWithTypes = this.ParameterStringWithTypes.TrimEnd(',', ' ');
                this.ParameterStringWithoutTypes = this.ParameterStringWithoutTypes.TrimEnd(',', ' ');
            }
            else
            {
                foreach (var header in headerKeyValues)
                {
                    this.HeaderKeyValues.Add(header.Key, this.Name + '.' + header.Value.Name);
                }
            }
        }

        if (parameterAttributeDetails.HasNoAttributes && isPrimitive)
        {
            this.QueryString = $$"""
                    ?{{parameterSymbol.Name}}={Uri.EscapeDataString({{parameterSymbol.Name}}.ToString())}
                    """;
        }

        if (parameterAttributeDetails.HasFormAttribute)
        {
            var formStringBuilder = new StringBuilder();

            formStringBuilder.AppendLine($$"""
                var formBoundaryGuid = Guid.NewGuid().ToString();
                var formContent = new MultipartFormDataContent(formBoundaryGuid);
                formContent.Headers.Remove("Content-Type");
                formContent.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data; boundary=" + formBoundaryGuid);
                """);

            if (isPrimitive)
            {
                formStringBuilder.AppendLine($"formContent.Add(new System.Net.Http.StringContent({parameterSymbol.Name}.ToString()), \"{parameterSymbol.Name}\");");
            }
            else
            {
                var members = parameterSymbol.Type.GetMembers();

                foreach (var member in members)
                {
                    if (member is IPropertySymbol property)
                    {
                        if (property.Type is INamedTypeSymbol)
                        {
                            formStringBuilder.AppendLine($"formContent.Add(new System.Net.Http.StringContent({parameterSymbol.Name}.{member.Name}.ToString()), \"{member.Name}\");");
                        }
                    }
                }
            }

            formStringBuilder.AppendLine("httpRequestMessage.Content = formContent;");
            formStringBuilder.AppendLine("httpRequestMessage.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(\"application/octet-stream\"));");
            this.FormString = formStringBuilder.ToString();
        }

        if (parameterAttributeDetails.HasQueryAttribute)
        {
            if (isPrimitive)
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
    }

    public string QueryString { get; }
    public bool IsRouteQueryParameter { get; set; }
    public string ComplexTypeString { get; }
    public string ParameterStringWithTypes { get; }
    public string ParameterStringWithoutTypes { get; }
    public string Name { get; }
    //public bool HasExplizitDefaultValue { get; }
    //public IParameterSymbol ParameterSymbol { get; }
    public bool IsNullable { get; }
    //public bool IsPrimitive { get; }
    public ParameterAttributeDetails AttributeDetails { get; }
    public Dictionary<string, string> HeaderKeyValues { get; }
    public string FormString { get; }
}
