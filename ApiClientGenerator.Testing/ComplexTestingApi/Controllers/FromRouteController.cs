using ComplexTestingApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace ComplexTestingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FromRouteController : ControllerBase
{
    [Route("allProps/{exampleString}/{exampleInteger}")]
    [HttpGet]
    public ActionResult Class_Parameter_WithoutAttribute_AllPropertiesAttributed([FromRoute] NoPropertiesAttributedClass allPropertiesAttributed)
    {
        return Ok(allPropertiesAttributed);
    }

    [Route("Primitive")]
    [HttpGet]
    public ActionResult PrimitiveParameterWithAttribute([FromRoute] string soloString)
    {
        return Ok(soloString);
    }

    [Route("route/{id}")]
    [HttpGet]
    public ActionResult PrimitiveParameterNoAttribute(string id)
    {
        return Ok(id);
    }
}
