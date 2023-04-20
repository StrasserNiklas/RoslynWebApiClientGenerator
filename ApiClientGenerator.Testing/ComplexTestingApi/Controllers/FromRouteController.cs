using ComplexTestingApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace ComplexTestingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FromRouteController : ControllerBase
{
    [Route("allProps/{ExampleString}/{ExampleInteger}")]
    [HttpGet]
    public ActionResult<NoPropertiesAttributedClass> ClassParameterWithAttributeNoPropertiesAttributed([FromRoute] NoPropertiesAttributedClass allPropertiesAttributed)
    {
        return Ok(allPropertiesAttributed);
    }

    [Route("Primitive/{soloString}")]
    [HttpGet]
    public ActionResult<string> PrimitiveParameterWithAttribute([FromRoute] string soloString)
    {
        return Ok(soloString);
    }

    [Route("route/{id}")]
    [HttpGet]
    public ActionResult<int> PrimitiveParameterNoAttribute(int id)
    {
        return Ok(id);
    }
}
