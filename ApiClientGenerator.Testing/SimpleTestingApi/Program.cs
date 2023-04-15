using Microsoft.AspNetCore.HttpLogging;

namespace SimpleTestingApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddHttpLogging(logging =>
        {
            // Customize HTTP logging here.
            logging.LoggingFields = HttpLoggingFields.RequestPath;
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
        }

        app.UseHttpsRedirection();
        app.UseHttpLogging();
        app.UseAuthorization();

        app.UseCors();
        app.MapControllers();

        app.Run();
    }
}