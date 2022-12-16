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
            Debug.WriteLine("Execute code generator");
            //System.Diagnostics.Debugger.Launch();
            var trees = context.Compilation.SyntaxTrees;

            var endpoints = new List<(TypeSyntax requestType, TypeSyntax responseType)>();

            //foreach (var treee in trees)
            //{
            //    endpoints.AddRange(Analyz.FindEndpoints(treee.GetRoot()));
            //}


            var assemblyClasses = context.Compilation.SyntaxTrees.SelectMany(
                x => x.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>());

            var apiControllers = assemblyClasses.GetApiControllers();

            // TODO find a way to couple parents and children to reduce time searching

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

                foreach (var method in methods)
                {
                    Debug.WriteLine($"{classDeclaration.Identifier.ToFullString()}: {method.Identifier.ToFullString()}");
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

    public class Analyz : CSharpSyntaxWalker
    {
        private readonly List<(TypeSyntax requestType, TypeSyntax responseType)> _endpoints =
            new List<(TypeSyntax requestType, TypeSyntax responseType)>();

        public static IEnumerable<(TypeSyntax requestType, TypeSyntax responseType)> FindEndpoints(SyntaxNode root)
        {
            var analyzer = new Analyz();
            analyzer.Visit(root);
            return analyzer._endpoints;
        }

        public override void VisitAttribute(AttributeSyntax node)
        {
            base.VisitAttribute(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            //if (IsController(node))
            //{
                // Get the methods in the class
                var methods = node.Members.OfType<MethodDeclarationSyntax>();
                var publicMethods = methods.GetPublicMethods();

                // Find the endpoints in the methods
                foreach (var method in publicMethods)
                {
                    var routeAttribute = method.AttributeLists.SelectMany(al => al.Attributes)
            .SingleOrDefault(a => a.Name.ToString() == "Route")?.ArgumentList?.Arguments.FirstOrDefault();
                    // Get the request and response types for the endpoint
                    TypeSyntax requestType = method.ParameterList.Parameters
                        .Select(p => p.Type)
                        .FirstOrDefault();
                    TypeSyntax responseType = method.ReturnType;

                    // Add the endpoint to the list
                    _endpoints.Add((requestType, responseType));
                }
            //}

            base.VisitClassDeclaration(node);
        }

        private static bool IsController(ClassDeclarationSyntax node)
        {
            //var baseTypes = node.BaseList?.Types;


            var classAttributes = node.AttributeLists.SelectMany(al => al.Attributes).ToList();

            var hasApiControllerAttribute = classAttributes.Any(a => a.Name.ToString() == "ApiController");
            //var isController = baseTypes?.Any(t => t.Type.ToString() == "ControllerBase" || t.Type.ToString() == "Controller") ?? false;

            //if (isController)
            //{
            //    return true;
            //}

            //baseTypes?.Node
            //IsController(wat);
            //var baseType = baseTypes?.SingleOrDefault(b => classDeclarations.Any(c => c.Identifier.Text == b.Type.ToString()));

            //var baseClass = classDeclarations.FirstOrDefault(c => c.Identifier.Text == baseType?.Type.ToString());

            return hasApiControllerAttribute;
        }


    }

    

    public static class DeclarationSyntaxExtensions
    {
        //public static string GetBaseRoute(this ClassDeclarationSyntax classDeclaration, List<AttributeSyntax> classAttributes)
        //{
        //    var routeAttribute = classAttributes.SingleOrDefault(a => a.Name.ToString() == "Route");


        //    var baseRoute = routeAttribute?.ArgumentList?.Arguments.Single();
        //}

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

