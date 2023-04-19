using ComplexTestingApi.CSharp;
using FluentAssertions;
using NUnit.Framework;

namespace TestingClient.ComplexApi.ACGT;

[TestFixture]
internal class FromBodyTestingHandler
{
    private FromBodyClient client;

    [SetUp]
    public void Setup()
    {
        var httpClient = HttpHelper.CreateHttpClient();
        var client = new FromBodyClient(httpClient);
        client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
        this.client = client;
    }

    [Test]
    public async Task Test_SimpleResponseOnSuccessNoBody()
    {
        var result = await this.client.ClassParameterNoAttributeAsync(ACGTClassHelper.NoPropertiesAttributed);
        result.SuccessResponse.Should().BeEquivalentTo(ACGTClassHelper.NoPropertiesAttributed);
    }

    [Test]
    public async Task Test_ClassParameterWithAttribute()
    {
        var result = await this.client.ClassParameterWithAttributeAsync(ACGTClassHelper.NoPropertiesAttributed);
        result.SuccessResponse.Should().BeEquivalentTo(ACGTClassHelper.NoPropertiesAttributed);
    }

    [Test]
    public async Task Test_PrimitiveParameterWithAttribute()
    {
        var result = await this.client.PrimitiveParameterWithAttributeAsync(ACGTClassHelper.NoPropertiesAttributed.ExampleString);
        result.SuccessResponse.Should().Be(ACGTClassHelper.NoPropertiesAttributed.ExampleString);
    }
}
