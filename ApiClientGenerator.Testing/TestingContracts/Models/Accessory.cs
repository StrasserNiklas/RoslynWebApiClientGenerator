using System.ComponentModel.DataAnnotations;
using TestingContracts.Enums;

namespace TestingContracts.Models;

public class Accessory
{
    [Required]
    public Guid AccessoryGuid { get; set; }

    [Required]
    public AccessoryType AccessoryType { get; set; }

    [Required]
    public IEnumerable<AccessoryFeature> AccessoryFeatures { get; set; }
}
