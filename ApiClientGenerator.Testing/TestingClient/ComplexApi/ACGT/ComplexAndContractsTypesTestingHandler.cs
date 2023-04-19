using ComplexTestingApi.CSharp;
using FluentAssertions;
using NUnit.Framework;
using TestingContracts.Models;
using TestingContracts.Responses;

namespace TestingClient.ComplexApi.ACGT;

[TestFixture]
internal class ComplexAndContractsTypesTestingHandler
{
    private ComplexAndContractsTypesClient client;

    [SetUp]
    public void Setup()
    {
        var httpClient = HttpHelper.CreateHttpClient();
        var client = new ComplexAndContractsTypesClient(httpClient);
        client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
        this.client = client;
    }

    [Test]
    public async Task Test_ComplexContractAsBodySameResponse()
    {
        var result = await this.client.ComplexContractAsBodySameResponseAsync(ACGTClassHelper.CarPool);
        result.SuccessResponse.Should().BeEquivalentTo(ACGTClassHelper.CarPool);
    }

    [Test]
    public async Task Test_ContractWithGenericResponse()
    {
        var dataResponse = new DataResponse<Car>(ACGTClassHelper.Car);
        var result = await this.client.ContractWithGenericResponseAsync(ACGTClassHelper.Car);
        result.SuccessResponse.Should().BeEquivalentTo(dataResponse);
    }

    [Test]
    public async Task Test_ContractWithAnotherGenericResponse()
    {
        var dataResponse = new DataResponse<Accessory>(ACGTClassHelper.Accessory);
        var result = await this.client.ContractWithAnotherGenericResponseAsync(ACGTClassHelper.CarPool);
        result.SuccessResponse.Should().BeEquivalentTo(dataResponse);
    }
}
