using Microsoft.AspNetCore.Mvc;

namespace ComplexTestingApi.Models.FromHeader;

public class AllPropertiesHeaderAttributed
{
    [FromHeader]
    public string? ExampleString { get; set; } // -> would be bound

    public int ExampleInteger { get; set; } // -> would not be bound
    [FromHeader]
    public int SecondInteger { get; set; } // -> would be bound
}

