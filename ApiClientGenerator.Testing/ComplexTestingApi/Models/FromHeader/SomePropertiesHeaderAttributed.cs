using Microsoft.AspNetCore.Mvc;

namespace ComplexTestingApi.Models.FromHeader;

public class SomePropertiesHeaderAttributed
{
    [FromHeader]
    public string? ExampleString { get; set; }

    [FromHeader(Name = "special-name")]
    public double ExampleDouble { get; set; }
    public int ExampleInteger { get; set; }
}

