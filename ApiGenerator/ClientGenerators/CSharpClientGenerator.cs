﻿using ApiGenerator.Extensions;
using ApiGenerator.Models;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ApiGenerator.ClientGenerators;

public class CSharpClientGenerator : ClientGeneratorBase
{
    public CSharpClientGenerator(Configuration configuration, string projectName) : base(configuration, projectName) { }

    public override void GenerateClient(IEnumerable<ControllerClientDetails> controllerClientDetails, string directoryPath)
    {
        // TODO for now place all clients into single file
        // if all clients are in one file, merge its generated (possible duplicates) classes 

        var mergedCodeClasses = controllerClientDetails
            .Select(client => client.GeneratedCodeClasses)
            .Merge()
            .ConcatValues();

        // beginning of file
        // name of the .cs file is the first controller (if single file) and before that the project name
        var fileName = $"{this.ProjectName}.{controllerClientDetails.FirstOrDefault()?.Name}.cs";

        var clientCodeStringBuilder = new StringBuilder();

        foreach (var controllerClient in controllerClientDetails)
        {
            if (this.Configuration.UseInterfacesForClients)
            {
                clientCodeStringBuilder.AppendLine(this.AddClientInterfaceWithMethods(controllerClient));
            }

            clientCodeStringBuilder.AppendLine(this.AddClientImplementation(controllerClient));
        }

        var finalClientCodeString = $$"""
            // <auto-generated>
            //     This file was auto generated.
            // </auto-generated>

            {{this.AddUsings()}}

            namespace {{this.ProjectName}}.CSharp
            {

            {{clientCodeStringBuilder}}

            {{mergedCodeClasses}}
            {{this.AddApiResponseClass()}}
            }
            """.PrettifyCode();

        File.WriteAllText($"{directoryPath}\\{fileName}", finalClientCodeString);
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
        var interfaceString = this.Configuration.UseInterfacesForClients ? $": I{controllerClientDetails.Name}" : string.Empty;

        return $$"""
            public{{partialString}} class {{controllerClientDetails.Name}} {{interfaceString}}
            {
                {{this.AddHttpClientConstructorWithField(controllerClientDetails.Name)}}
                {{this.AddPrepareRequestDelegate}}
                {{this.AddProcessResponseDelegate}}
            
                {{methodsBuilder}}

                {{this.AddPostJsonHelperMethod()}}
                {{this.AddDeserializeMethod()}}

            }
            """;
    }

    private string GenerateSingleMethod(ControllerMethodDetails methodDetails)
    {
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
        var methodCallString = (nonPrimitive.Key == null ? false : nonPrimitive.Value.HasBody) switch
        {
            true => $"var response = await this.SendJsonAsync<{nonPrimitive.Value.ParameterTypeString}>(uri, {nonPrimitiveArgument}, new HttpMethod(\"{methodDetails.HttpMethod.Method}\"), cancellationToken, prepareSingleRequest);",
            false => $"var response = await this.SendJsonAsync<object>(uri, null, new HttpMethod(\"{methodDetails.HttpMethod.Method}\"), cancellationToken, prepareSingleRequest);"
        };

        return $$"""
            public virtual async {{returnTypeString}} {{methodDetails.MethodName}}({{parameterString}}CancellationToken cancellationToken, Action<HttpClient, HttpRequestMessage> prepareSingleRequest = null)
            {
                {{parameterCheckSb}}

                var routeBuilder = new StringBuilder();
                routeBuilder.Append("{{methodDetails.Route}}");

                {{routeQueryParamSb}}

                var uri = new Uri(routeBuilder.ToString(), UriKind.RelativeOrAbsolute); 
                
                {{methodCallString}}

                this.ProcessResponse(this.httpClient, response);

                {{this.AddHandleResponseMethod(methodDetails)}}
            }
            """;
    }

    private string AddHandleResponseMethod(ControllerMethodDetails controllerMethodDetails)
    {
        var switchSb = new StringBuilder();

        foreach (var pair in controllerMethodDetails.ReturnTypes)
        {
            var cleanClassValue = pair.Value.ToString().SanitizeClassTypeString();
            var resultString = $$"""
                return new ApiResponse<{{controllerMethodDetails.ReturnTypeString}}>(default, statusCode) { ErrorResponse = new ApiErrorResponse<{{cleanClassValue}}>(result{{pair.Key}}.SuccessResponse, statusCode) };
                """; 

            if (controllerMethodDetails.ReturnTypes.IndexOf(pair) == 0)
            {
                resultString = $"return result{ pair.Key};";
            }

            switchSb.AppendLine($$"""
                case {{pair.Key}}:
                    var result{{pair.Key}} = await this.DeserializeResponse<{{cleanClassValue}}>(response, false, cancellationToken);
                    
                    if (result{{pair.Key}}.IsError)
                    {
                        return new ApiResponse<{{controllerMethodDetails.ReturnTypeString}}>(default, 0, result{{pair.Key}}.ErrorMessage, result{{pair.Key}}.Exception);
                    }

                    {{resultString}}
                """);
        }

        return $$"""
            var statusCode = (int)response.StatusCode;

            switch (statusCode)
            {
                {{switchSb}}

                default:
                    return new ApiResponse(statusCode, response.ReasonPhrase ?? string.Empty);
            }
            """;
    }

