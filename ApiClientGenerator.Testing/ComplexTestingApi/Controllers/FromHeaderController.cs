using ComplexTestingApi.Models;
using ComplexTestingApi.Models.FromHeader;
using Microsoft.AspNetCore.Mvc;

namespace ComplexTestingApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class FromHeaderController : ControllerBase
{
    [HttpGet]
    public ActionResult<AllPropertiesHeaderAttributed> ClassParameterWithAttributeAllPropertiesAttributed([FromHeader] AllPropertiesHeaderAttributed allPropertiesAttributed)
    {
        return Ok(allPropertiesAttributed);
    }

    [HttpGet]
    public ActionResult<string> PrimitiveParameterWithAttribute([FromHeader] string soloString)
    {
        return Ok(soloString);
    }

    [HttpGet]
    public ActionResult<NoPropertiesAttributedClass> ClassParameterWithAttribute(NoPropertiesAttributedClass noPropertiesAttributedClass)
    {
        return Ok(noPropertiesAttributedClass);
    }

    [HttpGet]
    public ActionResult<SomePropertiesHeaderAttributed> ClassParameterWithAttributeSomePropertiesAttributed([FromHeader] SomePropertiesHeaderAttributed somePropertiesAttributed)
    {
        return Ok(somePropertiesAttributed);
    }


    [HttpGet]
    public ActionResult<string> PrimitiveParameterWithAttributeNamed([FromHeader(Name = "x-solo")] string soloString)
    {
        return Ok(soloString);
    }

    [HttpGet]
    public ActionResult<AllPropertiesHeaderAttributed> ClassParameterWithoutAttributeAllPropertiesAttributed(AllPropertiesHeaderAttributed allPropertiesAttributed)
    {
        return Ok(allPropertiesAttributed);
    }

    // check if you need builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressInferBindingSourcesForParameters = true);
    [HttpGet]
    public ActionResult<SomePropertiesHeaderAttributed> ClassParameterWithoutAttributeSomePropertiesAttributed(SomePropertiesHeaderAttributed somePropertiesAttributed)
    {
        return Ok(somePropertiesAttributed);
    }

    [HttpGet]
    public ActionResult<string> PrimitiveParameterNoAttribute(string soloString)
    {
        return Ok(soloString);
    }

    [HttpGet]
    public ActionResult ClassParameterNoAttribute(NoPropertiesAttributedClass noPropertiesAttributedClass)
    {
        return Ok(noPropertiesAttributedClass);
    }
}
