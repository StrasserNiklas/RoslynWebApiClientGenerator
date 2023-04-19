using ComplexTestingApi.CSharp;
using FluentAssertions;
using NUnit.Framework;

namespace TestingClient.ComplexApi.ACGT;

[TestFixture]
internal class FromRouteTestingHandler
{
    private FromRouteClient client;

    [SetUp]
    public void Setup()
    {
        var httpClient = HttpHelper.CreateHttpClient();
        var client = new FromRouteClient(httpClient);
        client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
        this.client = client;

    }

    [Test]
    public async Task Test_ClassParameterWithAttributeNoPropertiesAttributed()
    {
        var result = await this.client.ClassParameterWithAttributeNoPropertiesAttributedAsync(ACGTClassHelper.NoPropertiesAttributed);
        result.SuccessResponse.Should().BeEquivalentTo(ACGTClassHelper.NoPropertiesAttributed);
    }

    [Test]
    public async Task Test_PrimitiveParameterWithAttribute()
    {
        var result = await this.client.PrimitiveParameterWithAttributeAsync(ACGTClassHelper.NoPropertiesAttributed.ExampleString);
        result.SuccessResponse.Should().Be(ACGTClassHelper.NoPropertiesAttributed.ExampleString);
    }

    [Test]
    public async Task Test_PrimitiveParameterNoAttribute()
    {
        var result = await this.client.PrimitiveParameterNoAttributeAsync(ACGTClassHelper.NoPropertiesAttributed.ExampleInteger);
        result.SuccessResponse.Should().Be(ACGTClassHelper.NoPropertiesAttributed.ExampleInteger);
    }
}
