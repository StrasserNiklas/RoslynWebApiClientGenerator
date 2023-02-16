using Microsoft.AspNetCore.Mvc;

namespace TestingPlayground.Models.FromHeader;

public class AllPropertiesHeaderAttributed
{
    [FromHeader]
    public string ExampleString { get; set; }
    [FromHeader]
    public int ExampleInteger { get; set; }
}
