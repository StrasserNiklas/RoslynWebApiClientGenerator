using ApiGenerator.Extensions;
using ApiGenerator.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ApiGenerator.ClientGenerators;

public class CSharpClientGenerator : ClientGeneratorBase
{
    public CSharpClientGenerator(Configuration configuration, string projectName) : base(configuration, projectName) { }

    public override void GenerateClient(IEnumerable<ControllerClientDetails> controllerClientDetails)
    {
        // TODO for now place all clients into single file
        // if all clients are in one file, merge its generated (possible duplicates) classes 

        var mergedCodeClasses = controllerClientDetails
            .Select(client => client.GeneratedCodeClasses)
            .Merge();

        // beginning of file
        // name of the .cs file is the first controller (if single file) and before that the project name
        var fileName = $"{this.ProjectName}.{controllerClientDetails.First().Name}.cs";

        var clientCodeStringBuilder = new StringBuilder();

        foreach (var controllerClient in controllerClientDetails)
        {
            if (this.Configuration.UseInterfacesForClients)
            {
                clientCodeStringBuilder.AppendLine(this.AddClientInterfaceWithMethods(controllerClient));
            }

            clientCodeStringBuilder.AppendLine(this.AddClientImplementation(controllerClient));
        }

        // TODO check if client works without opening bracket and closing for namespace (new namespace change), if it doesnt work, indent lines
        var finalClientCodeString = $$"""
            {{this.AddUsings()}}

            namespace {{this.ProjectName}}.CSharp;

            {{clientCodeStringBuilder}}

            """.PrettifyCode();


        // TODO decide where to write to (out folder or via config?)
        // write to file
        File.WriteAllText("C:\\Workspace\\TestingApp\\TestingApp\\Test\\GeneratedClient.cs", finalClientCodeString);
    }

    private string AddClientImplementation(ControllerClientDetails controllerClientDetails)
    {
        var methodsBuilder = new StringBuilder();

        foreach (var method in controllerClientDetails.HttpMethods)
        {
            methodsBuilder.AppendLine(this.GenerateSingleMethodWithoutCancellationToken(method));
            methodsBuilder.AppendLine(this.GenerateSingleMethod(method));
        }

        var partialString = this.Configuration.UsePartialClientClasses ? " partial" : string.Empty;

        return $$"""
            public{{partialString}} class {{controllerClientDetails.Name}} : I{{controllerClientDetails.Name}}
            {
                {{this.AddHttpClientConstructorWithField(controllerClientDetails.Name)}}

                {{methodsBuilder}}
            }
            """;

    }

    private string GenerateSingleMethod(ControllerMethodDetails methodDetails)
    {
        // TODO add the correct return type... or whatever how you get it I dont care
        var returnTypeString = methodDetails.HasReturnType ? $"Task<{methodDetails.ReturnType.Name}>" : "Task";

        // TODO parameter might not be neccessary for GET and DELETE
        // TODO add the paramter to method details, we should have it, think about auth too
        // TODO option for several parameters (auth as above e.g.)
        var parameterString = methodDetails.HasParameters ? $"{methodDetails.HasParameters} request, " : string.Empty;

        var parameterCheck = methodDetails.HasParameters ? """
            if(request is null)
            {
                throw new ArgumentNullException(nameof(request), "The request object can´t ben null");
            }
            """ : string.Empty;

        // TODO parameter might not be neccessary for GET and DELETE
        // TODO check if is is working
        return $$"""
            public virtual async {{returnTypeString}} {{methodDetails.MethodName}}({{parameterString}}CancellationToken cancellationToken)
            {
                {{parameterCheck}}
            }
            """;
    }

    private string GenerateSingleMethodWithoutCancellationToken(ControllerMethodDetails methodDetails)
    {
        //// TODO add the correct return type... or whatever how you get it I dont care
        //var returnTypeString = methodDetails.HasReturnType ? $"Task<{methodDetails.ReturnType.Name}>" : "Task";

        //// TODO add the paramter to method details, we should have it, think about auth too
        //var parameter = "TODO";

        var methodNameString = methodDetails.HasParameters ? ", " : string.Empty;

        // TODO add the correct return type... or whatever how you get it I dont care
        var returnTypeString = methodDetails.HasReturnType ? $"Task<{methodDetails.ReturnType.Name}>" : "Task";

        // TODO parameter might not be neccessary for GET and DELETE
        // TODO add the paramter to method details, we should have it, think about auth too
        // TODO option for several parameters (auth as above e.g.)
        var parameterString = methodDetails.HasParameters ? $"{methodDetails.HasParameters} request, " : string.Empty;
        var existingParameter = methodDetails.HasParameters ? "request, " : string.Empty;

        return $$"""
            public virtual {{returnTypeString}} {{methodDetails.MethodName}}({{parameterString}})
            {
                return this.{{methodDetails.MethodName}}({{existingParameter}}CancellationToken.None);
            }
            """;
    }

    // TODO maybe we will ned serializer settings here as well
    public string AddHttpClientConstructorWithField(string clientName)
    {
        return $$"""
            private HttpClient httpClient;

            public {{clientName}}(HttpClient httpClient)
            {
                this.httpClient = httpClient;
            }
            """;
    }

    private string AddClientInterfaceWithMethods(ControllerClientDetails controllerClientDetails)
    {
        var interfaceMethodsBuilder = new StringBuilder();

        foreach (var method in controllerClientDetails.HttpMethods)
        {
            interfaceMethodsBuilder.AppendLine(this.GenerateInterfaceMethod(method));
        }

        var partialString = this.Configuration.UsePartialClientClasses ? " partial" : string.Empty;

        return $$"""
            public{{partialString}} interface I{{controllerClientDetails.Name}}
            {
                {{interfaceMethodsBuilder}}
            }
            """;
    }

    private string GenerateInterfaceMethod(ControllerMethodDetails methodDetails)
    {
        // TODO add the correct return type... or whatever how you get it I dont care
        var returnTypeString = methodDetails.HasReturnType ? $"Task<{methodDetails.ReturnType.Name}>" : "Task";
        // TODO add the paramter to method details, we should have it
        var parameter = "TODO";

        // TODO be aware that authentication might be added here as a first parameter
        // TODO multiple parameters possible? (except authentication)

        if (methodDetails.HasParameters)
        {
            // TODO dont forget authentication
            return $"""
            {returnTypeString} {methodDetails.MethodName}({parameter});
            {returnTypeString} {methodDetails.MethodName}({parameter}, CancellationToken cancellationToken);
            """;
        }
        else
        {
            // TODO dont forget authentication
            return $"""
            {returnTypeString} {methodDetails.MethodName}();
            {returnTypeString} {methodDetails.MethodName}(CancellationToken cancellationToken);
            """;
        }

        
    }

    // TODO global usings?
    // TODO doesnt work yet, what do I need to do so I have the right assembly?
    private string AddUsings(IEnumerable<string> additionalUsings = null)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine("""
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using System.Net.Http;
            """);

        if (additionalUsings is not null)
        {
            foreach (var singleUsing in additionalUsings)
            {
                stringBuilder.AppendLine($"using {singleUsing};");
            }
        }

        return stringBuilder.ToString();
    }


}
