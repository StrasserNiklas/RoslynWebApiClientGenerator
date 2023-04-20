using ComplexTestingApi.CSharp;
using FluentAssertions;
using NUnit.Framework;

namespace TestingClient.ComplexApi.ACGT;

[TestFixture]
public class AuthorizationTestingHandler
{
    private AuthorizationClient client;

    [SetUp]
    public void Setup()
    {
        var httpClient = HttpHelper.CreateHttpClient();
        var client = new AuthorizationClient(httpClient);
        client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
        this.client = client;
    }

    [Test]
    public async Task Test_AuthenticationTest()
    {
        client.AddClientWideHeader("Api-Key", "TestApiKey");
        var result = await this.client.AuthenticationTestAsync(ACGTClassHelper.AllPropertiesQueryAttributed);
        result.SuccessResponse.Should().BeEquivalentTo(ACGTClassHelper.AllPropertiesQueryAttributed);
    }
}
