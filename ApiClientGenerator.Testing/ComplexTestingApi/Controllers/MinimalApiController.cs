using Microsoft.AspNetCore.Mvc;

namespace ComplexTestingApi.Controllers;

public static class MinimalApiController
{
    public static void AddMinimalEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("/todoitems", () => { return Results.Ok(new Todo() { Id = 1, IsComplete = true, Name = "todo" }); });

        endpointRouteBuilder.MapPost("/todoitems", async ([FromBody] Todo todo) =>
        {
            return Results.Ok();
        });

        endpointRouteBuilder.MapPut("/todoitems/{id}", async (int id, Todo inputTodo) =>
        {
            if (inputTodo is null) return Results.NotFound();

            inputTodo.Id = id;
            return Results.Ok(inputTodo);
        });

        endpointRouteBuilder.MapDelete("/todoitems/{id}", (int id, [FromBody] Todo deleteTodo) =>
        {
            if (deleteTodo is Todo)
            {
                deleteTodo.Id = id;
                return Results.Ok(deleteTodo);
            }

            return Results.NotFound();
        });
    }
}

public class Todo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
}
