using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TestingPlayground.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FromBodyController : ControllerBase
    {
    }

    [Route("api/[controller]")]
    [ApiController]
    public class FromQueryController : ControllerBase
    {
    }

    [Route("api/[controller]")]
    [ApiController]
    public class FromHeaderController : ControllerBase
    {
    }

    [Route("api/[controller]")]
    [ApiController]
    public class FromRouteController : ControllerBase
    {
    }
}
