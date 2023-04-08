using Microsoft.AspNetCore.Mvc;
using TestingPlayground.Models;
using TestingPlayground.Models.FromHeader;

namespace TestingPlayground.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class FromHeaderController : ControllerBase
{
    [HttpGet]
    public ActionResult ClassParameterWithAttributeAllPropertiesAttributed([FromHeader] AllPropertiesHeaderAttributed allPropertiesAttributed)
    {
        return Ok();
    }

    [HttpGet]
    public ActionResult PrimitiveParameterWithAttribute([FromHeader] string soloString)
    {
        return Ok();
    }

    [HttpGet]
    public ActionResult ClassParameterWithAttribute(NoPropertiesAttributedClass noPropertiesAttributedClass)
    {
        return Ok();
    }

    [HttpGet]
    public ActionResult ClassParameterWithAttributeSomePropertiesAttributed([FromHeader] SomePropertiesHeaderAttributed somePropertiesAttributed)
    {
        return Ok();
    }

    [HttpGet]
    public ActionResult PrimitiveParameterWithAttributeNamed([FromHeader(Name ="x-solo")] string soloString)
    {
        return Ok();
    }

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
    public ActionResult PrimitiveParameterNoAttribute(string soloString)
    {
        return Ok();
    }

    [HttpGet]
    public ActionResult ClassParameterNoAttribute(NoPropertiesAttributedClass noPropertiesAttributedClass)
    {
        return Ok();
    }
}