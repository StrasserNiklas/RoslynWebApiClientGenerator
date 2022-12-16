using Microsoft.AspNetCore.Mvc;

namespace ApiTest.Controllers
{
    [ApiController]
    [Route("api/betslip/[action]")]
    public class NormalControllerActionRoute : ControllerBase
    {
        [HttpGet]
        public ActionResult<Response> Get()
        {
            return Ok(new Response());
        }
    }
}