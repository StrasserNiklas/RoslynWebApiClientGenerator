using FluentAssertions;
using NSwag.ComplexTestingApi.CSharp;
using NUnit.Framework;
using TestingContracts.Models;
using TestingContracts.Responses;

namespace TestingClient.ComplexApi.NSwag;

[TestFixture]
internal class ComplexAndContractsTestingHandler
{
    private ComplexAndContractsTypesClient client;

    [SetUp]
    public void Setup()
    {
        var httpClient = HttpHelper.CreateHttpClient();
        var client = new ComplexAndContractsTypesClient(httpClient);
        client.JsonSerializerSettings.WithAllPossiblyNecessarySettings();
        this.client = client;
    }

    [Test]
    public async Task Test_ComplexContractAsBodySameResponse()
    {
        var result = await this.client.ComplexContractAsBodySameResponseAsync(NSwagClassHelper.CarPool);
        result.Should().BeEquivalentTo(NSwagClassHelper.CarPool);
    }

    [Test]
    public async Task Test_ContractWithGenericResponse()
    {
        var dataResponse = new DataResponse<Car>(NSwagClassHelper.Car);
        var result = await this.client.ContractWithGenericResponseAsync(NSwagClassHelper.Car);
        result.Should().BeEquivalentTo(dataResponse);
    }

    [Test]
    public async Task Test_ContractWithAnotherGenericResponse()
    {
        var dataResponse = new DataResponse<Accessory>(NSwagClassHelper.Accessory);
        var result = await this.client.ContractWithAnotherGenericResponseAsync(NSwagClassHelper.CarPool);
        result.Should().BeEquivalentTo(dataResponse);
    }
}
