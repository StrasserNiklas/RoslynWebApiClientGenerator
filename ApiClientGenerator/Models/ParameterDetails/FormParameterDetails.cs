using Microsoft.CodeAnalysis;
using System.Text;

namespace ApiClientGenerator.Models.ParameterDetails;

public class FormParameterDetails : ParameterDetails
{
    public FormParameterDetails(IParameterSymbol parameterSymbol, bool isPrimitive)
        : base(parameterSymbol)
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
        FormString = formStringBuilder.ToString();
    }

    public string FormString { get; }

}
