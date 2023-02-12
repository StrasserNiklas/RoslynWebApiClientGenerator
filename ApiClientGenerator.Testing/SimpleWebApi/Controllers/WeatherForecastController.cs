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


        [Route("getmore/{id}")]
        [HttpGet]
        public IEnumerable<WeatherForecast> GetMore([FromForm] NoAttributes noAttributes, [FromBody] UserBodyDto userBodyDto)
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [Route("{id}/getmore")]
        [HttpGet]
        public IEnumerable<WeatherForecast> GetMore2(int id, [FromHeader] string some, [FromQuery] NoAttributes noAttributes, [FromBody] UserBodyDto userBodyDto)
        {
            var x = "";

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [Route("get1")]
        [HttpGet]
        public ActionResult Get1(AllAttributes some)
        {
            return Ok(new { A = some.MineString, B = some.Second });
        }

        [Route("get2")]
        [HttpGet]
        public ActionResult Get2(SomeAttributes some)
        {
            return Ok(new { A = some.MineString, B = some.Second });
        }

        [Route("get3")]
        [HttpGet]
        public ActionResult Get3(NoAttributes some)
        {
            return Ok(new { A = some.MineString, B = some.Second });
        }

        [Route("get10")]
        [HttpGet]
        public ActionResult Get10([FromQuery] AllAttributes some)
        {
            return Ok(new { A = some.MineString, B = some.Second });
        }

        [Route("get20")]
        [HttpGet]
        public ActionResult Get20([FromQuery] SomeAttributes some)
        {
            return Ok(new { A = some.MineString, B = some.Second });
        }

        [Route("get30")]
        [HttpGet]
        public ActionResult Get30([FromQuery] NoAttributes some)
        {
            return Ok(new { A = some.MineString, B = some.Second });
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
        public IEnumerable<AllAttributes> Tools { get; set; }
        public IDictionary<string, AllAttributes> DicTools { get; set; }
        public List<IDictionary<string, AllAttributes>> ToolList { get; set; }
    }

    public class AllAttributes
    {
        [FromQuery]

        public string MineString { get; set; }

        [FromQuery]
        public int Second { get; set; }
    }

    public class SomeAttributes
    {
        public string MineString { get; set; }

        [FromQuery]

        public int Second { get; set; }
    }

    public class NoAttributes
    {
        public object kok { get; set; }


        public int Second { get; set; }

        public string MineString { get; set; }

        
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