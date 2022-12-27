using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.CodeAnalysis.Editing;
using System.Collections.Generic;
using System.Linq;

namespace ApiGenerator
{
    public static class SemanticModelExtensions
    {
        public static bool ContainsControllerTypes(this SemanticModel semanticModel)
        {
            var controllerBase = semanticModel.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.ControllerBase");
            var controller = semanticModel.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.Controller");

            if (controllerBase is null && controller is null)
            {
                return false;
            }

            return true;
        }
    }

    public static class DeclarationSyntaxExtensions
    {
        public static string GetBaseRoute(this ClassDeclarationSyntax classDeclaration, List<AttributeSyntax> classAttributes)
        {
            var routeAttribute = classAttributes.SingleOrDefault(a => a.Name.ToString() == "Route");

            return routeAttribute?.ArgumentList?.Arguments.Single().ToString();
        }

        private static bool GetBaseRoute(this ClassDeclarationSyntax classDeclaration,
        IReadOnlyCollection<ClassDeclarationSyntax> classDeclarations)
        {
            var baseClass = classDeclaration;

            while (baseClass != null)
            {
                var baseTypes = baseClass.BaseList?.Types;
                var classAttributes = baseClass.AttributeLists.SelectMany(al => al.Attributes).ToList();

                if (classAttributes.Any(a => a.Name.ToString() == "ApiController"))
                {
                    return true;
                }

                var baseType = baseTypes?.SingleOrDefault(b => classDeclarations.Any(c => c.Identifier.Text == b.Type.ToString()));

                baseClass = classDeclarations.FirstOrDefault(c => c.Identifier.Text == baseType?.Type.ToString());
            }

            return false;
        }

        public static IEnumerable<ClassDeclarationSyntax> GetApiControllers(this IEnumerable<ClassDeclarationSyntax> classDeclarations)
        {
            return classDeclarations.Where(classDeclaration => classDeclaration.IsApiController(classDeclarations.ToList()));
        }

        public static IEnumerable<MethodDeclarationSyntax> GetPublicMethods(this IEnumerable<MethodDeclarationSyntax> methods) =>
            methods.Where(method => method.Modifiers.Any(s => s.ValueText == "public"));

        public static bool IsApiController(this ClassDeclarationSyntax classDeclaration,
        IReadOnlyCollection<ClassDeclarationSyntax> classDeclarations)
        {
            var hasApiControllerAttribute = false;
            var inheritsFromController = classDeclaration.InheritsFromController(classDeclarations);
            
            if (!inheritsFromController)
            {
                hasApiControllerAttribute = classDeclaration.HasApiControllerAttribute(classDeclarations);
            }

            return inheritsFromController || hasApiControllerAttribute;

        }

        private static bool HasApiControllerAttribute(this ClassDeclarationSyntax classDeclaration,
        IReadOnlyCollection<ClassDeclarationSyntax> classDeclarations)
        {
            var baseClass = classDeclaration;

            while (baseClass != null)
            {
                var baseTypes = baseClass.BaseList?.Types;
                var classAttributes = baseClass.AttributeLists.SelectMany(al => al.Attributes).ToList();

                if (classAttributes.Any(a => a.Name.ToString() == "ApiController")) 
                { 
                    return true; 
                }

                var baseType = baseTypes?.SingleOrDefault(b => classDeclarations.Any(c => c.Identifier.Text == b.Type.ToString()));

                baseClass = classDeclarations.FirstOrDefault(c => c.Identifier.Text == baseType?.Type.ToString());
            }

            return false;
        }


        // TODO do not directly copy from daniboi
        private static bool InheritsFromController(this ClassDeclarationSyntax classDeclaration,
        IReadOnlyCollection<ClassDeclarationSyntax> classDeclarations)
        {
            var baseClass = classDeclaration;

            while (baseClass != null)
            {
                var baseTypes = baseClass.BaseList?.Types;

                if (baseTypes?.Any(b => b.Type.ToString() == "Controller" || b.Type.ToString() == "ControllerBase") ?? false)
                {
                    return true;
                }

                var baseType = baseTypes?.SingleOrDefault(b => classDeclarations.Any(c => c.Identifier.Text == b.Type.ToString()));

                baseClass = classDeclarations.FirstOrDefault(c => c.Identifier.Text == baseType?.Type.ToString());
            }

            return false;
        }
    }
}

