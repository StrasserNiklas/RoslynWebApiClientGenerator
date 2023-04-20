using FluentAssertions;
using NSwag.ComplexTestingApi.CSharp;
using NUnit.Framework;

namespace TestingClient.ComplexApi.NSwag;

[TestFixture]
internal class FromRouteTestingHandler
{
    private FromRouteClient client;

    [SetUp]
    public void Setup()
    {
        var httpClient = HttpHelper.CreateHttpClient();
        var client = new FromRouteClient(httpClient);
        client.JsonSerializerSettings.WithAllPossiblyNecessarySettings();
        this.client = client;
    }

    [Test]
    public async Task Test_ClassParameterWithAttributeNoPropertiesAttributed()
    {
        var result = await this.client.ClassParameterWithAttributeNoPropertiesAttributedAsync(NSwagClassHelper.NoPropertiesAttributed.ExampleString, NSwagClassHelper.NoPropertiesAttributed.ExampleInteger);
        result.Should().BeEquivalentTo(NSwagClassHelper.NoPropertiesAttributed);
    }

    [Test]
    public async Task Test_PrimitiveParameterWithAttribute()
    {
        var result = await this.client.PrimitiveParameterWithAttributeAsync(NSwagClassHelper.NoPropertiesAttributed.ExampleString);
        result.Should().Be(NSwagClassHelper.NoPropertiesAttributed.ExampleString);
    }

    [Test]
    public async Task Test_PrimitiveParameterNoAttribute()
    {
        var result = await this.client.PrimitiveParameterNoAttributeAsync(NSwagClassHelper.NoPropertiesAttributed.ExampleInteger);
        result.Should().Be(NSwagClassHelper.NoPropertiesAttributed.ExampleInteger);
    }
}
