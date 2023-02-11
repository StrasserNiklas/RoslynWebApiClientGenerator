using Microsoft.AspNetCore.Mvc;
using TestingPlayground.Models;

namespace TestingPlayground.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class FromHeaderController : ControllerBase
{
    [HttpGet]
    public ActionResult ClassParameterWithoutAttributeAllPropertiesAttributed(AllPropertiesHeaderAttributed allPropertiesAttributed)
    {
        return Ok();
    }

    // check if you need builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressInferBindingSourcesForParameters = true);
    [HttpGet]
    public ActionResult ClassParameterWithoutAttributeSomePropertiesAttributed(SomePropertiesHeaderAttributed somePropertiesAttributed)
    {
        return Ok();
    }

    [HttpGet]
    public ActionResult ClassParameterWithAttributeSomePropertiesAttributed([FromHeader] SomePropertiesHeaderAttributed somePropertiesAttributed)
    {
        return Ok();
    }

    [HttpGet]
    public ActionResult ClassParameterWithAttributeAllPropertiesAttributed([FromHeader] AllPropertiesHeaderAttributed somePropertiesAttributed)
    {
        return Ok();
    }

    [HttpGet]
    public ActionResult ClassParameterWithAttribute([FromHeader] NoPropertiesAttributedClass simpleQueryClass)
    {
        return Ok();
    }

    [HttpGet]
    public ActionResult PrimitiveParameterWithAttribute([FromHeader] string soloString)
    {
        return Ok();
    }

    [HttpGet]
    public ActionResult PrimitiveParameterNoAttribute(string soloString)
    {
        return Ok();
    }

    [HttpGet]
    public ActionResult ClassParameterNoAttribute(NoPropertiesAttributedClass soloString)
    {
        return Ok();
    }
}

public class AllPropertiesHeaderAttributed
{
    [FromHeader]
    public string ExampleString { get; set; }
    [FromHeader]
    public int ExampleInteger { get; set; }
}

public class SomePropertiesHeaderAttributed
{
    [FromHeader]
    public string ExampleString { get; set; }
    public int ExampleInteger { get; set; }
}