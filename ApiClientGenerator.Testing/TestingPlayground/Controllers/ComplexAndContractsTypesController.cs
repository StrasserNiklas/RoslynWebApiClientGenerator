using Microsoft.AspNetCore.Mvc;
using TestingContracts.Models;

namespace TestingPlayground.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class ComplexAndContractsTypesController : ControllerBase
{
    [HttpPost]
    public ActionResult PrimitiveParameterWithAttribute([FromBody] AccessoryFeature feature)
    {
        return Ok();
    }
}

