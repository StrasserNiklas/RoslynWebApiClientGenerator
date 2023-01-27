using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiGenerator.Extensions;

public static class SymbolStringRepresentationExtensions
{
    // TODO must unravel if necessary!
    public static IDictionary<string, string> GenerateClassString(this ITypeSymbol symbol)
    {
        var nameTyped = symbol as INamedTypeSymbol;
        //nameTyped.TypeArguments;

        // TODO I LEFT OFF HERE YESTERDAY

        //nameTyped.AllInterfaces;
        //symbol.AllInterfaces.First().MetadataName;

        string className = symbol.Name;
        string accessibility = symbol.DeclaredAccessibility.ToString().ToLowerInvariant();
        string classModifiers = string.Empty;

        if (symbol.IsAbstract)
        {
            classModifiers += "abstract ";
        }
        if (symbol.IsSealed)
        {
            classModifiers += "sealed ";
        }

        string classType = symbol.TypeKind.ToString().ToLowerInvariant();

        var stringClassRepresentations = new Dictionary<string, string>();

        var classMemberBuilder = new StringBuilder();

        foreach (var member in symbol.GetMembers())
        {
            if (member is IFieldSymbol field)
            {
                if (field.DeclaredAccessibility != Accessibility.Public)// || field.Name.StartsWith("get_") || field.Name.StartsWith("set_"))
                {
                    continue;
                }

                classMemberBuilder.AppendFormat("{0} {1} {2};", accessibility, field.Type, field.Name);
                classMemberBuilder.AppendLine(Environment.NewLine);
            }
            else if (member is IPropertySymbol property)
            {
                if (property.Type is INamedTypeSymbol propertyTypeSymbol)
                {
                    if (propertyTypeSymbol.TypeKind == TypeKind.Class)
                    {
                        if (!propertyTypeSymbol.IsPrimitive())
                        {
                            // TODO rename this
                            var propertyClassTypeString = GenerateClassString(propertyTypeSymbol);

                            foreach (var classStringRepresentation in propertyClassTypeString)
                            {
                                if (!stringClassRepresentations.ContainsKey(classStringRepresentation.Key))
                                {
                                    stringClassRepresentations.Add(classStringRepresentation.Key, classStringRepresentation.Value);
                                }
                            }
                        }
                    }
                    // TODO have fun with dictionaries
                    else if (propertyTypeSymbol.TypeKind == TypeKind.Interface)
                    {
                        var iii = propertyTypeSymbol.AllInterfaces;
                    }

                    
                }

                // TODO check if set is available?
                classMemberBuilder.AppendFormat("{0} {1} {2} {{ get; set; }}", accessibility, property.Type.ToString().Split('.').LastOrDefault() ?? string.Empty, property.Name);
                classMemberBuilder.AppendLine(Environment.NewLine);
            }
        }

        var classCode = $$"""
            {{accessibility}} {{classModifiers}} {{classType}} {{className}}
            {
                {{classMemberBuilder}}
            }
            """;

        // TODO instead of class name, might use full name with namespace?
        stringClassRepresentations.Add(className, classCode);
        return stringClassRepresentations;
    }
}
