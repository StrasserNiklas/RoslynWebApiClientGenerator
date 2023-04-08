using Microsoft.AspNetCore.Mvc;

namespace TestingPlayground.Controllers;

public static class MinimalApiController
{
    public static void AddMinimalEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        //endpointRouteBuilder.MapGet("/todoitems", (HttpContext http) => { return Results.Ok(new Todo()); });
        //endpointRouteBuilder.MapPost("/todoitems", async ([FromBody] Todo todo) =>
        //{
        //    return Results.Created($"/todoitems/{todo.Id}", todo);
        //});

        //endpointRouteBuilder.MapPut("/todoitems/{id}", async (int id, Todo inputTodo) =>
        //{
        //    if (inputTodo is null) return Results.NotFound();

        //    return Results.NoContent();
        //});

        //endpointRouteBuilder.MapDelete("/todoitems/{id}", async (int id, Todo db) =>
        //{
        //    if (db is Todo todo)
        //    {
        //        return Results.Ok(todo);
        //    }

        //    return Results.NotFound();
        //});
    }
}

public class Todo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
}
