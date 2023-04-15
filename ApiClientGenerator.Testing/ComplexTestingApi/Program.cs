using AspNetCore.Authentication.ApiKey;
using ComplexTestingApi.Controllers;
using Microsoft.AspNetCore.HttpLogging;
using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace ComplexTestingApi;

internal class AddApiKeyHeaderParameter : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        if (context.ControllerType.Name == "AuthorizationController")
        {
            context.OperationDescription.Operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Api-Key",
                Kind = OpenApiParameterKind.Header,
                Type = JsonObjectType.String,
                IsRequired = true,
                Description = "Api-Key"
            });
        }

        return true;
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        //builder.Services.AddSwaggerGen();
        builder.Services.AddOpenApiDocument(document =>
        {
            //document.OperationProcessors.Add(new OperationSecurityScopeProcessor("Api-Key"));
            document.OperationProcessors.Add(new AddApiKeyHeaderParameter());

            //document.AddSecurity("Api-Key", new OpenApiSecurityScheme
            //{
            //    Description = "Api Key security",
            //    Type = OpenApiSecuritySchemeType.ApiKey,
            //    Name = "Api-Key",
            //    In = OpenApiSecurityApiKeyLocation.Header
            //});
        });

        builder.Services.AddHttpLogging(logging =>
        {
            // Customize HTTP logging here.
            logging.LoggingFields = HttpLoggingFields.RequestPath;
        });

        builder.Services
            .AddAuthentication(ApiKeyDefaults.AuthenticationScheme)
            .AddApiKeyInHeader(options =>
            {
                options.Realm = "Testing api";
                options.KeyName = "Api-Key";
                options.Events = new ApiKeyEvents
                {
                    OnValidateKey = context =>
                    {
                        if (context.ApiKey == "TestApiKey")
                            context.ValidationSucceeded();
                        else
                            context.ValidationFailed();

                        return Task.CompletedTask;
                    },
                };
            });


        //builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressInferBindingSourcesForParameters = true);
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        //if (app.Environment.IsDevelopment())
        //{
        //    app.UseSwagger();
        //    app.UseSwaggerUI();
        //}

        app
            .UseOpenApi(settings =>
            {
                //settings.
                //settings.op
                settings.PostProcess = (document, _) =>
                {
                    document.Schemes = new List<OpenApiSchema>
                    {
                        OpenApiSchema.Https,
                        OpenApiSchema.Http
                    };
                };
            })
             .UseSwaggerUi3(settings => settings.Path = "/swagger");

        app.UseHttpsRedirection();

        app.UseAuthorization();
        app.UseAuthentication();

        app.MapControllers();
        app.AddMinimalEndpoints();

        app.Run();
    }
}