using ComplexTestingApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace ComplexTestingApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class ProducesResponseTypeController : ControllerBase
{
    [ProducesResponseType(typeof(NoPropertiesAttributedClass), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServerSideErrorResponse), StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public ActionResult<NoPropertiesAttributedClass> ClassResponseOnSuccessWithBody([FromBody] NoPropertiesAttributedClass noPropertiesAttributedClass)
    {
        return Ok(noPropertiesAttributedClass);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServerSideErrorResponse), StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public ActionResult NoResponseOnSuccessNoBody()
    {
        return Ok();
    }

    [ProducesResponseType(typeof(NoPropertiesAttributedClass), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServerSideErrorResponse), StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public ActionResult<NoPropertiesAttributedClass> ClassResponseOnNotFound([FromBody] NotFoundResponse notFoundResponse)
    {
        return BadRequest(notFoundResponse);
    }

    [ProducesResponseType(typeof(NoPropertiesAttributedClass), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServerSideErrorResponse), StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public ActionResult<NoPropertiesAttributedClass> ClassResponseOnServerError([FromBody] ServerSideErrorResponse serverSideErrorResponse)
    {
        return this.StatusCode(500, serverSideErrorResponse);
    }

    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServerSideErrorResponse), StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public ActionResult<string> SimpleResponseOnSuccessNoBody([FromQuery] string response)
    {
        return Ok(response);
    }

    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServerSideErrorResponse), StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public ActionResult<string> SimpleResponseOnSuccessWithBody([FromBody] NoPropertiesAttributedClass noPropertiesAttributedClass)
    {
        return Ok(noPropertiesAttributedClass.ExampleString);
    }
}
