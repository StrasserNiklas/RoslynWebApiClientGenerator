using ComplexTestingApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace ComplexTestingApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class FromBodyController : ControllerBase
{
    [HttpPost]
    public ActionResult<NoPropertiesAttributedClass> ClassParameterNoAttribute(NoPropertiesAttributedClass simpleBodyClass)
    {
        return Ok(simpleBodyClass);
    }

    [HttpPost]
    public ActionResult<NoPropertiesAttributedClass> ClassParameterWithAttribute([FromBody] NoPropertiesAttributedClass simpleBodyClass)
    {
        return Ok(simpleBodyClass);
    }

    [HttpPost]
    public ActionResult<string> PrimitiveParameterWithAttribute([FromBody] string simpleString)
    {
        return Ok(simpleString);
    }
}
