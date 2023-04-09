using ApiClientGenerator.Configuration;
using ApiGenerator.Models;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ApiGenerator.Extensions;

public static class SymbolStringRepresentationExtensions
{

    // TODO get alle types of semantic models oder so und dass dann verwenden bei der generierung von models von der eigenenn api
    // there is a method for that, getting all types of an assembly or something
    public static ClassGenerationDetails GenerateClassString(this ITypeSymbol symbol)
    {
        var classGenerationDetails = new ClassGenerationDetails();

        var assembly = symbol.ContainingAssembly;
        var symbolNamespace = symbol.ContainingNamespace.ToString();

        // generics like lists?...
        if (Configuration.UseExternalAssemblyContracts)
        {
            // DataResponse<T>
            // List<T>

            if (symbol is INamedTypeSymbol genericSymbol && genericSymbol.IsGenericType)
            {
                foreach (var argument in genericSymbol.TypeArguments)
                {
                    if (Configuration.ProjectAssemblyNamespaces.Contains(argument.ContainingNamespace.ToString()))
                    {
                        CheckAndGenerateClassString(argument, classGenerationDetails);
                    }
                }
            }

            if (!Configuration.ProjectAssemblyNamespaces.Contains(symbolNamespace))
            {
                classGenerationDetails.AdditionalUsings.Add(symbolNamespace);
                return classGenerationDetails;
            }
        }

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

        // keep an eye on this
        if (symbol.TypeKind == TypeKind.Struct || symbol.IsSimpleType())
        {
            return classGenerationDetails;
        }

        if (symbol.TypeKind == TypeKind.Enum)
        {
            var enumString = symbol.GenerateEnumClassString();
            classGenerationDetails.GeneratedCodeClasses.Add(className, enumString);
            return classGenerationDetails;
        }

        // TODO what about classes that inherit from it
        // maybe have a list of namespaces that are "not nice"
        if (symbol.ContainingNamespace.ToString().Contains("System.Collections") && symbol is INamedTypeSymbol namedTypeSymbol)
        {
            foreach (var argument in namedTypeSymbol.TypeArguments)
            {
                CheckAndGenerateClassString(argument, classGenerationDetails);
            }

            return classGenerationDetails;
        }

        var classMemberBuilder = new StringBuilder();
        var isGenericType = false;
        var addedGenericProperties = new List<string>();
        var genericPropertyStringBuilder = new StringBuilder();
        var genericConstructorStringBuilder = new StringBuilder();
        var genericConstructorAssignmentStringBuilder = new StringBuilder();
        var genericClassName = string.Empty;

        // generics
        if (symbol is INamedTypeSymbol namedTypeSymbolWithArguments && namedTypeSymbolWithArguments.IsGenericType)
        {
            isGenericType = namedTypeSymbolWithArguments.IsGenericType;
            genericClassName = namedTypeSymbolWithArguments.OriginalDefinition.ToString().Split('.').Last();
            var declaration = symbol.DeclaringSyntaxReferences.FirstOrDefault();

            foreach (var argument in namedTypeSymbolWithArguments.TypeArguments)
            {
                CheckAndGenerateClassString(argument, classGenerationDetails);
            }

            // this will only work if the type is contained in the API assembly (not in project references etc.)
            if (declaration is not null)
            {
                var syntax = declaration.GetSyntax();
                var genericCodeSyntax = syntax.ToFullString();
                classGenerationDetails.GeneratedCodeClasses.Add(genericClassName, genericCodeSyntax);
                return classGenerationDetails;
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

        // TODO check if we can do the same all the time for response/requests classes from the API itself like above
        // var declaration = symbol.DeclaringSyntaxReferences.FirstOrDefault();
        // var syntax = declaration.GetSyntax();
        // var genericCodeSyntax = syntax.ToFullString();
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
                    if (propertyTypeSymbol.TypeKind is TypeKind.Class or TypeKind.Enum or TypeKind.Interface)
                    {
                        //if (useExternalAssemblyContracts)
                        //{
                        //    var propertySymbolNamespace = propertyTypeSymbol.ContainingNamespace.ToString();

                        //    if (projectAssemblyNamespaces.Contains(propertySymbolNamespace))
                        //    {
                        //        CheckAndGenerateClassString(propertyTypeSymbol, classGenerationDetails, useExternalAssemblyContracts, projectAssemblyNamespaces);
                        //    }
                        //    else
                        //    {
                        //        classGenerationDetails.AdditionalUsings.Add(propertySymbolNamespace);
                        //    }
                        //}
                        //else
                        //{
                        //    CheckAndGenerateClassString(propertyTypeSymbol, classGenerationDetails, useExternalAssemblyContracts, projectAssemblyNamespaces);
                        //}
                        CheckAndGenerateClassString(propertyTypeSymbol, classGenerationDetails);
                    }


                    // e.g. Nullable<T>
                    if (propertyTypeSymbol.IsGenericType)
                    {
                        foreach (var argument in propertyTypeSymbol.TypeArguments)
                        {
                            CheckAndGenerateClassString(argument, classGenerationDetails);
                        }
                    }
                }

                if (property.Name == "Parameters")
                {

                }

                var outputString = property.Type.CheckAndSanitizeClassString();

                classMemberBuilder.AppendFormat("{0} {1} {2} {{ get; set; }}", accessibility, outputString, property.Name);
                classMemberBuilder.AppendLine(Environment.NewLine);
            }
            else if (member is IMethodSymbol methodSymbol)
            {
                if (methodSymbol.ReturnType is IArrayTypeSymbol arrayTypeSymbol)
                {
                    CheckAndGenerateClassString(arrayTypeSymbol.ElementType, classGenerationDetails);
                }
            }
            else
            {
                if (symbol is INamedTypeSymbol typeSymbol && typeSymbol.TypeArguments.Count() != 0)
                {
                    foreach (var argument in typeSymbol.TypeArguments)
                    {
                        CheckAndGenerateClassString(argument, classGenerationDetails);
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
            classGenerationDetails.GeneratedCodeClasses.Add(className, genericCode);
            return classGenerationDetails;
        }

        classGenerationDetails.GeneratedCodeClasses.Add(className, classCode);
        return classGenerationDetails;
    }

    private static void CheckAndGenerateClassString(ITypeSymbol argument, ClassGenerationDetails classGenerationDetails)
    {
        if (!argument.IsSimpleType())
        {
            if (argument.ToString() == "object?")
            {
                return;
            }

            var classGenerationDetailResult = argument.GenerateClassString();

            foreach (var classStringRepresentation in classGenerationDetailResult.GeneratedCodeClasses)
            {
                if (!classGenerationDetails.GeneratedCodeClasses.ContainsKey(classStringRepresentation.Key))
                {
                    classGenerationDetails.GeneratedCodeClasses.Add(classStringRepresentation.Key, classStringRepresentation.Value);
                }
            }

            foreach (var usingString in classGenerationDetailResult.AdditionalUsings)
            {
                if (!classGenerationDetails.AdditionalUsings.Contains(usingString))
                {
                    classGenerationDetails.AdditionalUsings.Add(usingString);
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
