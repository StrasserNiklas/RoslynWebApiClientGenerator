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
        public ActionResult Get(string some)
        {
            return Ok( new MetaResponse<WeatherForecast, object>(new WeatherForecast()
            {
                Date = DateTime.Now,
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }, ""));
        }

        [Route("getmore")]
        [HttpGet]
        public IEnumerable<WeatherForecast> GetMore(Tool tool, Guid clientId,
            DateTime? from,
            DateTime? to,
            int offset = 0,
            int limit = 100)
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
        //[FromHeader]
        //[FromQuery]

        public string MineString { get; set; }

        //[FromQuery]
        public int Second { get; set; }
        //[FromQuery]

        //[FromBody]

        public int Third { get; set; }
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