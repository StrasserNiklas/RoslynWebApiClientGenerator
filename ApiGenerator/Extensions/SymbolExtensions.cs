﻿using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace ApiGenerator.Extensions;

public static class SymbolExtensions
{
    public static AttributeData GetRouteAttribute(this ISymbol symbol)
    {
        return symbol.GetAttribute("Microsoft.AspNetCore.Mvc.RouteAttribute");
    }

    public static AttributeData GetAttribute(this ISymbol symbol, string identifier)
    {
        var occurences = symbol
            .GetAttributes()
            .Where(attr => attr.AttributeClass?.ToString() == identifier);

        if (occurences.Count() > 1)
        {
            // warning to use attribute only once
            // maybe only needed for certain attribute e.g. routes
        }

        return occurences.FirstOrDefault();
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

    private static bool InheritsFromController(INamedTypeSymbol classSymbol, List<INamedTypeSymbol> controllerBaseTypes)
    {
        var currentClassSymbol = classSymbol;

        while (currentClassSymbol.BaseType != null)
        {
            if (controllerBaseTypes.Any(type => currentClassSymbol.BaseType.Equals(type, SymbolEqualityComparer.Default)))
            {
                return true;
            }

            currentClassSymbol = currentClassSymbol.BaseType;
        }

        return false;
    }

    // TODO change this or attribute where it is from
    public static bool IsPrimitive(this ITypeSymbol typeSymbol)
    {
        switch (typeSymbol.SpecialType)
        {
            case SpecialType.System_Boolean:
            case SpecialType.System_SByte:
            case SpecialType.System_Int16:
            case SpecialType.System_Int32:
            case SpecialType.System_Int64:
            case SpecialType.System_Byte:
            case SpecialType.System_UInt16:
            case SpecialType.System_UInt32:
            case SpecialType.System_UInt64:
            case SpecialType.System_Single:
            case SpecialType.System_Double:
            case SpecialType.System_Char:
            case SpecialType.System_String:
                return true;
        }

        switch (typeSymbol.TypeKind)
        {
            case TypeKind.Enum:
                return true;
        }

        return false;
    }
}