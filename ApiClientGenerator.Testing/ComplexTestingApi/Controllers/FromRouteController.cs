using ComplexTestingApi.Models.FromQuery;
using Microsoft.AspNetCore.Mvc;

namespace ComplexTestingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FromRouteController : ControllerBase
{
    [Route("allProps")]
    [HttpGet]
    public ActionResult Class_Parameter_WithoutAttribute_AllPropertiesAttributed([FromRoute] AllPropertiesQueryAttributed allPropertiesAttributed)
    {
        return Ok();
    }

    [Route("Primitive")]
    [HttpGet]
    public ActionResult PrimitiveParameterWithAttribute([FromRoute] string soloString)
    {
        return Ok();
    }

    [Route("route/{id}")]
    [HttpGet]
    public ActionResult PrimitiveParameterNoAttribute(string id)
    {
        return Ok();
    }
}
