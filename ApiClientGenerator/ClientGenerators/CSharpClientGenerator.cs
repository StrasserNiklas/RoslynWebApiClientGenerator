﻿using ApiClientGenerator.Configuration;
using ApiClientGenerator.Models.ParameterDetails;
using ApiGenerator.Extensions;
using ApiGenerator.Models;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ApiGenerator.ClientGenerators;

public class CSharpClientGenerator : ClientGeneratorBase
{
    public CSharpClientGenerator(string projectName) : base(projectName) { }

    public override void GenerateClient(IEnumerable<ControllerClientDetails> controllerClientDetails, string directoryPath)
    {
        var mergedCodeClasses = controllerClientDetails
            .Select(client => client.GeneratedCodeClasses)
            .Merge()
            .ConcatValues();

        // beginning of file
        // name of the .cs file is the first controller (if single file) and before that the project name
        var fileName = $"{this.ProjectName}.{controllerClientDetails.FirstOrDefault()?.ClientName}.cs";

        var clientCodeStringBuilder = new StringBuilder();

        foreach (var controllerClient in controllerClientDetails)
        {
            if (Configuration.UseInterfacesForClients && !controllerClient.IsMinimalApiClient)
            {
                clientCodeStringBuilder.AppendLine(this.AddClientInterfaceWithMethods(controllerClient));
            }

            if (controllerClient.IsMinimalApiClient && !Configuration.GenerateMinimalApiClient)
            {
                continue;
            }

            clientCodeStringBuilder.AppendLine(this.AddClientImplementation(controllerClient));
        }

        var finalClientCodeString = $$"""
            // <auto-generated>
            //     This file was auto generated.
            // </auto-generated>
            #pragma warning disable 8669
            {{this.AddUsings(controllerClientDetails.Select(x => x.AdditionalUsings).Aggregate((a, b) => a.Union(b).ToList()))}}

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

        if (controllerClientDetails.IsMinimalApiClient)
        {
            foreach (var method in controllerClientDetails.Endpoints)
            {
                methodsBuilder.AppendLine(this.GenerateMinimalApiEndpoints(method));
            }
        }
        else
        {
            foreach (var method in controllerClientDetails.Endpoints)
            {
                methodsBuilder.AppendLine(this.GenerateSingleMethodWithoutCancellationToken(method));
                methodsBuilder.AppendLine(this.GenerateSingleEndpointMethod(method));
            }
        }

        var partialString = Configuration.UsePartialClientClasses ? " partial" : string.Empty;
        var interfaceString = Configuration.UseInterfacesForClients && !controllerClientDetails.IsMinimalApiClient ? $": I{controllerClientDetails.ClientName}" : string.Empty;

        return $$"""
            public{{partialString}} class {{controllerClientDetails.ClientName}} {{interfaceString}}
            {
                {{this.AddHttpClientConstructorWithField(controllerClientDetails.ClientName)}}
                {{methodsBuilder}}
                {{this.AddPrepareRequestMessageMethod()}}
                {{this.AddPostJsonHelperMethod()}}
                {{this.AddDeserializeMethod()}}
                {{this.AddHeaderHelperMethod()}}
                {{this.AddSetClientAuthorizationHeaderHelperMethod()}}
                {{this.AddPrepareRequestDelegate()}}
                {{this.AddProcessResponseDelegate()}}
            }
            """;
    }

    private string GenerateMinimalApiEndpoints(ControllerMethodDetails methodDetails)
    {
        var methodStringBuilder = new StringBuilder();
        var parameterString = string.Empty;
        var routeQueryParamStringBuilder = new StringBuilder();

        if (methodDetails.HasRouteQueryParameters)
        {
            foreach (Match match in methodDetails.RouteQueryMatches)
            {
                var value = match.Groups[1].Value;
                parameterString += $"string {value}, ";
                routeQueryParamStringBuilder.AppendLine($$"""
                        routeBuilder.Replace("{{{value}}}", Uri.EscapeDataString({{value}}));
                        """
                );
            }
        }

        var handleSimpleResponseString = "return new ApiResponse(statusCode, httpClientResponse.ReasonPhrase ?? string.Empty);";
        var handleObjectResponseString = $$"""
            var result = await this.DeserializeResponse<TOut>(httpClientResponse, false, cancellationToken);
            if (result.IsError)
            {
                return new ApiResponse<TOut>(default, statusCode, result.Message)
                {Exception = result.Exception};
            }
            
            return new ApiResponse<TOut>(result.SuccessResponse, statusCode, httpClientResponse.ReasonPhrase ?? string.Empty);
            """;

        var defaultReturnStringWithObjectString = $"return new ApiResponse<TOut>(default, statusCode, $\"Encountered unexpected statuscode {{statusCode}}. {{httpClientResponse.ReasonPhrase ?? string.Empty}}\") {{ErrorResponse = new ApiError()}};";
        var defaultReturnStringWithoutObjectString = $" return new ApiResponse(statusCode, $\"Encountered unexpected statuscode {{statusCode}}. {{httpClientResponse.ReasonPhrase ?? string.Empty}}\") {{ ErrorResponse = new ApiError() }};";

        var prepareSimpleRequestString = $"var httpRequestMessage = this.PrepareRequestMessage<object>(uri, null, new HttpMethod(\"{methodDetails.HttpMethod.Method}\"), null, prepareSingleRequest);";
        var prepareObjectRequestString = $"var httpRequestMessage = this.PrepareRequestMessage<TIn>(uri, requestBody, new HttpMethod(\"{methodDetails.HttpMethod.Method}\"), null, prepareSingleRequest);";

        // request and result object
        methodStringBuilder.AppendLine(this.BuildMinimalApiEndpointMethod(
            $"Task<ApiResponse<TOut>> {methodDetails.MethodName}<TIn, TOut>({parameterString}TIn requestBody, ",
            methodDetails.Route,
            routeQueryParamStringBuilder,
            prepareObjectRequestString,
            handleObjectResponseString,
            defaultReturnStringWithObjectString));

        // request and no result object
        methodStringBuilder.AppendLine(this.BuildMinimalApiEndpointMethod(
            $"Task<ApiResponse> {methodDetails.MethodName}<TIn>({parameterString}TIn requestBody, ",
            methodDetails.Route,
            routeQueryParamStringBuilder,
            prepareObjectRequestString,
            handleSimpleResponseString,
            defaultReturnStringWithoutObjectString));

        // no request and result object
        methodStringBuilder.AppendLine(this.BuildMinimalApiEndpointMethod(
            $"Task<ApiResponse<TOut>> {methodDetails.MethodName}<TOut>({parameterString}",
            methodDetails.Route,
            routeQueryParamStringBuilder,
            prepareSimpleRequestString,
            handleObjectResponseString,
            defaultReturnStringWithObjectString));

        // no request and no result object
        methodStringBuilder.AppendLine(this.BuildMinimalApiEndpointMethod(
            $"Task<ApiResponse> {methodDetails.MethodName}({parameterString}",
            methodDetails.Route,
            routeQueryParamStringBuilder,
            prepareSimpleRequestString,
            handleSimpleResponseString,
            defaultReturnStringWithoutObjectString));

        return methodStringBuilder.ToString();
    }

    private string BuildMinimalApiEndpointMethod(string individualMethodSignature, string route, StringBuilder routeQueryParamStringBuilder, string methodCallString, string handleResponseString, string defaultReturnString)
    {
        return $$"""
            public virtual async {{individualMethodSignature}}CancellationToken cancellationToken, Action<HttpClient, HttpRequestMessage> prepareSingleRequest = null)
            {
                var routeBuilder = new StringBuilder();
                routeBuilder.Append("{{route}}");
            
                {{routeQueryParamStringBuilder}}
            
                var uri = new Uri(routeBuilder.ToString(), UriKind.RelativeOrAbsolute); 
            
                {{methodCallString}}
            
                var httpClientResponse = await this.SendJsonAsync(httpRequestMessage, cancellationToken);
                this.ProcessResponse(this.httpClient, httpClientResponse);
            
                var statusCode = (int)httpClientResponse.StatusCode;
            
                if (statusCode == 200 || statusCode == 201)
                {
                    {{handleResponseString}}
                }
                else
                {
                    {{defaultReturnString}}
                }
            }
            """;
    }

    private string GenerateSingleEndpointMethod(ControllerMethodDetails methodDetails)
    {
        var returnTypeString = methodDetails.HasReturnType ? $"Task<ApiResponse<{methodDetails.MainReturnTypeString}>>" : "Task<ApiResponse>";
        var parameterString = methodDetails.HasParameters ? $"{methodDetails.ParameterStringWithTypes}, " : string.Empty;
        var parameterCheckStringBuilder = new StringBuilder();
        var routeQueryParamStringBuilder = new StringBuilder();
        var headerKeyValuesStringBuilder = new StringBuilder();
        ParameterDetails bodyParameterDetails = null;
        var hasHeaderParameter = false;
        var fromFormString = string.Empty;
        var queryStringHasBeenSet = false;

        foreach (var parameter in methodDetails.Parameters)
        {
            // add null check if possible
            if (parameter.Value.IsNullable)
            {
                parameterCheckStringBuilder.Append($$"""
                    if({{parameter.Key}} == null)
                    {
                        throw new ArgumentNullException(nameof({{parameter.Key}}), "The object can´t be null");
                    }
                    """);
            }

            switch (parameter.Value)
            {
                case HeaderParameterDetails headerParameterDetails:
                    hasHeaderParameter = true;

                    foreach (var header in headerParameterDetails.HeaderKeyValues)
                    {
                        headerKeyValuesStringBuilder.AppendLine($$"""
                            { "{{header.Key}}", {{header.Value}}.ToString()},
                            """);
                    }
                    break;

                case BodyParameterDetails:
                    bodyParameterDetails = parameter.Value;
                    break;

                case QueryParameterDetails queryParameterDetails:
                    // add route query parameters e.g. /api/clients/{id}
                    // this is due to a simple type being parsed as a query parameter because at parsing the parameters we don´t know if it is a route parameter
                    if (queryParameterDetails.IsRouteQueryParameter)
                    {
                        routeQueryParamStringBuilder.AppendLine($$"""
                            routeBuilder.Replace("{{{parameter.Key.ToLowerInvariant()}}}", Uri.EscapeDataString({{parameter.Key}}.ToString()));
                            """);
                    }
                    else
                    {
                        if (queryStringHasBeenSet)
                        {
                            routeQueryParamStringBuilder.AppendLine($"""
                                routeBuilder.Append($"{queryParameterDetails.QueryString.Replace('?', '&')}");
                                """);
                        }
                        else
                        {
                            routeQueryParamStringBuilder.AppendLine($"""
                                routeBuilder.Append($"{queryParameterDetails.QueryString}");
                                """);
                            queryStringHasBeenSet = true;
                        }
                    }
                    break;

                case RouteQueryParameterDetails routeQueryParameterDetails:
                    routeQueryParamStringBuilder.AppendLine(routeQueryParameterDetails.RouteQueryManipulationString);
                    break;

                case FormParameterDetails formParameterDetails:
                    fromFormString = formParameterDetails.FormString;
                    break;
            }
        }

        var headerString = hasHeaderParameter ? $$"""
            var headers = new Dictionary<string, string>()
            {
                {{headerKeyValuesStringBuilder}}
            };
            """ : string.Empty;

        var methodCallString = bodyParameterDetails is not null ?
            $"var httpRequestMessage = this.PrepareRequestMessage<{bodyParameterDetails.ParameterTypeString}>(uri, {bodyParameterDetails.ParameterName}, new HttpMethod(\"{methodDetails.HttpMethod.Method}\"), {(hasHeaderParameter ? "headers, " : "null, ")}prepareSingleRequest);"
            : $"var httpRequestMessage = this.PrepareRequestMessage<object>(uri, null, new HttpMethod(\"{methodDetails.HttpMethod.Method}\"), {(hasHeaderParameter ? "headers, " : "null, ")}prepareSingleRequest);";

        return $$"""
            public virtual async {{returnTypeString}} {{methodDetails.MethodName}}({{parameterString}}CancellationToken cancellationToken, Action<HttpClient, HttpRequestMessage> prepareSingleRequest = null)
            {
                {{parameterCheckStringBuilder}}

                var routeBuilder = new StringBuilder();
                routeBuilder.Append("{{methodDetails.Route}}");

                {{routeQueryParamStringBuilder}}

                var uri = new Uri(routeBuilder.ToString(), UriKind.RelativeOrAbsolute); 
                
                {{headerString}}
                {{methodCallString}}
                {{fromFormString}}

                var httpClientResponse = await this.SendJsonAsync(httpRequestMessage, cancellationToken);
                this.ProcessResponse(this.httpClient, httpClientResponse);

                {{this.AddHandleResponseMethod(methodDetails)}}
            }
            """;
    }

    private string AddHandleResponseMethod(ControllerMethodDetails methodDetails)
    {
        var switchStringBuilder = new StringBuilder();

        foreach (var pair in methodDetails.ReturnTypes)
        {
            // TODO fix hack later
            var isSuccessCase = pair.Key == 200 || pair.Key == 201 || pair.Key == 204;

            if (pair.Value is not null)
            {
                var cleanClassValue = pair.Value.CheckAndSanitizeClassString();

                var methodErrorReturnString = methodDetails.HasReturnType ? $$"""
                    return new ApiResponse<{{methodDetails.MainReturnTypeString}}>(default, statusCode,  result{{pair.Key}}.Message) { ErrorResponse = result{{pair.Key}}.ErrorResponse, Exception = result{{pair.Key}}.Exception };
                    """ : $$"""
                    return new ApiResponse(statusCode, result{{pair.Key}}.Message) { ErrorResponse = result{{pair.Key}}.ErrorResponse, Exception = result{{pair.Key}}.Exception };
                    """;

                var defaultString = isSuccessCase ? $"result{pair.Key}.SuccessResponse" : "default";
                var errorResponseString = isSuccessCase ? string.Empty : $"ErrorResponse = result{pair.Key}.SuccessResponse";

                var methodReturnString = methodDetails.HasReturnType ? $$"""
                    return new ApiResponse<{{methodDetails.MainReturnTypeString}}>({{defaultString}}, statusCode,  httpClientResponse.ReasonPhrase ?? string.Empty) 
                    { 
                        {{errorResponseString}}
                    };
                    """ : $$"""
                    return new ApiResponse(statusCode, httpClientResponse.ReasonPhrase ?? string.Empty) { {{errorResponseString}} };
                    """;

                switchStringBuilder.AppendLine($$"""
                    case {{pair.Key}}:
                        var result{{pair.Key}} = await this.DeserializeResponse<{{cleanClassValue}}>(httpClientResponse, false, cancellationToken);

