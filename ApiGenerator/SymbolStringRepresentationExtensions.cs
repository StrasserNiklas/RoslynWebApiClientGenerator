using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.Editing;
using System.Text;

namespace ApiGenerator
{
    //public static class SymbolStringRepresentationExtensions
    //{
    //    public static string GenerateClassString(this INamedTypeSymbol symbol)
    //    {
    //        // Step 1: Get the name of the class
    //        string className = symbol.Name;

    //        // Step 2: Determine the accessibility of the class
    //        string accessibility = symbol.DeclaredAccessibility.ToString().ToLowerInvariant();

    //        // Step 3: Determine if the class is abstract or sealed
    //        string classModifiers = string.Empty;
    //        if (symbol.IsAbstract)
    //        {
    //            classModifiers += "abstract ";
    //        }
    //        if (symbol.IsSealed)
    //        {
    //            classModifiers += "sealed ";
    //        }

    //        // Step 4: Determine the type of the class
    //        string classType = symbol.TypeKind.ToString().ToLowerInvariant();

    //        // Step 5: Get the members of the class
    //        var members = symbol.GetMembers();

    //        // Step 6: Generate a string representation for each member
    //        StringBuilder sb = new StringBuilder();
    //        foreach (var member in members)
    //        {
    //            if (member is IFieldSymbol field)
    //            {
    //                // Generate string for field: "public int MyField;"
    //                sb.AppendFormat("{0} {1} {2};", accessibility, field.Type, field.Name);
    //                sb.AppendLine();
    //            }
    //            else if (member is IPropertySymbol property)
    //            {
    //                // Check if the property is a class
    //                if (property.Type is INamedTypeSymbol propertyTypeSymbol && propertyTypeSymbol.TypeKind == TypeKind.Class)
    //                {
    //                    // Generate a string representation of the property's class type
    //                    string propertyClassTypeString = GenerateClassString(propertyTypeSymbol);

    //                    // Append the property's class type string to the string builder
    //                    sb.Append(propertyClassTypeString);
    //                }

    //                // Generate string for property: "public int MyProperty { get; set; }"
    //                sb.AppendFormat("{0} {1} {2} {{ get; set; }}", accessibility, property.Type, property.Name);
    //                sb.AppendLine();
    //            }
    //            // Add additional code here to handle other types of members (methods, events, etc.)
    //        }

    //        // Assemble the final string for the class
    //        string classString = string.Format("{0} {1} {2} {3} {{\n{4}\n}}", accessibility, classModifiers, classType, className, sb.ToString());
    //        return classString;
    //    }
    //}
}

