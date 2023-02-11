using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace ApiGenerator.Extensions;

public static class SymbolExtensions
{
    public static AttributeData GetRouteAttribute(this ISymbol symbol)
    {
        return symbol.GetAttribute("Microsoft.AspNetCore.Mvc.RouteAttribute");
    }

    public static (HttpMethod, AttributeData) GetHttpMethodWithAtrributeData(this ISymbol symbol)
    {
        var httpMethods = new Dictionary<string, HttpMethod>()
        {
            { "HttpGetAttribute",  HttpMethod.Get },
            { "HttpPutAttribute",  HttpMethod.Put },
            { "HttpPostAttribute",  HttpMethod.Post },
            { "HttpDeleteAttribute",  HttpMethod.Delete }
        };

        var attributes = symbol.GetAttributes();

        var httpAttribute = attributes.SingleOrDefault(x => httpMethods.Keys.Contains(x.AttributeClass.Name));

        if (httpAttribute is not null)
        {
            httpMethods.TryGetValue(httpAttribute.AttributeClass.Name, out var httpMethod);
            return (httpMethod, httpAttribute);
        }
        
        return (null, null);
    }

    public static AttributeData GetAttribute(this ISymbol symbol, string identifier)
    {
        var occurences = symbol
            .GetAttributes()
            .Where(attr => attr.AttributeClass?.ToString() == identifier);

        if (occurences.Count() > 1)
        {
            // could diagnostic here
            // warning to use attribute only once
            // maybe only needed for certain attribute e.g. routes
        }

        return occurences.FirstOrDefault();
    }

    public static IEnumerable<AttributeData> GetAttributes(this ISymbol symbol, string identifier)
    {
        return symbol
            .GetAttributes()
            .Where(attr => attr.AttributeClass?.ToString() == identifier);
    }

    public static bool IsApiController(this INamedTypeSymbol classDeclaration, SemanticModel semantic)
    {
        var controllerBase = semantic.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.ControllerBase");
        var controller = semantic.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.Controller");

        var hasApiControllerAttribute = false;
        var inheritsFromController = InheritsFromController(classDeclaration, new List<INamedTypeSymbol>() { controllerBase, controller });

        if (!inheritsFromController)
        {
            hasApiControllerAttribute = classDeclaration.GetAttribute("Microsoft.AspNetCore.Mvc.ApiControllerAttribute") != null;
        }

        return inheritsFromController || hasApiControllerAttribute;
    }

    private static bool InheritsFromController(INamedTypeSymbol classSymbol, List<INamedTypeSymbol> baseTypes)
    {
        var currentClassSymbol = classSymbol;

        while (currentClassSymbol.BaseType != null)
        {
            if (baseTypes.Any(type => currentClassSymbol.BaseType.Equals(type, SymbolEqualityComparer.Default)))
            {
                return true;
            }

            currentClassSymbol = currentClassSymbol.BaseType;
        }

        return false;
    }

    // also maybe need more like here: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-7.0#simple-types
    // think about structs
    public static bool IsSimpleType(this ITypeSymbol typeSymbol)
    {
        switch (typeSymbol.SpecialType)
        {
            case SpecialType.None: 
                return false;

            case SpecialType.System_Boolean:
            case SpecialType.System_SByte:
            case SpecialType.System_Byte:
            case SpecialType.System_Int16:
            case SpecialType.System_Int32:
            case SpecialType.System_Int64:
            case SpecialType.System_UInt16:
            case SpecialType.System_UInt32:
            case SpecialType.System_UInt64:
            case SpecialType.System_Single:
            case SpecialType.System_Double:
            case SpecialType.System_Char:
            case SpecialType.System_String:
            case SpecialType.System_Decimal:
            case SpecialType.System_DateTime:
            case SpecialType.System_Object:
                return true;

            default:
                return false;
        }
    }

    public static string CheckAndSanitizeClassString(this ITypeSymbol typeSymbol)
    {
        if (Configuration.UseExternalAssemblyContracts)
        {
            if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                typeSymbol = arrayTypeSymbol.ElementType;
            }

            if (Configuration.ProjectAssemblyNamespaces.Contains(typeSymbol.ContainingNamespace.ToString()))
            {
                return typeSymbol.SanitizeClassTypeString();
            }
            else if (typeSymbol is INamedTypeSymbol namedTypeSymbol &&  namedTypeSymbol.IsGenericType)
            {
                return typeSymbol.SanitizeClassTypeString();
            }
            else
            {
                return typeSymbol.ToString();
            }
        }
        else
        {
            return typeSymbol.SanitizeClassTypeString();
        }
    }
    
    public static string SanitizeClassTypeString(this ISymbol symbol)
    {
        if (symbol is null)
        {
            return string.Empty;
        }

        var typeString = symbol.Name;

        if (symbol is ITypeSymbol typeSymbol)
        {
            if (typeSymbol.IsSimpleType())
            {
                typeString = typeSymbol.ToString();
                return typeString;
            }
        }

        if (symbol is INamedTypeSymbol namedTypeSymbol)
        {
            if (typeString == "Nullable")
            {
                typeString = namedTypeSymbol.TypeArguments.First().SanitizeClassTypeString();
                typeString += "?";
                return typeString;
            }

            if (namedTypeSymbol.TypeArguments.Count() != 0)
            {
                typeString += "<";

                for (int i = 0; i < namedTypeSymbol.TypeArguments.Count(); i++)
                {
                    typeString += namedTypeSymbol.TypeArguments[i].CheckAndSanitizeClassString();//SanitizeClassTypeString();

                    if (i != namedTypeSymbol.TypeArguments.Count() - 1)
                    {
                        typeString += ", ";
                    }
                }

                typeString += ">";
            }
        }

        if (symbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            typeString += arrayTypeSymbol.ElementType.SanitizeClassTypeString() + "[]";
        }

        return typeString;
    }

    public static bool IsNullable(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol.Name == "Nullable")
        {
            return true;
        }

        if (typeSymbol.IsSimpleType())
        {
            return typeSymbol.IsNullableValueType();
        }

        if (typeSymbol.TypeKind == TypeKind.Struct)
        {
            return typeSymbol.IsNullableValueType();
        }

        return typeSymbol.IsNullableObject();
    }
}
