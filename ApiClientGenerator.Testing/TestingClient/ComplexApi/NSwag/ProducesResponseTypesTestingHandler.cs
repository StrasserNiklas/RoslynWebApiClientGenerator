using FluentAssertions;
using NSwag.ComplexTestingApi.CSharp;
using NUnit.Framework;

namespace TestingClient.ComplexApi.NSwag;

[TestFixture]
internal class ProducesResponseTypesTestingHandler
{
    private ProducesResponseTypeClient client;

    [SetUp]
    public void Setup()
    {
        var httpClient = HttpHelper.CreateHttpClient();
        var client = new ProducesResponseTypeClient(httpClient);
        client.JsonSerializerSettings.WithAllPossiblyNecessarySettings();
        this.client = client;
    }

    [Test]
    public async Task Test_ClassResponseOnSuccessWithBody()
    {
        var result = await this.client.ClassResponseOnSuccessWithBodyAsync(NSwagClassHelper.NoPropertiesAttributed);
        result.Should().BeEquivalentTo(NSwagClassHelper.NoPropertiesAttributed);
    }

    [Test]
    public async Task Test_NoResponseOnSuccessNoBody()
    {
        await this.client.NoResponseOnSuccessNoBodyAsync();
    }

    [Test]
    public async Task Test_ClassResponseOnNotFound()
    {
        try
        {
            var result = await this.client.ClassResponseOnNotFoundAsync(NSwagClassHelper.NotFoundResponse);
        }
        catch (ApiException<NotFoundResponse> errorResponse)
        {
            Assert.IsTrue(errorResponse is not null);
            return;
        }

        Assert.IsTrue(false);
    }

    [Test]
    public async Task Test_ClassResponseOnServerError()
    {
        try
        {
            var result = await this.client.ClassResponseOnServerErrorAsync(NSwagClassHelper.ServerSideErrorResponse);
        }
        catch (ApiException<ServerSideErrorResponse> errorResponse)
        {
            Assert.IsTrue(errorResponse is not null);
            return;
        }
        Assert.IsTrue(false);
    }

    [Test]
    public async Task Test_SimpleResponseOnSuccessNoBody()
    {
        var result = await this.client.SimpleResponseOnSuccessNoBodyAsync(NSwagClassHelper.NoPropertiesAttributed.ExampleString);
        result.Should().Be(NSwagClassHelper.NoPropertiesAttributed.ExampleString);
    }

    [Test]
    public async Task Test_SimpleResponseOnSuccessWithBody()
    {
        var result = await this.client.SimpleResponseOnSuccessWithBodyAsync(NSwagClassHelper.NoPropertiesAttributed);
        result.Should().Be(NSwagClassHelper.NoPropertiesAttributed.ExampleString);
    }
}
