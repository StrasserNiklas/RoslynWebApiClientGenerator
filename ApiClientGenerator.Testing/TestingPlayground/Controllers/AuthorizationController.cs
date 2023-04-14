using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestingPlayground.Models.FromQuery;

namespace TestingPlayground.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(AuthenticationSchemes = ApiKeyDefaults.AuthenticationScheme)]
public class AuthorizationController : ControllerBase
{
    [HttpGet]
    public ActionResult AuthTest(AllPropertiesQueryAttributed allPropertiesAttributed)
    {
        return Ok();
    }

    [HttpPost]
    public ActionResult AuthTestPost(TestEnum allPropertiesAttributed)
    {
        return Ok();
    }
}

public enum TestEnum
{
    Value1 = 1,
    Value2 = 2,
    Value3 = 4
}
