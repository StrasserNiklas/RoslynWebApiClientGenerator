using ClassLibrary;
using Microsoft.AspNetCore.Mvc;

namespace ApiTest.Controllers
{

    [ApiController]
    [Route("[controller]")]
    //[Route("hmm")]
    public class ApiControllerWithRoute// : ControllerBase
    {
        // lool oder wenn bei lol route(some)
        // contRoute
        [HttpGet]
        public ActionResult<IEnumerable<Response>> GetThisLol()
        {
            return new ActionResult<IEnumerable<Response>>(new List<Response>());
            //return Ok(new Response());
        }

        [HttpGet]
        public ActionResult<IDictionary<string, Response>> GetThisDict()
        {
            return new ActionResult<IDictionary<string, Response>>(new Dictionary<string, Response>());
            //return Ok(new Response());
        }

        [HttpGet]
        public ActionResult<Request> GetThisLol2()
        {
            return new ActionResult<Request>(new Request());
            //return Ok(new Response());
        }



    }

    //[Route("[controller]")]
    //public class ControllerWithNoRouteButWithMethodRoutes : Controller
    //{
    //    //[Route("ok")]
    //    //[HttpGet]
    //    //public ActionResult<Response> Get([FromBody] Request request)
    //    //{
    //    //    return new ActionResult<Response>(new Response());
    //    //    //return Ok(new Response());
    //    //}

    //    [Route("ok/{id}")]
    //    [HttpGet]
    //    public static Task<ActionResult<IEnumerable<BasicClassToUse>>> Getit([FromServices] ITestService testservice, int id, [FromQuery] Request request)
    //    {
    //        return Task.FromResult(new ActionResult<IEnumerable<BasicClassToUse>>(new List<BasicClassToUse>()));
    //        //return Ok();
    //    }

    //    //[Route("TestNoHttp")]
    //    //public static ActionResult<Response> NoHttp()
    //    //{
    //    //    return new ActionResult<Response>(new Response());
    //    //    //return Ok(new Response());
    //    //}
    //}



   
    



    //[Route("[controller]")]
    //[Route("kak")]
    //public class ControllerBaseWithRoute : ControllerBase
    //{
    //    [Route("ok")]
    //    [HttpGet]
    //    public ActionResult<Response> Get()
    //    {
    //        return new ActionResult<Response>(new Response());
    //        //return Ok(new Response());
    //    }
    //}


    //public class InheritControllerBaseWithRoute : ControllerBaseWithRoute
    //{
    //    // /ok
    //    [Route("ok")]
    //    [HttpGet]
    //    public ActionResult<Response> Get()
    //    {
    //        return new ActionResult<Response>(new Response());
    //        //return Ok(new Response());
    //    }
    //}

    //[Route("some")]
    //public class InheritFromApiControllerWithRoute : ApiControllerWithRoute
    //{
    //    // some/ok
    //    //[Route("ok")]
    //    [HttpGet]
    //    public new ActionResult<Response> GetMe()
    //    {
    //        return new ActionResult<Response>(new Response());
    //        //return Ok(new Response());
            
    //    }
    //}


    //[ApiController]
    //[Route("api/betslip/[action]")]
    //public class NormalControllerActionRoute : ControllerBase
    //{
    //    [HttpGet]
    //    public ActionResult<Response> Get()
    //    {
    //        return Ok(new Response());
    //    }
    //}

    //[ApiController]
    //[Route("api/[controller]/[action]")]
    //public class ControllerActionRoute : Controller
    //{
    //    [HttpGet]
    //    public ActionResult<Response> Get([FromBody] Request request)
    //    {
    //        return Ok(new Response());
    //    }

    //    [HttpPost]
    //    private ActionResult<Response> Post([FromBody] Request request)
    //    {
    //        return Ok(new Response());
    //    }
    //}

    public class Response
    {
        //public int Id { get; set; }

        //public ResponseClass WellWellWell { get; set; }

        public ICollection<Request> Parameters { get; set; } = new System.Collections.ObjectModel.Collection<Request>();

        public string ReturnText { get; set; }
        public string? NullableReturnText { get; set; }
    }

    public class Request
    {
        public ResponseEnum ResponseEnumResponse { get; set; }
    }

    public class ResponseClass
    {
        public int IndoorInteger { get; set; }
    }

    public enum ResponseEnum
    {
        IsEnum,
        IsNotEnum
    }
}