                        if (result{{pair.Key}}.IsError)
                        {
                            {{methodErrorReturnString}}
                        }

                        {{methodReturnString}}
                    """);
            }
            else
            {
                // TODO fix this hack 
                if (isSuccessCase && methodDetails.ReturnTypes.Any(x => x.Key != pair.Key && (x.Key == 200 || x.Key == 201 || x.Key == 204)))
                {
                    //if (methodDetails.HasReturnType && methodDetails.MainReturnTypeString != pair.Value.CheckAndSanitizeClassString())
                    //{

                    //}

                    if (methodDetails.HasReturnType)
                    {
                        switchStringBuilder.AppendLine($$"""
                            case {{pair.Key}}:
                                return new ApiResponse<{{methodDetails.MainReturnTypeString}}>(default, statusCode,  httpClientResponse.ReasonPhrase ?? string.Empty) { };
                            """);
                    }
                    else
                    {
                        switchStringBuilder.AppendLine($$"""
                            case {{pair.Key}}:
                                return new ApiResponse(statusCode, httpClientResponse.ReasonPhrase ?? string.Empty);
                            """);
                    }
                }
                else
                {
                    if (methodDetails.HasReturnType)
                    {
                        switchStringBuilder.AppendLine($$"""
                            case {{pair.Key}}:
                                return new ApiResponse<{{methodDetails.MainReturnTypeString}}>(default, statusCode,  httpClientResponse.ReasonPhrase ?? string.Empty) { ErrorResponse = new ApiError() };
                            """);
                    }
                    else
                    {
                        switchStringBuilder.AppendLine($$"""
                        case {{pair.Key}}:
                            return new ApiResponse(statusCode, httpClientResponse.ReasonPhrase ?? string.Empty)  { ErrorResponse = new ApiError() };
                    """);
                    }
                }
            }
        }

        var defaultReturnString = methodDetails.HasReturnType ? $"<{methodDetails.MainReturnTypeString}>(default, " : "(";

        if (!methodDetails.HasReturnType && methodDetails.ReturnTypes.Count() == 0)
        {
            switchStringBuilder.AppendLine($$"""
                case 200:
                case 201:
                    return new ApiResponse(statusCode, httpClientResponse.ReasonPhrase ?? string.Empty);
                """);
        }

        return $$"""
            var statusCode = (int)httpClientResponse.StatusCode;

