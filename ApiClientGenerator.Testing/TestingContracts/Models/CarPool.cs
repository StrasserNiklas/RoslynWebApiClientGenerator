using System.ComponentModel.DataAnnotations;

namespace TestingContracts.Models;

public class CarPool
{
    public int ZipCode { get; set; }

    public string? LocationName { get; set; }

    [Required]
    public IEnumerable<Car> Cars { get; set; }
}