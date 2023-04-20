using FluentAssertions;
using NUnit.Framework;
using NSwag.SimpleTestingApi.CSharp;

namespace TestingClient.SimpleApi;

[TestFixture]
public class NSwagSimpleTestingHandler
{
    private SimpleTestingApiClient healthCheckClient;
    private  HealthCheckOptions healthCheckOptions;

    [SetUp]
    public void Setup()
    {
        var httpClient = HttpHelper.CreateHttpClient();
        var client = new SimpleTestingApiClient("https://localhost:7205", httpClient);
        client.JsonSerializerSettings.WithAllPossiblyNecessarySettings();
        this.healthCheckClient = client;
        this.healthCheckOptions = new HealthCheckOptions() { Interval = 1, Path = "test" };

    }

    [Test]
    public async Task Test_PostHealthCheckOptions()
    {
        var result = await this.healthCheckClient.PostHealthCheckOptionsAsync(this.healthCheckOptions);
        result.Should().BeEquivalentTo(this.healthCheckOptions);
    }

    [Test]
    public async Task Test_GetHealthCheckOptions()
    {
        var result = await this.healthCheckClient.GetHealthCheckOptionsAsync(this.healthCheckOptions.Interval, this.healthCheckOptions.Path);
        result.Should().BeEquivalentTo(this.healthCheckOptions);

    }

    [Test]
    public async Task Test_DeleteHealthCheckOptions()
    {
        var result = await this.healthCheckClient.DeleteHealthCheckOptionsAsync(this.healthCheckOptions.Interval);
        result.Interval.Should().Be(this.healthCheckOptions.Interval);
    }
}
