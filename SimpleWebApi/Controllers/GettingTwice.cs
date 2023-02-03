using Microsoft.AspNetCore.Mvc;

namespace SimpleWebApi.Controllers
{
    public partial class PartialTestingController
    {
        [HttpGet]
        [Route("gettingtwice")]
        public async Task<IActionResult> GettingTwice()
        {
            return Ok("hallo nochmal");
        }
    }
}
