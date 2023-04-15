using AspNetCore.Authentication.ApiKey;
using ComplexTestingApi.Models.FromQuery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplexTestingApi.Controllers;


[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(AuthenticationSchemes = ApiKeyDefaults.AuthenticationScheme)]
public class AuthorizationController : ControllerBase
{
    [HttpGet]
    public ActionResult<AllPropertiesQueryAttributed> AuthTest(AllPropertiesQueryAttributed allPropertiesAttributed)
    {
        return Ok(allPropertiesAttributed);
    }

    [HttpPost]
    public ActionResult<TestEnum> AuthTestPost(TestEnum allPropertiesAttributed)
    {
        return Ok(allPropertiesAttributed);
    }
}

public enum TestEnum
{
    Value1 = 1,
    Value2 = 2,
    Value3 = 4
}

