using ApiGenerator.Extensions;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace ApiGenerator.Models;

public class ParameterDetails
{
    public ParameterDetails(IParameterSymbol parameterSymbol, bool isPrimitive, ParameterAttributeDetails parameterAttributeDetails, List<string> headerKeys)
    {
        this.Name = parameterSymbol.Name;
        this.HasExplizitDefaultValue = parameterSymbol.HasExplicitDefaultValue;
        this.ParameterSymbol = parameterSymbol;
        this.IsPrimitive = isPrimitive;
        this.AttributeDetails = parameterAttributeDetails;
        this.HeaderKeys = headerKeys;

        this.ParameterTypeString = parameterSymbol.Type.CheckAndSanitizeClassString();

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
                            formStringBuilder.AppendLine($"formContent.Add(new System.Net.Http.StringContent{parameterSymbol.Name}.{member.Name}.ToString()), \"{member.Name}\");");
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
    public string ParameterTypeString { get; }
    public string Name { get; }
    public bool HasExplizitDefaultValue { get; }
    public IParameterSymbol ParameterSymbol { get; }
    public bool IsPrimitive { get; }
    public ParameterAttributeDetails AttributeDetails { get; }
    public List<string> HeaderKeys { get; }
    public string FormString { get; }
}
