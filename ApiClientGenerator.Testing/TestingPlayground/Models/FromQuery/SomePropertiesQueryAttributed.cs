using Microsoft.AspNetCore.Mvc;

namespace TestingPlayground.Models.FromQuery;

public class SomePropertiesQueryAttributed
{
    [FromQuery]
    public string ExampleString { get; set; }
    public int ExampleInteger { get; set; }
}
