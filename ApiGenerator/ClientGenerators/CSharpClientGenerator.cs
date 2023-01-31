﻿using ApiGenerator.Extensions;
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
            {{this.AddApiResponseClass()}}

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

            }
            """;

    }

    private string GenerateSingleMethod(ControllerMethodDetails methodDetails)
    {
        // TODO auth

        var returnTypeString = methodDetails.HasReturnType ? $"Task<ApiResponse<{methodDetails.ReturnTypeString}>>" : "Task<ApiResponse>";
        var parameterString = methodDetails.HasParameters ? $"{methodDetails.ParameterString}, " : string.Empty;
        var parameterCheckSb = new StringBuilder();

        foreach (var parameter in methodDetails.Parameters)
        {
            parameterCheckSb.Append($$"""
                if({{parameter.Key}} == null)
                {
                    throw new ArgumentNullException(nameof({{parameter.Key}}), "The object can´t be null");
                }
                """);
        }

        var nonPrimitive = methodDetails.Parameters.FirstOrDefault(x => !x.Value.IsPrimitive); //.Value.ParameterTypeString ?? string.Empty;
        var nonPrimitiveString = nonPrimitive.Value is not null ? nonPrimitive.Value.ParameterTypeString : string.Empty;
        var nonPrimitiveArgument = nonPrimitive.Value is not null ? nonPrimitive.Key : "null";

        var routeStringWithQueryParams = string.Empty;
        var routeQueryParamSb = new StringBuilder();

        if (methodDetails.HasRouteQueryParameters)
        {
            foreach (var parameter in methodDetails.Parameters)
            {
                if (parameter.Value.IsRouteQueryParameter)
                {
                    routeQueryParamSb.AppendLine($$"""
                        routeBuilder.Replace("{{{parameter.Key}}}", Uri.EscapeDataString({{parameter.Key}}.ToString()));
                        """);
                }
            }
        }

        if (methodDetails.Parameters.SingleOrDefault(x => x.Value.IsQueryParameter) is var keyValuePair && keyValuePair.Value is not null)
        {
            routeQueryParamSb.AppendLine($"""
                routeBuilder.Append($"{keyValuePair.Value.QueryString}");
                """);
        }

        // (for now) only use a request class for an actual body
        var methodCallString = (nonPrimitive.Value.HasBody, methodDetails.HasReturnType) switch
        {
            (true, true) => $"return await this.SendJsonAsync<{nonPrimitive.Value.ParameterTypeString}, {methodDetails.ReturnTypeString}>(uri, {nonPrimitiveArgument}, new HttpMethod(\"{methodDetails.HttpMethod.Method}\"), cancellationToken);",
            (true, false) => $"return await this.SendJsonAsync<{methodDetails.ParameterString}>(uri, null, new HttpMethod(\"{methodDetails.HttpMethod.Method}\"), cancellationToken);",
            (false, true) => $"return await this.SendJsonAsync<object, {methodDetails.ReturnTypeString}>(uri, null, new HttpMethod(\"{methodDetails.HttpMethod.Method}\"), cancellationToken);",
            (false, false) => "return await this.SendJsonAsync(uri, new HttpMethod(\"{methodDetails.HttpMethod.Method}\"), cancellationToken);"
        };

        return $$"""
            public virtual async {{returnTypeString}} {{methodDetails.MethodName}}({{parameterString}}CancellationToken cancellationToken)
            {
                {{parameterCheckSb}}

                var routeBuilder = new StringBuilder();
                routeBuilder.Append("{{methodDetails.Route}}");

                {{routeQueryParamSb}}

                var uri = new Uri(routeBuilder.ToString(), UriKind.RelativeOrAbsolute); 
                {{methodCallString}}
            }
            """;
    }

    private string GenerateSingleMethodWithoutCancellationToken(ControllerMethodDetails methodDetails)
    {
        //TODO auth parameter

        var methodNameString = methodDetails.HasParameters ? ", " : string.Empty;
        var returnTypeString = methodDetails.HasReturnType ? $"Task<ApiResponse<{methodDetails.ReturnTypeString}>>" : "Task<ApiResponse>";
        var parameterString = methodDetails.HasParameters ? $"{methodDetails.ParameterString}" : string.Empty;
        var existingParameter = methodDetails.HasParameters ? string.Join(", ", methodDetails.Parameters.Select(x => x.Key)) + ", " : string.Empty;

        return $$"""
            public virtual {{returnTypeString}} {{methodDetails.MethodName}}({{parameterString}})
            {
                return this.{{methodDetails.MethodName}}({{existingParameter}}CancellationToken.None);
            }
            """;
    }

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
        // TODO auth

        var returnTypeString = methodDetails.HasReturnType ? $"Task<ApiResponse<{methodDetails.ReturnTypeString}>>" : "Task<ApiResponse>";
        var parameterString = methodDetails.HasParameters ? $"{methodDetails.ParameterString}" : string.Empty;

        if (methodDetails.HasParameters)
        {
            return $"""
            {returnTypeString} {methodDetails.MethodName}({parameterString});
            {returnTypeString} {methodDetails.MethodName}({parameterString}, CancellationToken cancellationToken);
            """;
        }
        else
        {
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
            using System.Net.Http.Headers;
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
        private async Task<ApiResponse<TResponse>> HandleResponse<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            return await this.DeserializeResponse<TResponse>(response, false, cancellationToken);
        }
        """;

    private string AddDeserializeMethod() => """
        protected virtual async Task<ApiResponse<T>> DeserializeResponse<T>(HttpResponseMessage response, bool isStream, CancellationToken cancellationToken)
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
                IsError = statusCode < 200 || statusCode >= 300;
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

    private string AddPostJsonHelperMethods() => $$"""
        private async Task<ApiResponse<TResponse>> SendJsonAsync<TRequest, TResponse>(Uri endpoint, TRequest? requestObject, HttpMethod httpMethod, CancellationToken cancellationToken)
        {
            using var timeout = new CancellationTokenSource(this.httpClient.Timeout);

            try
            {
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);

                using var request = new HttpRequestMessage();
                request.RequestUri = endpoint;
                request.Method = httpMethod;

                if (requestObject is not null)
                {
                    var requestJson = JsonSerializer.Serialize(request, this.jsonSerializerOptions);
                    request.Content = new StringContent(requestJson);
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                }

                using var response = await this.httpClient.SendAsync(request, linked.Token);
                return await this.HandleResponse<TResponse>(response, linked.Token);

            }
            catch (TaskCanceledException) when (timeout.IsCancellationRequested)
            {
                throw new TimeoutException($"Did not receive an answer from the service within a timespan of {this.httpClient.Timeout}.");
            }
        }

        private async Task<ApiResponse> SendJsonAsync<TRequest>(Uri endpoint, TRequest? requestObject, HttpMethod httpMethod, CancellationToken cancellationToken)
        {
            var res = await this.SendJsonAsync<TRequest, object>(endpoint, requestObject, httpMethod, cancellationToken);
            return new ApiResponse(res.StatusCode, res.ErrorMessage, res.Exception);
        }

        private Task<ApiResponse> SendJsonAsync(Uri endpoint, HttpMethod httpMethod, CancellationToken cancellationToken)
        {
            return this.SendJsonAsync<object>(endpoint, null, httpMethod, cancellationToken);
        }
        """;
    


}
