var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(options => options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo()
{
    Title = "bla",

}));

var app = builder.Build();

// Configure the HTTP request pipeline.


app.UseSwagger(x =>
{
    x.RouteTemplate = "ui/swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(x =>
{
    x.RoutePrefix = "ui/swagger";
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
