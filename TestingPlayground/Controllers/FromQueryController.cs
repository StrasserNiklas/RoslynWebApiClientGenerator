using Microsoft.AspNetCore.Mvc;

namespace TestingPlayground.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FromQueryController : ControllerBase
    {
        [HttpGet]
        public ActionResult Class_Parameter_WithoutAttribute_AllPropertiesAttributed(AllPropertiesAttributed allPropertiesAttributed)
        {
            return Ok();
        }

        // check if you need builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressInferBindingSourcesForParameters = true);
        [HttpGet]
        public ActionResult Class_Parameter_WithoutAttribute_SomePropertiesAttributed(SomePropertiesAttributed somePropertiesAttributed)
        {
            return Ok();
        }

        [HttpGet]
        public ActionResult Class_Parameter_WithAttribute_SomePropertiesAttributed([FromQuery] SomePropertiesAttributed somePropertiesAttributed)
        {
            return Ok();
        }

        [HttpGet]
        public ActionResult Class_Parameter_WithAttribute_AllPropertiesAttributed([FromQuery] AllPropertiesAttributed somePropertiesAttributed)
        {
            return Ok();
        }

        [HttpGet]
        public ActionResult Class_Parameter_WithAttribute([FromQuery] SimpleQueryClass simpleQueryClass)
        {
            return Ok();
        }

        [HttpGet]
        public ActionResult Primitive_Parameter_WithAttribute([FromQuery] string soloString)
        {
            return Ok();
        }

        [HttpGet]
        public ActionResult Primitive_Parameter_NoAttribute(string soloString)
        {
            return Ok();
        }
    }

    public class SimpleQueryClass
    {
        public string ExampleString { get; set; }
        public int ExampleInteger { get; set; }
    }

    public class AllPropertiesAttributed
    {
        [FromQuery]
        public string ExampleString { get; set; }
        [FromQuery]
        public int ExampleInteger { get; set; }
    }

    public class SomePropertiesAttributed
    {
        [FromQuery]
        public string ExampleString { get; set; }
        public int ExampleInteger { get; set; }
    }

    public class NoPropertiesAttributed
    {
        public string ExampleString { get; set; }
        public int ExampleInteger { get; set; }
    }
}
