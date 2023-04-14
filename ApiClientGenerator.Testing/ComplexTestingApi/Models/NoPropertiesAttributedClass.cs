using Microsoft.AspNetCore.Mvc;

namespace ComplexTestingApi.Models;

public class NoPropertiesAttributedClass
{
    [FromHeader]
    public string? ExampleString { get; set; }

    public int ExampleInteger { get; set; }
}
