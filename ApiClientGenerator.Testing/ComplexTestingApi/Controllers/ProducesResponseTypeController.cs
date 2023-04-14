﻿using ComplexTestingApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace ComplexTestingApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class ProducesResponseTypeController : ControllerBase
{
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServerSideErrorResponse), StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public ActionResult NoResponseOnSuccessNoBody()
    {
        return Ok();
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServerSideErrorResponse), StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public ActionResult NoResponseOnSuccessWithBody([FromBody] NoPropertiesAttributedClass noPropertiesAttributedClass, [FromQuery] bool flag = true)
    {
        return Ok();
    }

    [ProducesResponseType(typeof(NoPropertiesAttributedClass), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServerSideErrorResponse), StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public ActionResult<NoPropertiesAttributedClass> ClassResponseOnSuccessNoBody()
    {
        return Ok();
    }

    [ProducesResponseType(typeof(NoPropertiesAttributedClass), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServerSideErrorResponse), StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public ActionResult<NoPropertiesAttributedClass> ClassResponseOnSuccessWithBody([FromBody] NoPropertiesAttributedClass noPropertiesAttributedClass)
    {
        return Ok();
    }

    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServerSideErrorResponse), StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public ActionResult<string> SimpleResponseOnSuccessNoBody()
    {
        return Ok();
    }

    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServerSideErrorResponse), StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public ActionResult<string> SimpleResponseOnSuccessWithBody([FromBody] NoPropertiesAttributedClass noPropertiesAttributedClass)
    {
        return Ok();
    }
}
