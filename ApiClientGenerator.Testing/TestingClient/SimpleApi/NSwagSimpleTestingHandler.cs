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
        var client = new SimpleTestingApiClient("https://localhost:7205", new HttpClient() { BaseAddress = new Uri("https://localhost:7205") });
        //client.
        //client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
        this.healthCheckClient = client;
        this.healthCheckOptions = new HealthCheckOptions() { Interval = 1, Path = "test" };

    }

    [Test]
    public async Task Test_PostHealthCheckOptions()
    {
        var result = await this.healthCheckClient.PostHealthCheckOptionsAsync(this.healthCheckOptions);
        healthCheckOptions.Should().BeEquivalentTo(result);
    }

    [Test]
    public async Task Test_GetHealthCheckOptions()
    {
        var result = await this.healthCheckClient.GetHealthCheckOptionsAsync(this.healthCheckOptions.Interval, this.healthCheckOptions.Path);
        this.healthCheckOptions.Should().BeEquivalentTo(result);

    }

    [Test]
    public async Task Test_DeleteHealthCheckOptions()
    {
        var result = await this.healthCheckClient.DeleteHealthCheckOptionsAsync(this.healthCheckOptions.Interval);
        this.healthCheckOptions.Interval.Should().Be(result.Interval);
    }
}
