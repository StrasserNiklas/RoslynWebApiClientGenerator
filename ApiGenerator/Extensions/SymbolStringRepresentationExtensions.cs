using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace ApiGenerator.Extensions;

public static class SymbolStringRepresentationExtensions
{
    // TODO use literal string instead of code builder!
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

        var codeBuilder = new CodeStringBuilder();
        codeBuilder.AppendFormat("{0} {1} {2} {3}", accessibility, classModifiers, classType, className);
        codeBuilder.OpenCurlyBracketLine();

        foreach (var member in symbol.GetMembers())
        {
            if (member is IFieldSymbol field)
            {
                if (field.DeclaredAccessibility != Accessibility.Public)// || field.Name.StartsWith("get_") || field.Name.StartsWith("set_"))
                {
                    continue;
                }

                codeBuilder.AppendFormat("{0} {1} {2};", accessibility, field.Type, field.Name);
                codeBuilder.AppendNewLine();
            }
            else if (member is IPropertySymbol property)
            {
                if (property.Type is INamedTypeSymbol propertyTypeSymbol && propertyTypeSymbol.TypeKind == TypeKind.Class)
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

                // TODO check if set is available?
                codeBuilder.AppendFormat("{0} {1} {2} {{ get; set; }}", accessibility, property.Type.ToString().Split('.').LastOrDefault() ?? string.Empty, property.Name);
                codeBuilder.AppendNewLine();
            }
        }

        codeBuilder.CloseCurlyBracketLine();
        codeBuilder.AppendNewLine();

        // TODO instead of class name, might use full name with namespace?
        stringClassRepresentations.Add(className, codeBuilder.ToString());
        return stringClassRepresentations;
    }
}
