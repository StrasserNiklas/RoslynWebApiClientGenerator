using Microsoft.AspNetCore.Mvc;

namespace ComplexTestingApi.Models.FromHeader;

public class AllPropertiesHeaderAttributed
{
    [FromHeader]
    public string? ExampleString { get; set; }
    [FromHeader]
    public int ExampleInteger { get; set; }
}

