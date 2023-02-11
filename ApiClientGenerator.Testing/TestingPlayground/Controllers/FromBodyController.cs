using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestingPlayground.Models;

namespace TestingPlayground.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class FromBodyController : ControllerBase
{
    [HttpPost]
    public ActionResult ClassParameterNoAttribute(NoPropertiesAttributedClass simpleBodyClass)
    {
        return Ok();
    }

    [HttpPost]
    public ActionResult ClassParameterWithAttribute([FromBody] NoPropertiesAttributedClass simpleBodyClass)
    {
        return Ok();
    }

    [HttpPost]
    public ActionResult PrimitiveParameterWithAttribute([FromBody] string simpleString)
    {
        return Ok();
    }
}
