using ApiTest;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<ITestService,TestService>();

builder.Services.AddSwaggerGen(options => options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo()
{
    Title = "bla",

}));

var app = builder.Build();

// Configure the HTTP request pipeline.

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapPost("/whatever", (Todo todo) => { });

app.MapGet("/weatherforecast", () =>
{
var forecast = Enumerable.Range(1, 5).Select(index =>
    new WeatherForecast
    (
        DateTime.Now.AddDays(index),
        Random.Shared.Next(-20, 55),
        summaries[Random.Shared.Next(summaries.Length)]
    ))
    .ToArray();
return 2;
});


app.Run();


app.UseSwagger(x =>
{
    x.RouteTemplate = "ui/swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(x =>
{
    x.RoutePrefix = "ui/swagger";
});

app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

app.Run();


internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public record Todo(string what);