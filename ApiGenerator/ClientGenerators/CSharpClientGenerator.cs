using ApiGenerator.Extensions;
using ApiGenerator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ApiGenerator.ClientGenerators;

public class CSharpClientGenerator : ClientGeneratorBase
{
    public CSharpClientGenerator(string projectName) : base(projectName) { }

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

        // add the necessary usings
        this.AddUsings();

        this.CodeStringBuilder.AppendLine($"namespace {this.ProjectName}.CSharp");
        // TODO check if client works without opening bracket and closing for namespace (new namespace change)
        // if it doesnt work, indent lines

        // add the clients
        foreach (var controllerClient in controllerClientDetails)
        {
            this.GenerateSingleClient(controllerClient);
        }

        // TODO see above about closing, dont forget to unindent line

        // TODO decide where to write to (out folder?)
        // write to file
    }

    private void GenerateSingleClient(ControllerClientDetails controllerClientDetails)
    {
        // TODO could add whether to add an interface or not in config, not sure if I want to though (but no interface = performance)
        this.AddClientInterface(controllerClientDetails);
        this.GenerateClientImplementation(controllerClientDetails);
        
    }

    private void GenerateClientImplementation(ControllerClientDetails controllerClientDetails)
    {
        this.CodeStringBuilder.AppendLine($"public partial class {controllerClientDetails.Name} : I{controllerClientDetails.Name}");
        this.CodeStringBuilder.OpenCurlyBracketLine();

        this.AddConstructor(controllerClientDetails.Name);

        // add implementation methods
        foreach (var method in controllerClientDetails.HttpMethods)
        {
            this.GenerateSingleMethod(method);
            this.GenerateSingleMethodWithoutCancellationToken(method);
        }

        this.CodeStringBuilder.CloseCurlyBracketLine();
    }

    private void GenerateSingleMethod(ControllerMethodDetails methodDetails)
    {

    }

    private void GenerateSingleMethodWithoutCancellationToken(ControllerMethodDetails methodDetails)
    {
        /*
         * public virtual System.Threading.Tasks.Task<EvaluateBetslipResponse> EvaluateBetslipAsync(string x_asw_sessiontoken, EvaluateBetslipRequest request)
        {
            return EvaluateBetslipAsync(x_asw_sessiontoken, request, System.Threading.CancellationToken.None);
        }
         * */

        // TODO add the correct return type... or whatever how you get it I dont care
        var returnType = "TODO";
        // TODO add the paramter to method details, we should have it, think about auth too
        var parameter = "TODO";

        this.CodeStringBuilder.AppendLine($"public virtual Task<{returnType}> {methodDetails.MethodName}({parameter})");
        this.CodeStringBuilder.OpenCurlyBracketLine();

        // TODO once again add the parameter/s
        this.CodeStringBuilder.AppendLine($"return this.{methodDetails.MethodName}(CancellationToken.None);");
        this.CodeStringBuilder.CloseCurlyBracketLine();
    }

    // TODO maybe we will ned serializer settings here as well
    private void AddConstructor(string clientName)
    {
        this.CodeStringBuilder.AppendLine($"public {clientName}(HttpClient httpClient)");
        this.CodeStringBuilder.OpenCurlyBracketLine();
        this.CodeStringBuilder.AppendLine($"this.httpClient = httpClient;");
        this.CodeStringBuilder.CloseCurlyBracketLine();
    }

    private void AddClientInterface(ControllerClientDetails controllerClientDetails)
    {
        this.CodeStringBuilder.AppendLine($"public partial interface I{controllerClientDetails.Name}");
        this.CodeStringBuilder.OpenCurlyBracketLine();

        foreach (var method in controllerClientDetails.HttpMethods)
        {
            this.AddInterfaceMethod(method);
        }

        this.CodeStringBuilder.CloseCurlyBracketLine();
    }

    private void AddInterfaceMethod(ControllerMethodDetails methodDetails)
    {
        // TODO add the correct return type... or whatever how you get it I dont care
        var returnType = "TODO";
        // TODO add the paramter to method details, we should have it
        var parameter = "TODO";

        // TODO be aware that authentication might be added here as a first parameter
        // TODO multiple parameters possible? (except authentication)
        this.CodeStringBuilder.AppendLine($"Task<{returnType}> {methodDetails.MethodName}({parameter});");
        this.CodeStringBuilder.AppendLine($"Task<{returnType}> {methodDetails.MethodName}({parameter}, CancellationToken cancellationToken);");
    }

    // TODO global usings?
    private void AddUsings(IEnumerable<string> additionalUsings = null)
    {
        // TODO doesnt work yet, what do I need to do so I have the right assembly?
        this.CodeStringBuilder.AppendLine("using System;");
        this.CodeStringBuilder.AppendLine("using System.Threading;"); // for cancellation token
        this.CodeStringBuilder.AppendLine("using System.Threading.Tasks;");
        this.CodeStringBuilder.AppendLine("using System.Net.Http;"); // http client

        if (additionalUsings is not null)
        {
            foreach (var singleUsing in additionalUsings)
            {
                this.CodeStringBuilder.AppendLine($"using {singleUsing};");
            }
        }
    }


}
