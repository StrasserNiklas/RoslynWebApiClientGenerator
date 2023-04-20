using FluentAssertions;
using NSwag.ComplexTestingApi.CSharp;
using NUnit.Framework;

namespace TestingClient.ComplexApi.NSwag;

[TestFixture]
internal class FromFormTestingHandler
{
    private FromFormClient client;

    [SetUp]
    public void Setup()
    {
        var httpClient = HttpHelper.CreateHttpClient();
        var client = new FromFormClient(httpClient);
        client.JsonSerializerSettings.WithAllPossiblyNecessarySettings();
        this.client = client;
    }

    [Test]
    public async Task Test_ClassParameterWithAttribute()
    {
        var result = await this.client.ClassParameterWithAttributeAsync(NSwagClassHelper.NoPropertiesAttributed.ExampleString, NSwagClassHelper.NoPropertiesAttributed.ExampleInteger);
        result.Should().BeEquivalentTo(NSwagClassHelper.NoPropertiesAttributed);
    }

    [Test]
    public async Task Test_SimpleParameterWithAttribute()
    {
        var result = await this.client.SimpleParameterWithAttributeAsync(NSwagClassHelper.NoPropertiesAttributed.ExampleString);
        result.Should().Be(NSwagClassHelper.NoPropertiesAttributed.ExampleString);
    }
}
