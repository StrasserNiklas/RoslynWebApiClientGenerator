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
    public ActionResult<AllPropertiesQueryAttributed> AuthenticationTest([FromQuery] AllPropertiesQueryAttributed allPropertiesAttributed)
    {
        return Ok(allPropertiesAttributed);
    }
}