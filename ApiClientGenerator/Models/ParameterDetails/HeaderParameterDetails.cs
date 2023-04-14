using ApiGenerator.Extensions;
using ApiGenerator.Models;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace ApiClientGenerator.Models.ParameterDetails;

public class HeaderParameterDetails : ParameterDetails
{
    public HeaderParameterDetails(IParameterSymbol parameterSymbol, bool isSimpleType, Dictionary<string, HeaderDetails> headerKeyValues)
        : base(parameterSymbol)
    {
        HeaderKeyValues = new Dictionary<string, string>();

        // should you also check if the symbol even has the attribute?
        var memberCount = parameterSymbol.Type.GetMembers().Count(s => s is IPropertySymbol);

        if (isSimpleType)
        {
            HeaderKeyValues.Add(headerKeyValues.First().Key, headerKeyValues.First().Value.Name);
        }
        else if (memberCount != headerKeyValues.Count && headerKeyValues.Count > 0)
        {
            // set the parameter different
            IsNullable = false;
            ParameterStringWithTypes = string.Empty;
            ParameterStringWithoutTypes = string.Empty;

            foreach (var header in headerKeyValues)
            {
                var name = char.ToLowerInvariant(header.Value.Name[0]) + header.Value.Name.Substring(1);
                ParameterStringWithTypes += $"{header.Value.TypeInformation.CheckAndSanitizeClassString()} {name}, ";
                ParameterStringWithoutTypes += $"{name}, ";
                HeaderKeyValues.Add(header.Key, name);
            }

            ParameterStringWithTypes = ParameterStringWithTypes.TrimEnd(',', ' ');
            ParameterStringWithoutTypes = ParameterStringWithoutTypes.TrimEnd(',', ' ');
        }
        else
        {
            foreach (var header in headerKeyValues)
            {
                HeaderKeyValues.Add(header.Key, ParameterName + '.' + header.Value.Name);
            }
        }
    }

    public Dictionary<string, string> HeaderKeyValues { get; }
}
