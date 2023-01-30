using ApiGenerator.Extensions;
using ApiGenerator.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
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
            .Merge()
            .ConcatValues();

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

            {{mergedCodeClasses}}

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

                {{this.AddPostJsonHelperMethods()}}    
                {{this.AddHandleResponseMethod()}}
                {{this.AddDeserializeMethod()}}
                {{this.AddApiResponseClass()}}

            }
            """;

    }

    private string GenerateSingleMethod(ControllerMethodDetails methodDetails)
    {
        // TODO add the correct return type... or whatever how you get it I dont care
        var returnTypeString = methodDetails.HasReturnType ? $"Task<{methodDetails.ReturnTypeString}>" : "Task";

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

        var methodCallString = (methodDetails.HasParameters, methodDetails.HasReturnType) switch
        {
            (true, true) => "return await this.SendJsonAsync<string, string>(uri, string.Empty, cancellationToken);",
            (true, false) => "return await this.SendJsonAsync<string>(uri, null, cancellationToken);",
            (false, true) => "return await this.SendJsonAsync<string, string>(uri, null, cancellationToken);",
            (false, false) => "return await this.SendJsonAsync(uri, cancellationToken);"
        };

        // TODO parameter might not be neccessary for GET and DELETE
        // TODO check if is is working
        return $$"""
            public virtual async {{returnTypeString}} {{methodDetails.MethodName}}({{parameterString}}CancellationToken cancellationToken)
            {
                {{parameterCheck}}

                var uri = new Uri("{{methodDetails.Route}}");
                {{methodCallString}}
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
        var returnTypeString = methodDetails.HasReturnType ? $"Task<{methodDetails.ReturnTypeString}>" : "Task";

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
            private JsonSerializerOptions jsonSerializerOptions;

            public {{clientName}}(HttpClient httpClient)
            {
                this.httpClient = httpClient;
                this.jsonSerializerOptions = new JsonSerializerOptions();
            }

            public JsonSerializerOptions JsonSerializerOptions => this.jsonSerializerOptions;
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
        var returnTypeString = methodDetails.HasReturnType ? $"Task<{methodDetails.ReturnTypeString}>" : "Task";
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
            using System.Net;
            using System.Net.Http;
            using System.Net.Http.Json;
            using System.Text;
            using System.Text.Json;
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

    private string AddHandleResponseMethod() => """
        public async Task<ApiResponse<TResponse>> HandleResponse<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            return await this.DeserializeResponse<TResponse>(response, false, cancellationToken);
        }
        """;

    private string AddDeserializeMethod() => """
        public virtual async Task<ApiResponse<T>> DeserializeResponse<T>(HttpResponseMessage response, bool isStream, CancellationToken cancellationToken)
        {
            if (response == null)
            {
                return new ApiResponse<T>(default, 0, "The response was null.");
            }

            if (response.Content == null)
            {
                return new ApiResponse<T>(default, (int)response.StatusCode, "There was no content.");
            }

            if (isStream)
            {
                try
                {
                    using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    {
                        var result = await JsonSerializer.DeserializeAsync<T>(responseStream, this.jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
                        return new ApiResponse<T>(result, (int)response.StatusCode, string.Empty);
                    }
                }
                catch (JsonException exception)
                {
                    return new ApiResponse<T>(default, (int)response.StatusCode, $"Failed to deserialze stream of type {typeof(T).FullName}.", exception);

                }
            }
            else
            {
                var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    var result = JsonSerializer.Deserialize<T>(responseText, this.jsonSerializerOptions);
                    return new ApiResponse<T>(result, (int)response.StatusCode,string.Empty);
                }
                catch (JsonException exception)
                {
                    return new ApiResponse<T>(default, (int)response.StatusCode, $"Failed to deserialze response of type {typeof(T).FullName}.", exception);
                }
            }
        }
        """;

    private string AddApiResponseClass() => """
        public class ApiResponse
        {
            public ApiResponse(int statusCode, string errorMessage = "", Exception? exception = null)
            {
                StatusCode = statusCode;
                IsError = statusCode >= 200 && statusCode < 300;
                ErrorMessage = errorMessage;
                Exception = exception;
            }

            public int StatusCode { get; }
            public bool IsError { get; }
            public string ErrorMessage { get; }
            public Exception? Exception { get; }
        }

        public class ApiResponse<TResponse> : ApiResponse
        {
            public ApiResponse(TResponse response, int statusCode, string errorMessage = "", Exception? exception = null)
                : base(statusCode, errorMessage, exception)
            {
                Response = response;
            }

            public TResponse Response { get; }
        }
        """;

    private string AddPostJsonHelperMethods() => """
        private async Task<ApiResponse<TResponse>> SendJsonAsync<TRequest, TResponse>(Uri endpoint, TRequest? requestObject, CancellationToken cancellationToken)
        {
            using var timeout = new CancellationTokenSource(this.httpClient.Timeout);

            try
            {
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);

                using var request = new HttpRequestMessage();
                request.RequestUri = endpoint;
                request.Method = HttpMethod.Post; // this is set via code!

                if (requestObject is not null)
                {
                    var requestJson = JsonSerializer.Serialize(request, this.jsonSerializerOptions);
                    request.Content = new StringContent(requestJson);
                }

                using var response = await this.httpClient.SendAsync(request, linked.Token);
                return await this.HandleResponse<TResponse>(response, linked.Token);

            }
            catch (TaskCanceledException) when (timeout.IsCancellationRequested)
            {
                throw new TimeoutException($"Did not receive an answer from the service within a timespan of {this.httpClient.Timeout}.");
            }
        }

        private async Task<ApiResponse> SendJsonAsync<TRequest>(Uri endpoint, TRequest? requestObject, CancellationToken cancellationToken)
        {
            var res = await this.SendJsonAsync<TRequest, object>(endpoint, requestObject, cancellationToken);
            return new ApiResponse(res.StatusCode, res.ErrorMessage, res.Exception);
        }

        private Task<ApiResponse> SendJsonAsync(Uri endpoint, CancellationToken cancellationToken)
        {
            return this.SendJsonAsync<object>(endpoint, null, cancellationToken);
        }
        """;
    


}
