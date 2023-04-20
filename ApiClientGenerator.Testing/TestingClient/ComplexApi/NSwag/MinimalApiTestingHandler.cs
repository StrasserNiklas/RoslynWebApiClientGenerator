using NSwag.ComplexTestingApi.CSharp;
using NUnit.Framework;

namespace TestingClient.ComplexApi.NSwag;

//[TestFixture]
internal class MinimalApiTestingHandler
{
    //private IHealthCheckClient client;
    //private HealthCheckOptions healthCheckOptions;

    //[SetUp]
    public void Setup()
    {
        var httpClient = HttpHelper.CreateHttpClient();
        var client = new CombinedAttributesClient(httpClient);
        client.JsonSerializerSettings.WithAllPossiblyNecessarySettings();
        //this.client = client;
    }

    //[Test]
    public async Task Test_PostHealthCheckOptions()
    {
        //var result = await this.healthCheckClient.PostHealthCheckOptionsAsync(this.healthCheckOptions);
        //healthCheckOptions.Should().BeEquivalentTo(result.SuccessResponse);
    }
}
