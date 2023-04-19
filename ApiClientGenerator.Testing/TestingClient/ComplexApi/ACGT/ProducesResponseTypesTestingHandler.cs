using ComplexTestingApi.CSharp;
using FluentAssertions;
using NUnit.Framework;

namespace TestingClient.ComplexApi.ACGT;

[TestFixture]
internal class ProducesResponseTypesTestingHandler
{
    private ProducesResponseTypeClient client;

    [SetUp]
    public void Setup()
    {
        var httpClient = HttpHelper.CreateHttpClient();
        var client = new ProducesResponseTypeClient(httpClient);
        client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
        this.client = client;

    }

    [Test]
    public async Task Test_ClassResponseOnSuccessWithBody()
    {
        var result = await this.client.ClassResponseOnSuccessWithBodyAsync(ACGTClassHelper.NoPropertiesAttributed);
        result.SuccessResponse.Should().BeEquivalentTo(ACGTClassHelper.NoPropertiesAttributed);
    }

    [Test]
    public async Task Test_NoResponseOnSuccessNoBody()
    {
        var result = await this.client.NoResponseOnSuccessNoBodyAsync();
        result.StatusCode.Should().Be(200);
    }

    [Test]
    public async Task Test_ClassResponseOnNotFound()
    {
        var result = await this.client.ClassResponseOnNotFoundAsync(ACGTClassHelper.NotFoundResponse);
        var errorResponse = result.ErrorResponse as NotFoundResponse;
        errorResponse.Should().BeEquivalentTo(ACGTClassHelper.NotFoundResponse);
    }

    [Test]
    public async Task Test_ClassResponseOnServerError()
    {
        var result = await this.client.ClassResponseOnServerErrorAsync(ACGTClassHelper.ServerSideErrorResponse);
        var errorResponse = result.ErrorResponse as ServerSideErrorResponse;
        errorResponse.Should().BeEquivalentTo(ACGTClassHelper.ServerSideErrorResponse);
    }

    [Test]
    public async Task Test_SimpleResponseOnSuccessNoBody()
    {
        var result = await this.client.SimpleResponseOnSuccessNoBodyAsync(ACGTClassHelper.NoPropertiesAttributed.ExampleString);
        result.SuccessResponse.Should().Be(ACGTClassHelper.NoPropertiesAttributed.ExampleString);
    }

    [Test]
    public async Task Test_SimpleResponseOnSuccessWithBody()
    {
        var result = await this.client.SimpleResponseOnSuccessWithBodyAsync(ACGTClassHelper.NoPropertiesAttributed);
        result.SuccessResponse.Should().Be(ACGTClassHelper.NoPropertiesAttributed.ExampleString);
    }

}
