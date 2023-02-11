using TestingContracts.Enums;

namespace TestingContracts.Models;

public class Car
{
    public string CarIdentifier { get; set; }
    public EngineType EngineType { get; set; }
    public string? CarImgBase64 { get; set; }
    public IEnumerable<Accessory> Accessories { get; set; }
    public IDictionary<string, string> Properties { get; set; }
}
