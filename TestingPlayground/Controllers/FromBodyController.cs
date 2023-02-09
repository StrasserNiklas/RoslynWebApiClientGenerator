using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TestingPlayground.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FromBodyController : ControllerBase
    {
        [HttpPost]
        public ActionResult Class_Parameter_NoAttribute(SimpleBodyClass simpleBodyClass)
        {
            return Ok();
        }

        [HttpPost]
        public ActionResult Class_Parameter_WithAttribute([FromBody] SimpleBodyClass simpleBodyClass)
        {
            return Ok();
        }

        [HttpPost]
        public ActionResult Primitive_Parameter_WithAttribute([FromBody] string simpleString)
        {
            return Ok();
        }


    }

    public class SimpleBodyClass
    {
        public string ExampleString { get; set; }
        public int ExampleInteger { get; set; }
    }
}
