using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiGenerator.Extensions;

public static class SymbolStringRepresentationExtensions
{
    public static IDictionary<string, string> GenerateClassString(this ITypeSymbol symbol)
    {
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

        if (symbol.TypeKind == TypeKind.Enum)
        {
            var enumString = symbol.GenerateEnumClassString();
            stringClassRepresentations.Add(className, enumString);
            return stringClassRepresentations;
        }

        

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
                    if (propertyTypeSymbol.TypeKind == TypeKind.Class || propertyTypeSymbol.TypeKind == TypeKind.Enum)
                    {
                        CheckAndGenerateClassString(propertyTypeSymbol, stringClassRepresentations);
                    }

                    if (propertyTypeSymbol.TypeArguments.Count() != 0)
                    {
                        foreach (var argument in propertyTypeSymbol.TypeArguments)
                        {
                            CheckAndGenerateClassString(argument, stringClassRepresentations);
                        }
                    }
                }

                var outputString = property.Type.ToString().SanitizeClassTypeString();
                
                // TODO check if set is available?
                classMemberBuilder.AppendFormat("{0} {1} {2} {{ get; set; }}", accessibility, outputString, property.Name);
                classMemberBuilder.AppendLine(Environment.NewLine);
            }
        }

        // this is for e.g. IEnumerable<T>
        if (symbol.TypeKind == TypeKind.Interface)
        {
            if (symbol is INamedTypeSymbol typeSymbol && typeSymbol.TypeArguments.Count() != 0)
            {
                foreach (var argument in typeSymbol.TypeArguments)
                {
                    CheckAndGenerateClassString(argument, stringClassRepresentations);
                }
            }

            return stringClassRepresentations;
        }

        var classCode = $$"""
            {{accessibility}} {{classModifiers}} {{classType}} {{className}}
            {
                {{classMemberBuilder}}
            }
            """;

        // TODO instead of class name, might use full name with namespace? in the future for assemblies
        stringClassRepresentations.Add(className, classCode);
        return stringClassRepresentations;
    }

    private static void CheckAndGenerateClassString(ITypeSymbol argument, IDictionary<string, string> stringClassRepresentations)
    {
        if (!argument.IsPrimitive())
        {
            var propertyClassTypeString = argument.GenerateClassString();

            foreach (var classStringRepresentation in propertyClassTypeString)
            {
                if (!stringClassRepresentations.ContainsKey(classStringRepresentation.Key))
                {
                    stringClassRepresentations.Add(classStringRepresentation.Key, classStringRepresentation.Value);
                }
            }
        }
    }

    public static string GenerateEnumClassString(this ITypeSymbol symbol)
    {
        string className = symbol.Name;
        var classMemberBuilder = new StringBuilder();

        foreach (var member in symbol.GetMembers())
        {
            if (member is IFieldSymbol)
            {
                classMemberBuilder.AppendLine($"{member.Name},");
            }
        }

        return $$"""
            public enum {{className}}
            {
                {{classMemberBuilder}}
            }
            """;
    }
}
