using Microsoft.AspNetCore.Mvc;
using TestingContracts.Models;
using TestingContracts.Responses;

namespace SimpleTestingApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class TestController : ControllerBase
{
    [HttpPost]
    public ActionResult<DataResponse<string>> Test([FromBody] Car car)
    {
        return Ok();
    }
}