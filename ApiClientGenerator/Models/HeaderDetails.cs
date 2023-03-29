using Microsoft.CodeAnalysis;

namespace ApiGenerator.Models;

public class HeaderDetails
{
    public HeaderDetails(string name, ITypeSymbol typeInformation)
    {
        this.Name = name;
        this.TypeInformation = typeInformation;
    }

    public string Name { get;}

    public ITypeSymbol TypeInformation { get;}
}
