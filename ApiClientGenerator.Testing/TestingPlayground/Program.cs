
using Microsoft.AspNetCore.Mvc;
using NSwag;

namespace TestingPlayground
{
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
            builder.Services.AddOpenApiDocument();

            builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressInferBindingSourcesForParameters = true);
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


            app.MapControllers();

            app.Run();
        }
    }
}