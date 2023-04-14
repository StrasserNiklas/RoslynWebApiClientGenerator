using Microsoft.AspNetCore.Mvc;

namespace SimpleTestingApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class HealthCheckController : ControllerBase
{
    [HttpPost]
    public ActionResult<HealthCheckOptions> PostHealthCheckOptions([FromBody] HealthCheckOptions healthCheckOptions)
    {
        return Ok(healthCheckOptions);
    }

    [HttpGet]
    public ActionResult<HealthCheckOptions> GetHealthCheckOptions([FromHeader] int interval, [FromQuery] string path)
    {
        return Ok(new HealthCheckOptions() { Path = path, Interval = interval });
    }

    [HttpDelete]
    [Route("delete/{interval}")]
    public ActionResult<HealthCheckOptions> DeleteHealthCheckOptions([FromRoute] int interval)
    {
        return Ok(new HealthCheckOptions() { Interval = interval });
    }
}

public class HealthCheckOptions
{
    public string Path { get; set; }
    public int Interval { get; set; }
}

