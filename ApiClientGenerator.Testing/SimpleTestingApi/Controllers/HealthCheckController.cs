using Microsoft.AspNetCore.Mvc;

namespace SimpleTestingApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class HealthCheckController : ControllerBase
{
    [HttpPost]
    public ActionResult<HealthCheckOptions> PostHealthCheckOptions([FromBody] HealthCheckOptions healthCheckOptions, [FromHeader] string test)
    {
        return Ok(healthCheckOptions);
    }

    [HttpGet]
    public ActionResult<HealthCheckOptions> GetHealthCheckOptions([FromHeader] int interval, [FromQuery] string path)
    {
        return Ok(new HealthCheckOptions() { Path = path, Interval = interval });
    }

    [HttpDelete]
    [Route("delete/{id}")]
    public ActionResult<HealthCheckOptions> DeleteHealthCheckOptions([FromRoute] int id)
    {
        return Ok(new HealthCheckOptions() { Interval = id });
    }
}

public class HealthCheckOptions
{
    public string Path { get; set; }
    public int Interval { get; set; }
}