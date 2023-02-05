﻿using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
        var isGenericType = false;
        var addedGenericProperties = new List<string>();
        var genericPropertyStringBuilder = new StringBuilder();
        var genericConstructorStringBuilder = new StringBuilder();
        var genericConstructorAssignmentStringBuilder = new StringBuilder();
        var genericClassName = string.Empty;

        if (symbol is INamedTypeSymbol namedTypeSymbolWithArguments && namedTypeSymbolWithArguments.IsGenericType)
        {
            isGenericType = namedTypeSymbolWithArguments.IsGenericType;
            genericClassName = namedTypeSymbolWithArguments.OriginalDefinition.ToString().Split('.').Last();
            var declaration = symbol.DeclaringSyntaxReferences.FirstOrDefault();

            foreach (var argument in namedTypeSymbolWithArguments.TypeArguments)
            {
                CheckAndGenerateClassString(argument, stringClassRepresentations);
            }

            // this will only work if the type is contained in the API assembly (not in project references etc.)
            if (declaration is not null)
            {
                var syntax = declaration.GetSyntax();
                var genericCodeSyntax = syntax.ToFullString();
                stringClassRepresentations.Add(genericClassName, genericCodeSyntax);
                return stringClassRepresentations;
            }
            else
            {
                var args = namedTypeSymbolWithArguments.GetMembers();
                
                // generate the generic type
                foreach (var mem in args)
                {
                    if (mem is IPropertySymbol propertySymbol)
                    {
                        var propertyName = propertySymbol.Name;
                        var match = Regex.Match(genericClassName, @"\<(.*?)\>");

                        string genericParameter = string.Empty;

                        if (match.Success && match.Groups.Count > 1)
                        {
                            genericParameter = Regex.Match(genericClassName, @"\<(.*?)\>").Groups[1].Value;
                        }

                        var generics = genericParameter.Split(',').Select(x => x.Trim());

                        var returnTypeString = propertySymbol.OriginalDefinition.GetMethod.ReturnType.ToString();

                        if (generics.Contains(returnTypeString))
                        {
                            genericPropertyStringBuilder.AppendLine($$"""
                                public {{returnTypeString}} {{propertyName}} { get; set; }
                                """);

                            genericConstructorStringBuilder.Append($"{returnTypeString} {propertyName},");
                            genericConstructorAssignmentStringBuilder.AppendLine($"this.{propertyName} = {propertyName};");
                            addedGenericProperties.Add(propertyName);
                        }
                    }
                }
            }
        }

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
                if (addedGenericProperties.Contains(member.Name))
                {
                    continue;
                }

                if (property.Type is INamedTypeSymbol propertyTypeSymbol)
                {
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

                var outputString = property.Type.SanitizeClassTypeString();

                // TODO check if set is available? <- what does he mean?
                classMemberBuilder.AppendFormat("{0} {1} {2} {{ get; set; }}", accessibility, outputString, property.Name);
                classMemberBuilder.AppendLine(Environment.NewLine);
            }
            // handle arrays
            else if (member is IMethodSymbol methodSymbol)
            {
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
        }

        var genericCode = $$"""
                public class {{genericClassName}}
                {
                    public {{className}}({{genericConstructorStringBuilder.ToString().TrimEnd(',')}})
                    {
                        {{genericConstructorAssignmentStringBuilder}}
                    }

                    {{genericPropertyStringBuilder}}
                    {{classMemberBuilder}}
                }
                """;

        var classCode = $$"""
            {{accessibility}} {{classModifiers}} {{classType}} {{className}}
            {
                {{classMemberBuilder}}
            }
            """;

        if (isGenericType)
        {
            stringClassRepresentations.Add(className, genericCode);
            return stringClassRepresentations;
        }
        
        stringClassRepresentations.Add(className, classCode);
        return stringClassRepresentations;
    }

    private static void CheckAndGenerateClassString(ITypeSymbol argument, IDictionary<string, string> stringClassRepresentations)
    {
        if (!argument.IsPrimitive())
        {
            if (argument.ToString() == "object" || argument.ToString() == "object?")
            {
                return;
            }

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
