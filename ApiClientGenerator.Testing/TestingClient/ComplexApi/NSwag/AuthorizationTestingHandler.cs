using FluentAssertions;
using NSwag.ComplexTestingApi.CSharp;
using NUnit.Framework;

namespace TestingClient.ComplexApi.NSwag;

[TestFixture]
public class AuthorizationTestingHandler
{
    private AuthorizationClient client;

    [SetUp]
    public void Setup()
    {
        var httpClient = HttpHelper.CreateHttpClient();
        var client = new AuthorizationClient(httpClient);
        client.JsonSerializerSettings.WithAllPossiblyNecessarySettings();
        this.client = client;
    }

    [Test]
    public async Task Test_AuthenticationTest()
    {
        var result = await this.client.AuthenticationTestAsync(NSwagClassHelper.AllPropertiesQueryAttributed.ExampleString, NSwagClassHelper.AllPropertiesQueryAttributed.ExampleInteger, "TestApiKey");
        result.Should().BeEquivalentTo(NSwagClassHelper.AllPropertiesQueryAttributed);
    }

}
