using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Resources;

namespace ApiGenerator 
{
    [Generator]
    public class ApiSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
            Debug.WriteLine("Initalize code generator");
        }

        public ApiSourceGenerator() { }
        public void Execute(GeneratorExecutionContext context)
        {
            var assemblyClasses = context.Compilation.SyntaxTrees.SelectMany(
                x => x.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>());

            var apiControllers = assemblyClasses.GetApiControllers();

            foreach (var classDeclaration in apiControllers)
            {
                var classAttributes = classDeclaration.AttributeLists.SelectMany(al => al.Attributes).ToList();

                //var baseRoute = classDeclaration.GetBaseRoute(classAttributes);

                var authorizeAttribute = classAttributes.SingleOrDefault(a => a.Name.ToString() == "Authorize");

                var parameters = classDeclaration.TypeParameterList;
                var tree = classDeclaration.SyntaxTree;
                //IEnumerable<(TypeSyntax requestType, TypeSyntax responseType)> endpoints = Analyz.FindEndpoints(tree.GetRoot());

                //var syntaxGenerator = SyntaxGenerator.GetGenerator(new AdhocWorkspace(), LanguageNames.CSharp);
                //var generatedCode = syntaxGenerator.GenerateTypeDeclaration(classDeclaration);

                var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>().GetPublicMethods();
                var route = classDeclaration.GetBaseRoute(classAttributes);

                Debug.WriteLine("---------------------------------------------");
                Debug.WriteLine("Class: " + classDeclaration.Identifier.ToFullString());
                Debug.WriteLine("Route: " + route);
                Debug.WriteLine("Methods:");

                foreach (var method in methods)
                {
                    Debug.WriteLine("***********");

                    // Get the request and response types for the endpoint
                    var methodParameters = method.ParameterList.Parameters;

                    var parameterStringified = string.Join(string.Empty, methodParameters.Select(parameter => parameter.Type.ToFullString() + parameter.Identifier.ValueText));

                    TypeSyntax requestType = method.ParameterList.Parameters
                        .Select(p => p.Type)
                        .FirstOrDefault();
                    TypeSyntax responseType = method.ReturnType;

                    // Add the endpoint to the list
                    //_endpoints.Add((requestType, responseType));

                    Debug.WriteLine($"public {responseType.ToFullString().Trim()} {method.Identifier.ToFullString().Trim()}({parameterStringified.Trim()})");

                    Debug.WriteLine("Request class(es) used:");

                    foreach (var parameter in methodParameters)
                    {
                        var parameterClass = assemblyClasses.SingleOrDefault(classInstance => classInstance.Identifier.ValueText == parameter.Identifier.ValueText);
                        var propertyTypes = classDeclaration?
                            .DescendantNodes().OfType<PropertyDeclarationSyntax>()
                            .Where(p => p.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                            .Select(p => p.Type)
                            .ToList() ?? new List<TypeSyntax>();

                        Debug.WriteLine(@"

");

                    }

                    Debug.WriteLine("Response class(es) used:");

                    var res = method.ReturnType;
                    //res.
                    
                }

                //    foreach (var method in publicMethods)
                //    {
                //        var routeAttribute = method.AttributeLists.SelectMany(al => al.Attributes)
                //.SingleOrDefault(a => a.Name.ToString() == "Route")?.ArgumentList?.Arguments.FirstOrDefault();
                //        // Get the request and response types for the endpoint
                //        TypeSyntax requestType = method.ParameterList.Parameters
                //            .Select(p => p.Type)
                //            .FirstOrDefault();
                //        TypeSyntax responseType = method.ReturnType;

                //        // Add the endpoint to the list
                //        _endpoints.Add((requestType, responseType));
                //    }
            }
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

