using Microsoft.AspNetCore.Mvc;

namespace SimpleWebApi
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string? Summary { get; set; }
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
        public string MineString { get; set; }

        [FromHeader]
        public int Second { get; set; }
    }
}