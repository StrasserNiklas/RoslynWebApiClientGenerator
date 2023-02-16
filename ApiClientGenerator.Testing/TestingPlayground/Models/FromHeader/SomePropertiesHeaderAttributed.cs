using Microsoft.AspNetCore.Mvc;

namespace TestingPlayground.Models.FromHeader;

public class SomePropertiesHeaderAttributed
{
    [FromHeader]
    public string ExampleString { get; set; }
    public int ExampleInteger { get; set; }
}
