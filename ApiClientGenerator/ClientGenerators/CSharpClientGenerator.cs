﻿using ApiGenerator.Extensions;
using ApiGenerator.Models;
using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ApiGenerator.ClientGenerators;

public class CSharpClientGenerator : ClientGeneratorBase
{
    public CSharpClientGenerator( string projectName) : base(projectName) { }

    public override void GenerateClient(IEnumerable<ControllerClientDetails> controllerClientDetails, string directoryPath)
    {
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
            if (Configuration.UseInterfacesForClients)
            {
                clientCodeStringBuilder.AppendLine(this.AddClientInterfaceWithMethods(controllerClient));
            }

            clientCodeStringBuilder.AppendLine(this.AddClientImplementation(controllerClient));
        }

        var finalClientCodeString = $$"""
            // <auto-generated>
            //     This file was auto generated.
            // </auto-generated>

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

        foreach (var method in controllerClientDetails.HttpMethods)
        {
            methodsBuilder.AppendLine(this.GenerateSingleMethodWithoutCancellationToken(method));
            methodsBuilder.AppendLine(this.GenerateSingleEndpointMethod(method));
        }

        var partialString = Configuration.UsePartialClientClasses ? " partial" : string.Empty;
        var interfaceString = Configuration.UseInterfacesForClients ? $": I{controllerClientDetails.Name}" : string.Empty;

        return $$"""
            public{{partialString}} class {{controllerClientDetails.Name}} {{interfaceString}}
            {
                {{this.AddHttpClientConstructorWithField(controllerClientDetails.Name)}}
                {{methodsBuilder}}
                {{this.AddPrepareRequestMessageMethod()}}
                {{this.AddPostJsonHelperMethod()}}
                {{this.AddDeserializeMethod()}}
                {{this.AddPrepareRequestDelegate()}}
                {{this.AddProcessResponseDelegate()}}
            }
            """;
    }

    private string GenerateSingleEndpointMethod(ControllerMethodDetails methodDetails)
    {
        // CODE FOR FromForm !!!!!!!!!!!!!!!!
        /*
         
            var formBoundaryGuid = Guid.NewGuid().ToString();
            var formContent = new MultipartFormDataContent(formBoundaryGuid);
            formContent.Headers.Remove("Content-Type");
            formContent.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data; boundary=" + formBoundaryGuid);

            //if (name != null)
            //{
            //    formContent.Add(new System.Net.Http.StringContent(ConvertToString(name, System.Globalization.CultureInfo.InvariantCulture)), "Name");
            //}

            //if (favoriteDish != null)
            //{
            //    formContent.Add(new System.Net.Http.StringContent(ConvertToString(favoriteDish, System.Globalization.CultureInfo.InvariantCulture)), "FavoriteDish");
            //}
            request.Content = formContent;
          */


        var returnTypeString = methodDetails.HasReturnType ? $"Task<ApiResponse<{methodDetails.ReturnTypeString}>>" : "Task<ApiResponse>";
        var parameterString = methodDetails.HasParameters ? $"{methodDetails.ParameterString}, " : string.Empty;
        var parameterCheckStringBuilder = new StringBuilder();
        var routeQueryParamStringBuilder = new StringBuilder();
        var headerKeyValuesStringBuilder = new StringBuilder();
        ParameterDetails bodyParameter = null;
        var hasHeaderParameter = false;

        foreach (var parameter in methodDetails.Parameters)
        {
            // add null check if possible
            if (parameter.Value.ParameterSymbol.Type.IsNullable())
            {
                parameterCheckStringBuilder.Append($$"""
                if({{parameter.Key}} == null)
                {
                    throw new ArgumentNullException(nameof({{parameter.Key}}), "The object can´t be null");
                }
                """);
            }

            // assign a possible body parameter
            if (parameter.Value.AttributeDetails.HasBodyAttribute)
            {
                bodyParameter = parameter.Value;
            }

            if (parameter.Value.AttributeDetails.HasHeaderAttribute)
            {
                hasHeaderParameter = true;
            }

            // add route query parameters e.g. /api/clients/{id}
            if (methodDetails.HasRouteQueryParameters)
            {
                if (parameter.Value.IsRouteQueryParameter)
                {
                    routeQueryParamStringBuilder.AppendLine($$"""
                        routeBuilder.Replace("{{{parameter.Key}}}", Uri.EscapeDataString({{parameter.Key}}.ToString()));
                        """);
                }
            }

            // add query values e.g. ?example=10
            if (parameter.Value.AttributeDetails.HasQueryAttribute)
            {
                routeQueryParamStringBuilder.AppendLine($"""
                        routeBuilder.Append($"{parameter.Value.QueryString}");
                        """);
            }

            // add header values
            if (parameter.Value.AttributeDetails.HasHeaderAttribute)
            {
                foreach (var headerKey in parameter.Value.HeaderKeys)
                {
                    headerKeyValuesStringBuilder.AppendLine($$"""
                    { "{{headerKey}}", {{headerKey}}.ToString()},
                    """);
                }
            }
        }

        var headerString = hasHeaderParameter ? $$"""
            var headers = new Dictionary<string, string>()
            {
                {{headerKeyValuesStringBuilder}}
            };
            """ : string.Empty;

        var methodCallString = bodyParameter is not null ? 
            $"var httpRequestMessage = this.PrepareRequestMessage<{bodyParameter.ParameterTypeString}>(uri, {bodyParameter.Name}, new HttpMethod(\"{methodDetails.HttpMethod.Method}\"), {(hasHeaderParameter ? "headers, " : "null, ")}prepareSingleRequest);"
            :  $"var httpRequestMessage = this.PrepareRequestMessage<object>(uri, null, new HttpMethod(\"{methodDetails.HttpMethod.Method}\"), {(hasHeaderParameter ? "headers, " : "null, ")}prepareSingleRequest);";

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
            if (pair.Value is not null)
            {
                var cleanClassValue = pair.Value.CheckAndSanitizeClassString();

                var methodErrorReturnString = methodDetails.ReturnType is not null ? $"""
                    return new ApiErrorResponse<{methodDetails.ReturnTypeString}, {cleanClassValue}>(default, default, 0, errorResponse{pair.Key}.Message, errorResponse{pair.Key}.Exception);
                    """: $"""
                    return new ApiErrorResponse<{cleanClassValue}>(default, 0, errorResponse{pair.Key}.Message, errorResponse{pair.Key}.Exception);
                    """;

                var methodReturnString = methodDetails.ReturnType is not null ? $"""
                    return new ApiErrorResponse<{methodDetails.ReturnTypeString}, {cleanClassValue}>(default, result{pair.Key}.Response, statusCode);
                    """ : $"""
                    return new ApiErrorResponse<{cleanClassValue}>(result{pair.Key}.Response, statusCode);
                    """;

                switchStringBuilder.AppendLine($$"""
                    case {{pair.Key}}:
                        var result{{pair.Key}} = await this.DeserializeResponse<{{cleanClassValue}}>(httpClientResponse, false, cancellationToken);

