using ComplexTestingApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace ComplexTestingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CombinedAttributesController : ControllerBase
{
    [Route("Route1/{id}")]
    [HttpPost]
    public ActionResult<CombinedClass> ComplexHeaderSimpleRouteSimpleQuerySimpleBody(int id, [FromQuery] string name, [FromBody] int intervall, [FromHeader] NoPropertiesAttributedClass simpleClass)
    {
        return Ok(new CombinedClass() { Id = id, Intervall = intervall, Name = name, SimpleBodyClass = simpleClass });
    }

    [Route("Route2/{exampleString}/{exampleInteger}")]
    [HttpPost]
    public ActionResult<CombinedClass> SimpleHeaderComplexRouteSimpleQuerySimpleBody(int id, [FromQuery] string name, [FromBody] int intervall, [FromRoute] NoPropertiesAttributedClass simpleClass)
    {
        return Ok(new CombinedClass() { Id = id, Intervall = intervall, Name = name, SimpleBodyClass = simpleClass });
    }

    [Route("Route3/{id}")]
    [HttpPost]
    public ActionResult<CombinedClass> SimpleHeaderSimpleRouteComplexQuerySimpleBody(int id, [FromHeader] string name, [FromBody] int intervall, [FromQuery] NoPropertiesAttributedClass simpleClass)
    {
        return Ok(new CombinedClass() { Id = id, Intervall = intervall, Name = name, SimpleBodyClass = simpleClass });
    }

    [Route("Route4/{id}")]
    [HttpPost]
    public ActionResult<CombinedClass> SimpleHeaderSimpleRouteSimpleQueryComplexBody(int id, [FromQuery] string name, [FromHeader] int intervall, [FromBody] NoPropertiesAttributedClass simpleClass)
    {
        return Ok(new CombinedClass() { Id = id, Intervall = intervall, Name = name, SimpleBodyClass = simpleClass });
    }

    [Route("Route5/{exampleString}/{exampleInteger}")]
    [HttpPost]
    public ActionResult<CombinedClass> ComplexHeaderComplexRouteComplexQueryComplexBody(
        [FromRoute] NoPropertiesAttributedClass simpleClass1,
        [FromQuery] NoPropertiesAttributedClass simpleClass2, 
        [FromHeader] NoPropertiesAttributedClass simpleClass3, 
        [FromBody] NoPropertiesAttributedClass simpleClass4)
    {
        return Ok(new ComplexCombinedClass() { SimpleBodyClass1 = simpleClass1, SimpleBodyClass2 = simpleClass2, SimpleBodyClass3 = simpleClass3, SimpleBodyClass4 = simpleClass4 });
    }
}

public class CombinedClass
{
    public int Id { get; set; }
    public string Name { get; set; } 
    public int Intervall { get; set; } 
    public NoPropertiesAttributedClass SimpleBodyClass { get; set; }
}

public class ComplexCombinedClass
{
    public NoPropertiesAttributedClass SimpleBodyClass1 { get; set; }
    public NoPropertiesAttributedClass SimpleBodyClass2 { get; set; }
    public NoPropertiesAttributedClass SimpleBodyClass3 { get; set; }

    public NoPropertiesAttributedClass SimpleBodyClass4 { get; set; }
}