    private string GenerateSingleMethodWithoutCancellationToken(ControllerMethodDetails methodDetails)
    {
        var methodNameString = methodDetails.HasParameters ? ", " : string.Empty;
        var returnTypeString = methodDetails.HasReturnType ? $"Task<ApiResponse<{methodDetails.ReturnTypeString}>>" : "Task<ApiResponse>";
        var parameterString = methodDetails.HasParameters ? $"{methodDetails.ParameterString}, " : string.Empty;
        var existingParameter = methodDetails.HasParameters ? string.Join(", ", methodDetails.Parameters.Select(x => x.Key)) + ", " : string.Empty;

        return $$"""
            public virtual {{returnTypeString}} {{methodDetails.MethodName}}({{parameterString}}Action<HttpClient, HttpRequestMessage> prepareSingleRequest = null)
            {
                return this.{{methodDetails.MethodName}}({{existingParameter}}CancellationToken.None, prepareSingleRequest);
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
        var returnTypeString = methodDetails.HasReturnType ? $"Task<ApiResponse<{methodDetails.ReturnTypeString}>>" : "Task<ApiResponse>";
        var parameterString = methodDetails.HasParameters ? $"{methodDetails.ParameterString}" : string.Empty;

        if (methodDetails.HasParameters)
        {
            return $"""
            {returnTypeString} {methodDetails.MethodName}({parameterString}, Action<HttpClient, HttpRequestMessage> prepareSingleRequest = null);
            {returnTypeString} {methodDetails.MethodName}({parameterString}, CancellationToken cancellationToken, Action<HttpClient, HttpRequestMessage> prepareSingleRequest = null);
            """;
        }
        else
        {
            return $"""
            {returnTypeString} {methodDetails.MethodName}(Action<HttpClient, HttpRequestMessage> prepareSingleRequest = null);
            {returnTypeString} {methodDetails.MethodName}(CancellationToken cancellationToken, Action<HttpClient, HttpRequestMessage> prepareSingleRequest = null);
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

    private string AddPrepareRequestDelegate => """
        public Action<HttpClient, HttpRequestMessage> PrepareRequest { get; set; } = (HttpClient client, HttpRequestMessage httpRequestMessage) => {};
        """;

    private string AddProcessResponseDelegate => """
        public Action<HttpClient, HttpResponseMessage> ProcessResponse { get; set; } = (HttpClient client, HttpResponseMessage httpResponseMessage) => {};
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
                ErrorMessage = errorMessage;
                Exception = exception;
            }

            public int StatusCode { get; }
            public string ErrorMessage { get; }
            public Exception? Exception { get; }
        }

        public class ApiResponse<TResponse> : ApiResponse
        {
            public ApiResponse(TResponse response, int statusCode, string errorMessage = "", Exception? exception = null)
                : base(statusCode, errorMessage, exception)
            {
                SuccessResponse = response;
            }
            public bool IsError => this.ErrorResponse != null;

            public TResponse SuccessResponse { get; set; }

            public ApiResponse? ErrorResponse { get; set; } = null;
        }

        public class ApiErrorResponse<TErrorResponse> : ApiResponse
        {
            public ApiErrorResponse(TErrorResponse response, int statusCode, string errorMessage = "", Exception? exception = null)
                : base(statusCode, errorMessage, exception)
            {
                ErrorResponse = response;
            }

            public TErrorResponse? ErrorResponse { get; }
        }
        """;

    private string AddPostJsonHelperMethod() => $$"""
        private async Task<HttpResponseMessage> SendJsonAsync<TRequest>(Uri endpoint, TRequest? requestObject, HttpMethod httpMethod, CancellationToken cancellationToken, Action<HttpClient, HttpRequestMessage> prepareSingleRequest = null)
        {
            using var timeout = new CancellationTokenSource(this.httpClient.Timeout);

            try
            {
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);

                using var request = new HttpRequestMessage();
                this.PrepareRequest(this.httpClient, request);
                request.RequestUri = endpoint;
                request.Method = httpMethod;

                if (prepareSingleRequest is not null)
                {
                    prepareSingleRequest(this.httpClient, request);
                }

                if (requestObject is not null)
                {
                    var requestJson = JsonSerializer.Serialize(request, this.jsonSerializerOptions);
                    request.Content = new StringContent(requestJson);
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                }

                return await this.httpClient.SendAsync(request, linked.Token);

            }
            catch (TaskCanceledException) when (timeout.IsCancellationRequested)
            {
                throw new TimeoutException($"Did not receive an answer from the service within a timespan of {this.httpClient.Timeout}.");
            }
        }
        """;
}
