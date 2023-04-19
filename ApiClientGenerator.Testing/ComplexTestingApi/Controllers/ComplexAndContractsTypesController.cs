using Microsoft.AspNetCore.Mvc;
using TestingContracts.Models;
using TestingContracts.Responses;

namespace ComplexTestingApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class ComplexAndContractsTypesController : ControllerBase
{
    [HttpPost]
    public ActionResult<CarPool> ComplexContractAsBodySameResponse([FromBody] CarPool carPool)
    {
        return Ok(carPool);
    }

    [HttpPost]
    public ActionResult<DataResponse<Car>> ContractWithGenericResponse([FromBody] Car car)
    {
        return Ok(new DataResponse<Car>(car));
    }

    [HttpPost]
    public ActionResult<DataResponse<Accessory>> ContractWithAnotherGenericResponse([FromBody] CarPool carPool)
    {
        return Ok(new DataResponse<Accessory>(carPool.Cars.First().Accessories.First()));
    }
}