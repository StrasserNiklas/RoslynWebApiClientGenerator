using Microsoft.AspNetCore.Mvc;
using TestingPlayground.Models;

namespace TestingPlayground.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class FromFormController : ControllerBase
{
    [HttpPost]
    public ActionResult ClassParameterWithAttribute([FromForm] NoPropertiesAttributedClass simpleBodyClass)
    {
        return Ok();
    }

    [HttpPost]
    public ActionResult SimpleParameterWithAttribute([FromForm] string primitive)
    {
        return Ok();
    }
}