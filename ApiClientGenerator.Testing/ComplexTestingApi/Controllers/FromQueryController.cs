using ComplexTestingApi.Models;
using ComplexTestingApi.Models.FromQuery;
using Microsoft.AspNetCore.Mvc;

namespace ComplexTestingApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class FromQueryController : ControllerBase
{
    [HttpGet]
    public ActionResult<AllPropertiesQueryAttributed> ClassParameterWithAttributeAllPropertiesAttributed([FromQuery] AllPropertiesQueryAttributed allPropertiesQueryAttributed)
    {
        return Ok(allPropertiesQueryAttributed);
    }

    [HttpGet]
    public ActionResult<NoPropertiesAttributedClass> ClassParameterWithAttribute([FromQuery] NoPropertiesAttributedClass noPropertiesAttributedClass)
    {
        return Ok(noPropertiesAttributedClass);
    }

    [HttpGet]
    public ActionResult<string> PrimitiveParameterWithAttribute([FromQuery] string soloString)
    {
        return Ok(soloString);
    }

    [HttpGet]
    public ActionResult<string> PrimitiveParameterNoAttribute(string soloString)
    {
        return Ok(soloString);
    }

    [HttpGet]
    public ActionResult<SomePropertiesQueryAttributed> ClassParameterWithAttributeSomePropertiesAttributed([FromQuery] SomePropertiesQueryAttributed somePropertiesAttributed)
    {
        return Ok(somePropertiesAttributed);
    }

    // for all methods below check if you need builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressInferBindingSourcesForParameters = true);
    [HttpGet]
    public ActionResult<NoPropertiesAttributedClass> ClassParameterNoAttribute(NoPropertiesAttributedClass soloString)
    {
        return Ok(soloString);
    }

    [HttpGet]
    public ActionResult<AllPropertiesQueryAttributed> ClassParameterWithoutAttributeAllPropertiesAttributed(AllPropertiesQueryAttributed allPropertiesAttributed)
    {
        return Ok(allPropertiesAttributed);
    }

    [HttpGet]
    public ActionResult<SomePropertiesQueryAttributed> ClassParameterWithoutAttributeSomePropertiesAttributed(SomePropertiesQueryAttributed somePropertiesAttributed)
    {
        return Ok(somePropertiesAttributed);
    }
}
