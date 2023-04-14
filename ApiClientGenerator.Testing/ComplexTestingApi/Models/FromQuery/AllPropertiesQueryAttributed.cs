using Microsoft.AspNetCore.Mvc;

namespace ComplexTestingApi.Models.FromQuery;

public class AllPropertiesQueryAttributed
{
    [FromQuery]
    public string? ExampleString { get; set; }
    [FromQuery]
    public int ExampleInteger { get; set; }
}