                        if (result{{pair.Key}} is ApiErrorResponse<{{cleanClassValue}}, object> errorResponse{{pair.Key}})
                        {
                            {{methodErrorReturnString}}
                        }

                        {{methodReturnString}}
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
        
        var defaultReturnString = methodDetails.ReturnType is not null ? $"<{methodDetails.ReturnTypeString}, object>(default, default, " : "(";

        if (methodDetails.ReturnType is null && methodDetails.ReturnTypes.Count == 0)
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
                    return new ApiErrorResponse{{defaultReturnString}}statusCode, $"Encountered unexpected statuscode {statusCode}. {httpClientResponse.ReasonPhrase ?? string.Empty}");
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
        var interfaceMethodsStringBuilder = new StringBuilder();

        foreach (var method in controllerClientDetails.HttpMethods)
        {
            interfaceMethodsStringBuilder.AppendLine(this.GenerateInterfaceMethod(method));
        }

        var partialString = Configuration.UsePartialClientClasses ? " partial" : string.Empty;

        return $$"""
            public{{partialString}} interface I{{controllerClientDetails.Name}}
            {
                {{interfaceMethodsStringBuilder}}
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
                    return new ApiErrorResponse<T, object>(default, default, (int)response.StatusCode, $"Failed to deserialze stream of type {typeof(T).FullName}.", exception);
                }
            }
            else
            {
                var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    var result = JsonSerializer.Deserialize<T>(responseText, this.jsonSerializerOptions);
                    return new ApiResponse<T>(result, (int)response.StatusCode, string.Empty);
                }
                catch (JsonException exception)
                {
                    return new ApiErrorResponse<T, object>(default, default, (int)response.StatusCode, $"Failed to deserialze response of type {typeof(T).FullName}.", exception);
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
        }
        
        public class ApiErrorResponse : ApiResponse
        {
            public ApiErrorResponse(int statusCode, string message = "", Exception? exception = null) : base(statusCode, message)
            {
                this.Exception = exception;
            }
        
            public Exception? Exception { get; }
        }

        public class ApiErrorResponse<TErrorResponse> : ApiResponse<TErrorResponse>
        {
            public ApiErrorResponse(TErrorResponse response, int statusCode, string message = "", Exception? exception = null)
                : base(response, statusCode, message)
            {
                Response = response;
                Exception = exception;
            }

            public new TErrorResponse? Response { get; }
            public Exception? Exception { get; }
        }
        
        public class ApiResponse<TResponse> : ApiResponse
        {
            public ApiResponse(TResponse response, int statusCode, string message = "")
                : base(statusCode, message)
            {
                this.Response = response;
            }
        
            public TResponse Response { get; set; }
        }
        
        public class ApiErrorResponse<TExpectedResponse, TErrorResponse> : ApiResponse<TExpectedResponse>
        {
            public ApiErrorResponse(TExpectedResponse exprectedResponse, TErrorResponse response, int statusCode, string message = "", Exception? exception = null)
                : base(exprectedResponse, statusCode, message)
            {
                Response = response;
                Exception = exception;
            }
        
            public new TErrorResponse? Response { get; }
            public Exception? Exception { get; }
        }
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
        }
        """;
}