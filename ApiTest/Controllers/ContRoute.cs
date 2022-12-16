using Microsoft.AspNetCore.Mvc;

namespace ApiTest.Controllers
{

    [ApiController]
    [Route("[controller]")]
    //[Route("hmm")]
    public class ContRoute// : ControllerBase
    {
        // lool oder wenn bei lol route(some)
        // contRoute
        [HttpGet]
        public ActionResult<Response> Get()
        {
            return new ActionResult<Response>(new Response());
            //return Ok(new Response());
        }
    }

    //[Route("[controller]")]
    public class SoloControlNoRoute : Controller
    {
        [Route("ok")]
        [HttpGet]
        public ActionResult<Response> Get()
        {
            return new ActionResult<Response>(new Response());
            //return Ok(new Response());
        }
    }

    [Route("[controller]")]
    public class ContRouteo : ControllerBase
    {
        [Route("ok")]
        [HttpGet]
        public ActionResult<Response> Get()
        {
            return new ActionResult<Response>(new Response());
            //return Ok(new Response());
        }
    }


    public class ContRouteoo : ContRouteo
    {
        // /ok
        [Route("ok")]
        [HttpGet]
        public ActionResult<Response> Get()
        {
            return new ActionResult<Response>(new Response());
            //return Ok(new Response());
        }
    }

    //[Route("some")]
    public class Lool : ContRoute
    {
        // some/ok
        //[Route("ok")]
        [HttpGet]
        public new ActionResult<Response> GetMe()
        {
            return new ActionResult<Response>(new Response());
            //return Ok(new Response());
            
        }
    }
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

    }

    public class Request
    {

    }
}