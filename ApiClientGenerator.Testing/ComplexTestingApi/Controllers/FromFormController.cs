using ComplexTestingApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace ComplexTestingApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class FromFormController : ControllerBase
{
    [HttpPost]
    public ActionResult<NoPropertiesAttributedClass> ClassParameterWithAttribute([FromForm] NoPropertiesAttributedClass simpleBodyClass)
    {
        return Ok(simpleBodyClass);
    }

    [HttpPost]
    public ActionResult<string> SimpleParameterWithAttribute([FromForm] string primitive)
    {
        return Ok(primitive);
    }
}
