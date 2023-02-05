using Microsoft.AspNetCore.Mvc;

namespace SimpleWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }


        [Route("get")]
        [HttpGet]
        public ActionResult Get(Tool some)
        {
            return Ok(new { A = some.MineString, B = some.Second });
        }

        [Route("get2")]
        [HttpGet]
        public ActionResult Get2(Tool2 some)
        {
            return Ok(new { A = some.MineString, B = some.Second});
        }

        [Route("get3")]
        [HttpGet]
        public ActionResult Get3(Tool3 some)
        {
            return Ok(new { A = some.MineString, B = some.Second });
        }

        [Route("getmore")]
        [HttpGet]
        public IEnumerable<WeatherForecast> GetMore([FromHeader] string some)
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        

        
    }

    public class DataResponse<TGeneric>
    {
        public TGeneric Data { get; set; }

        public DataResponse(TGeneric data)
        {
            this.Data = data;
        }
    }

    public class MetaResponse<TGeneric, SecondOne>
    {
        public TGeneric Data { get; set; }
        public SecondOne kek { get; set; }

        public MetaResponse(TGeneric data, SecondOne kek)
        {
            this.Data = data;
            this.kek = kek;
        }

        public string MineStreing { get; set; }
    }

    public class Response
    {
        public IEnumerable<Tool> Tools { get; set; }
        public IDictionary<string, Tool> DicTools { get; set; }
        public List<IDictionary<string, Tool>> ToolList { get; set; }
    }

    public class Tool
    {
        [FromHeader]
        //[FromQuery]

        public string MineString { get; set; }

        [FromHeader]
        //[FromQuery]
        public int Second { get; set; }
        //[FromQuery]
    }

    public class Tool2
    {
        [FromHeader]
        public string MineString { get; set; }

        public int Second { get; set; }
    }

    public class Tool3
    {
        public string MineString { get; set; }

        public int Second { get; set; }
    }

    public class UserDto
    {
        [FromRoute]
        public int Id { get; set; }
        [FromBody]
        public UserBodyDto Body { get; set; }
    }

    public class UserBodyDto
    {
        public string Name { get; set; }
        public string FavoriteDish { get; set; }
    }
}