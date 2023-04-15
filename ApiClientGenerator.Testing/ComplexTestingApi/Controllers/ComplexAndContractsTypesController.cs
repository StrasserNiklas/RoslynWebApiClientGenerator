using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TestingContracts.Models;
using TestingContracts.Responses;

namespace ComplexTestingApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class ComplexAndContractsTypesController : ControllerBase
{
    [HttpPost]
    public ActionResult<CarPool> PrimitiveParameterWithAttribute([FromBody] CarPool carPool)
    {
        return Ok(carPool);
    }

    [HttpPost]
    public ActionResult<DataResponse<Car>> ContractWithGenericResponse([FromBody] CarPool carPool)
    {
        return Ok(carPool.Cars.First());
    }

    [HttpPost]
    public ActionResult<DataResponse<Accessory>> ContractWithAnotherGenericResponse([FromBody] CarPool carPool)
    {
        return Ok(carPool.Cars.First().Accessories.First());
    }
}

// DataResponse: T Data
// CarPool:   int ZipCode string? LocationName  IEnumerable<Car> Cars 
// AccessoryFeature: string Description
// Car: IEnumerable<Accessory>? Accessories IDictionary<string, string>? Properties string? CarImgBase64 EngineType EngineType string? CarIdentifier
// Accessory: IEnumerable<AccessoryFeature> AccessoryFeatures AccessoryType AccessoryType Guid AccessoryGuid
// AccesstoryType
// EngineType