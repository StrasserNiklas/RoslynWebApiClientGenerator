using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressInferBindingSourcesForParameters = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//app.MapGet("/todoitems", (HttpContext http) => { return Results.Ok(new Todo()); });

////app.Mapme

//app.MapPost("/todoitems", ([FromBody] Todo todo) =>
//{
//    return Results.Created($"/todoitems/{todo.Id}", todo);
//});

//app.MapPut("/todoitems/{id}", (int id, Todo inputTodo) =>
//{

//    if (inputTodo is null) return Results.NotFound();

//    return Results.NoContent();
//});

//app.MapDelete("/todoitems/{id}", (int id, Todo db) =>
//{
//    if (db is Todo todo)
//    {
//        return Results.Ok(todo);
//    }

//    return Results.NotFound();
//});

app.Run();

class Todo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
}