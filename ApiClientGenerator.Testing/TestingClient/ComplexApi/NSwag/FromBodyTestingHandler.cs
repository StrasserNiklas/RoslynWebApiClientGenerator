using FluentAssertions;
using NSwag.ComplexTestingApi.CSharp;
using NUnit.Framework;

namespace TestingClient.ComplexApi.NSwag;

[TestFixture]
internal class FromBodyTestingHandler
{
    private FromBodyClient client;

    [SetUp]
    public void Setup()
    {
        var httpClient = HttpHelper.CreateHttpClient();
        var client = new FromBodyClient(httpClient);
        client.JsonSerializerSettings.WithAllPossiblyNecessarySettings();
        this.client = client;
    }

    [Test]
    public async Task Test_SimpleResponseOnSuccessNoBody()
    {
        var result = await this.client.ClassParameterNoAttributeAsync(NSwagClassHelper.NoPropertiesAttributed);
        result.Should().BeEquivalentTo(NSwagClassHelper.NoPropertiesAttributed);
    }

    [Test]
    public async Task Test_ClassParameterWithAttribute()
    {
        var result = await this.client.ClassParameterWithAttributeAsync(NSwagClassHelper.NoPropertiesAttributed);
        result.Should().BeEquivalentTo(NSwagClassHelper.NoPropertiesAttributed);
    }

    [Test]
    public async Task Test_PrimitiveParameterWithAttribute()
    {
        var result = await this.client.PrimitiveParameterWithAttributeAsync(NSwagClassHelper.NoPropertiesAttributed.ExampleString);
        result.Should().Be(NSwagClassHelper.NoPropertiesAttributed.ExampleString);
    }
}
