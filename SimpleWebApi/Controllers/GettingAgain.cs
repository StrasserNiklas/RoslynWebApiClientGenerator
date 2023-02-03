using Microsoft.AspNetCore.Mvc;

namespace SimpleWebApi.Controllers
{
    public partial class PartialTestingController
    {
        [HttpGet]
        [Route("getting")]
        public async Task<IActionResult> Getting(Response response)
        {
            return Ok("hallo");
        }
    }

    
}