            switch (statusCode)
            {
                {{switchStringBuilder}}

                default:
                    var errorContent = await httpClientResponse.Content.ReadAsStringAsync();
                    return new ApiResponse{{defaultReturnString}}statusCode, $"Encountered unexpected statuscode {statusCode}. {httpClientResponse.ReasonPhrase ?? string.Empty}. {errorContent}") { ErrorResponse = new ApiError() };
            }
            """;
    }

    private string GenerateSingleMethodWithoutCancellationToken(ControllerMethodDetails methodDetails)
    {
        var returnTypeString = methodDetails.HasReturnType ? $"Task<ApiResponse<{methodDetails.MainReturnTypeString}>>" : "Task<ApiResponse>";
        var parameterString = methodDetails.HasParameters ? $"{methodDetails.ParameterStringWithTypes}, " : string.Empty;
        var existingParameter = methodDetails.HasParameters ? $"{methodDetails.ParameterStringWithoutTypes}, " : string.Empty;

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
            private Dictionary<string, string> clientWideHeaders;
            
            public {{clientName}}(HttpClient httpClient)
            {
                this.httpClient = httpClient;
                this.jsonSerializerOptions = new JsonSerializerOptions();
                this.clientWideHeaders = new Dictionary<string, string>();    
            }

            public JsonSerializerOptions JsonSerializerOptions => this.jsonSerializerOptions;
            """;
    }

    private string AddClientInterfaceWithMethods(ControllerClientDetails controllerClientDetails)
    {
        var interfaceMethodsStringBuilder = new StringBuilder();

        foreach (var method in controllerClientDetails.Endpoints)
        {
            interfaceMethodsStringBuilder.AppendLine(this.GenerateInterfaceMethod(method));
        }

        var partialString = Configuration.UsePartialClientClasses ? " partial" : string.Empty;

        return $$"""
            public{{partialString}} interface I{{controllerClientDetails.ClientName}}
            {
                {{interfaceMethodsStringBuilder}}
            }
            """;
    }

    private string GenerateInterfaceMethod(ControllerMethodDetails methodDetails)
    {
        var returnTypeString = methodDetails.HasReturnType ? $"Task<ApiResponse<{methodDetails.MainReturnTypeString}>>" : "Task<ApiResponse>";
        var parameterString = methodDetails.HasParameters ? $"{methodDetails.ParameterStringWithTypes}" : string.Empty;

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

    private string AddUsings(IEnumerable<string> additionalUsings = null)
    {
        IEnumerable<string> finalList = new List<string>();

        var baseList = new List<string>()
        {
            "System",
            "System.Threading",
            "System.Threading.Tasks",
            "System.Net",
            "System.Net.Http",
            "System.Net.Http.Headers",
            "System.Text",
            "System.Text.Json",
        };

        var stringBuilder = new StringBuilder();

        if (additionalUsings is not null)
        {
            finalList = baseList.Union(additionalUsings);
        }
        else
        {
            finalList = baseList;
        }

        foreach (var usingString in finalList)
        {
            stringBuilder.AppendLine($"using {usingString};");
        }

        return stringBuilder.ToString();
    }

    private string AddPrepareRequestDelegate() => """
        public Action<HttpClient, HttpRequestMessage> PrepareRequest { get; set; } = (HttpClient client, HttpRequestMessage httpRequestMessage) => {};
        """;

    private string AddProcessResponseDelegate() => """
        public Action<HttpClient, HttpResponseMessage> ProcessResponse { get; set; } = (HttpClient client, HttpResponseMessage httpResponseMessage) => {};
        """;

    private string AddHeaderHelperMethod() => """
        public void AddClientWideHeader(string headerKey, string headerValue)
        {
            this.clientWideHeaders.Add(headerKey, headerValue);
        }
        """;

    private string AddSetClientAuthorizationHeaderHelperMethod() => """
        public void SetClientAuthorizationHeader(string headerKey, string headerValue)
        {
            this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(headerKey, headerValue);
        }
        """;

    private string AddDeserializeMethod() => """
        protected virtual async Task<ApiResponse<T>> DeserializeResponse<T>(HttpResponseMessage response, bool isStream, CancellationToken cancellationToken)
        {
            if (response == null)
            {
                return new ApiResponse<T>(default, 0, "The response was null.") { ErrorResponse = new ApiError() };
            }

            if (response.Content == null)
            {
                return new ApiResponse<T>(default, (int)response.StatusCode, "There was no content.") { ErrorResponse = new ApiError() };
            }

            if (isStream)
            {
                try
                {
                    using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    {
                        var result = await JsonSerializer.DeserializeAsync<T>(responseStream, this.jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
                        return new ApiResponse<T>(result, (int)response.StatusCode);
                    }
                }
                catch (JsonException exception)
                {
                    return new ApiResponse<T>(default, (int)response.StatusCode, $"Failed to deserialize stream of type {typeof(T).FullName}.") { Exception = exception };
                }
            }
            else
            {
                var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    if (typeof(T) == typeof(string))
                    {
                        return new ApiResponse<T>((T)(object)responseText, (int)response.StatusCode);
                    }

                    var result = JsonSerializer.Deserialize<T>(responseText, this.jsonSerializerOptions);
                    return new ApiResponse<T>(result, (int)response.StatusCode);
                }
                catch (JsonException exception)
                {
                    return new ApiResponse<T>(default, (int)response.StatusCode, $"Failed to deserialize object of type {typeof(T).FullName}.") { Exception = exception };
                }
            }
        }
        """;

    private string AddApiResponseClass() => """
        public class ApiResponse
        {
            public ApiResponse(int statusCode, string message = "")
            {
                this.StatusCode = statusCode;
                this.Message = message;
            }

            public int StatusCode { get; }
            public string Message { get; }
            public bool IsError => this.ErrorResponse != null || this.Exception != null;
            public Exception? Exception { get; set; }
            public object? ErrorResponse { get; set; }
        }

        public class ApiResponse<T> : ApiResponse
        {
            public ApiResponse(T successResult, int statusCode, string message = "") : base(statusCode, message)
            {
                SuccessResponse = successResult;
            }

            public T SuccessResponse { get; set; }
        }

        public class ApiError {}
        """;

    private string AddPrepareRequestMessageMethod() => """
        private HttpRequestMessage PrepareRequestMessage<TRequest>(Uri endpoint, TRequest? requestObject, HttpMethod httpMethod, Dictionary<string, string> headers, Action<HttpClient, HttpRequestMessage> prepareSingleRequest = null)
        {
            var request = new HttpRequestMessage();
            this.PrepareRequest(this.httpClient, request);
            request.RequestUri = endpoint;
            request.Method = httpMethod;

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            foreach (var header in this.clientWideHeaders)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (prepareSingleRequest is not null)
            {
                prepareSingleRequest(this.httpClient, request);
            }

            if (requestObject is not null)
            {
                var requestJson = JsonSerializer.Serialize(requestObject, this.jsonSerializerOptions);
                request.Content = new StringContent(requestJson);
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            }

            return request;
        }
        """;

    private string AddPostJsonHelperMethod() => """
        private async Task<HttpResponseMessage> SendJsonAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using var timeout = new CancellationTokenSource(this.httpClient.Timeout);
            try
            {
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
                using (request)
                {
                    return await this.httpClient.SendAsync(request, linked.Token);

                }
            }
            catch (TaskCanceledException)when (timeout.IsCancellationRequested)
            {
                throw new TimeoutException($"Did not receive an answer from the service within a timespan of {this.httpClient.Timeout}.");
            }
            //catch (HttpRequestException)
            //{
            //    throw new TimeoutException($"Did not receive an answer from the service within a timespan of {this.httpClient.Timeout}.");
            //}
        }
        """;
}
