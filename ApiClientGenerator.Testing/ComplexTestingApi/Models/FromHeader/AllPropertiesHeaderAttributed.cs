using Microsoft.AspNetCore.Mvc;

namespace ComplexTestingApi.Models.FromHeader;

public class AllPropertiesHeaderAttributed
{
    [FromHeader]
    public string? ExampleString { get; set; } // -> would be bound
    [FromHeader]
    public int ExampleInteger { get; set; } // -> would not be bound
    
    //public int SecondInteger { get; set; } // -> would be bound
}

