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
        //MetadataReference.
        var assembly = symbol.ContainingAssembly;

        var stringClassRepresentations = new Dictionary<string, string>();
        string className = symbol.Name;
        string accessibility = symbol.DeclaredAccessibility.ToString().ToLowerInvariant();
        string classModifiers = string.Empty;

        if (symbol.IsAbstract && symbol.TypeKind != TypeKind.Interface)
        {
            classModifiers += "abstract ";
        }
        if (symbol.IsSealed) // be aware of outside DLLs
        {
            //classModifiers += "sealed ";
            //return stringClassRepresentations;
        }

        string classType = symbol.TypeKind.ToString().ToLowerInvariant();

        // TODO keep an eye on this
        if (symbol.TypeKind == TypeKind.Struct || symbol.IsPrimitive())
        {
            return stringClassRepresentations;
        }

        if (symbol.TypeKind == TypeKind.Enum)
        {
            var enumString = symbol.GenerateEnumClassString();
            stringClassRepresentations.Add(className, enumString);
            return stringClassRepresentations;
        }

        // this is for e.g. IEnumerable<T>
        //if (symbol.TypeKind == TypeKind.Interface)
        //{
        //    if (symbol is INamedTypeSymbol typeSymbol && typeSymbol.TypeArguments.Count() != 0)
        //    {
        //        foreach (var argument in typeSymbol.TypeArguments)
        //        {
        //            CheckAndGenerateClassString(argument, stringClassRepresentations);
        //        }
        //    }

        //    return stringClassRepresentations;
        //}

        // TODO what about classes that inherit from it shrug
        if (symbol.ContainingNamespace.ToString().Contains("System.Collections") && symbol is INamedTypeSymbol namedTypeSymbol)
        {
            foreach (var argument in namedTypeSymbol.TypeArguments)
            {
                CheckAndGenerateClassString(argument, stringClassRepresentations);
            }

            return stringClassRepresentations;
        }

        var classMemberBuilder = new StringBuilder();

        foreach (var member in symbol.GetMembers())
        {
            if (member is IFieldSymbol field)
            {
                if (field.DeclaredAccessibility != Accessibility.Public)
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
                    if (propertyTypeSymbol.ToString().Contains("claim"))
                    {

                    }

                    if (propertyTypeSymbol.TypeKind == TypeKind.Class || propertyTypeSymbol.TypeKind == TypeKind.Enum || propertyTypeSymbol.TypeKind == TypeKind.Interface)
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

                // TODO check if set is available? <- what does he mean?
                classMemberBuilder.AppendFormat("{0} {1} {2} {{ get; set; }}", accessibility, outputString, property.Name);
                classMemberBuilder.AppendLine(Environment.NewLine);
            }
            else if (member is IMethodSymbol methodSymbol)
            {
                // TODO arrays...
                if (methodSymbol.ReturnType is IArrayTypeSymbol arrayTypeSymbol)
                {
                    CheckAndGenerateClassString(arrayTypeSymbol.ElementType, stringClassRepresentations);
                }
            }
            else
            {
                if (symbol is INamedTypeSymbol typeSymbol && typeSymbol.TypeArguments.Count() != 0)
                {
                    foreach (var argument in typeSymbol.TypeArguments)
                    {
                        CheckAndGenerateClassString(argument, stringClassRepresentations);
                    }
                }
            }

            //((Microsoft.CodeAnalysis.IArrayTypeSymbol)((Microsoft.CodeAnalysis.IMethodSymbol)member).ReturnType).ElementType
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
