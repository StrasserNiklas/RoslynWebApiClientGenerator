
using FluentAssertions;
using NUnit.Framework;
using SimpleTestingApi.CSharp;

namespace TestingClient.SimpleApi;

[TestFixture]
public class ACGTSimpleTestingHandler
{
    private IHealthCheckClient healthCheckClient;
    private HealthCheckOptions healthCheckOptions;

    [SetUp]
    public void Setup()
    {
        var httpClient = HttpHelper.CreateHttpClient();
        var client = new HealthCheckClient(httpClient);
        client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
        this.healthCheckClient = client;
        this.healthCheckOptions = new HealthCheckOptions() { Interval = 1, Path = "test" };
    }

    [Test]
    public async Task Test_PostHealthCheckOptions()
    {
        var result = await this.healthCheckClient.PostHealthCheckOptionsAsync(this.healthCheckOptions);
        result.SuccessResponse.Should().BeEquivalentTo(this.healthCheckOptions);
    }

    [Test]
    public async Task Test_GetHealthCheckOptions()
    {
        var result = await this.healthCheckClient.GetHealthCheckOptionsAsync(this.healthCheckOptions.Interval, this.healthCheckOptions.Path);
        result.SuccessResponse.Should().BeEquivalentTo(this.healthCheckOptions);

    }

    [Test]
    public async Task Test_DeleteHealthCheckOptions()
    {
        var result = await this.healthCheckClient.DeleteHealthCheckOptionsAsync(this.healthCheckOptions.Interval);
        result.SuccessResponse.Interval.Should().Be(this.healthCheckOptions.Interval);
    }
}
