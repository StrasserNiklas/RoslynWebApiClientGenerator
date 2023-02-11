﻿using Microsoft.AspNetCore.Mvc;
using TestingPlayground.Models;

namespace TestingPlayground.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FromQueryController : ControllerBase
{
    [HttpGet]
    public ActionResult Class_Parameter_WithoutAttribute_AllPropertiesAttributed(AllPropertiesQueryAttributed allPropertiesAttributed)
    {
        return Ok();
    }
    // check if you need builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressInferBindingSourcesForParameters = true);
    [HttpGet]
    public ActionResult ClassParameterWithoutAttributeSomePropertiesAttributed(SomePropertiesQueryAttributed somePropertiesAttributed)
    {
        return Ok();
    }
    [HttpGet]
    public ActionResult ClassParameterWithAttributeSomePropertiesAttributed([FromQuery] SomePropertiesQueryAttributed somePropertiesAttributed)
    {
        return Ok();
    }
    [HttpGet]
    public ActionResult ClassParameterWithAttributeAllPropertiesAttributed([FromQuery] AllPropertiesQueryAttributed somePropertiesAttributed)
    {
        return Ok();
    }
    [HttpGet]
    public ActionResult ClassParameterWithAttribute([FromQuery] NoPropertiesAttributedClass simpleQueryClass)
    {
        return Ok();
    }
    [HttpGet]
    public ActionResult PrimitiveParameterWithAttribute([FromQuery] string soloString)
    {
        return Ok();
    }
    [HttpGet]
    public ActionResult PrimitiveParameterNoAttribute(string soloString)
    {
        return Ok();
    }
    [HttpGet]
    public ActionResult ClassParameterNoAttribute(NoPropertiesAttributedClass soloString)
    {
        return Ok();
    }
}

public class AllPropertiesQueryAttributed
{
    [FromQuery]
    public string ExampleString { get; set; }
    public int ExampleInteger { get; set; }
}

public class SomePropertiesQueryAttributed
{
    [FromQuery]
    public string ExampleString { get; set; }
    public int ExampleInteger { get; set; }
}