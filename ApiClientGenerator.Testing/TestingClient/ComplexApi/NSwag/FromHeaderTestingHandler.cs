using FluentAssertions;
using NSwag.ComplexTestingApi.CSharp;
using NUnit.Framework;

namespace TestingClient.ComplexApi.NSwag;

[TestFixture]
internal class FromHeaderTestingHandler
{
    private FromHeaderClient client;

    [SetUp]
    public void Setup()
    {
        var httpClient = HttpHelper.CreateHttpClient();
        var client = new FromHeaderClient(httpClient);
        client.JsonSerializerSettings.WithAllPossiblyNecessarySettings();
        this.client = client;
    }

    [Test]
    public async Task Test_ClassParameterWithAttributeAllPropertiesAttributed()
    {
        var result = await this.client.ClassParameterWithAttributeAllPropertiesAttributedAsync(NSwagClassHelper.AllPropertiesHeaderAttributed);
        result.Should().BeEquivalentTo(NSwagClassHelper.AllPropertiesHeaderAttributed);
    }

    [Test]
    public async Task Test_PrimitiveParameterWithAttribute()
    {
        var result = await this.client.PrimitiveParameterWithAttributeAsync(NSwagClassHelper.NoPropertiesAttributed.ExampleString);
        result.Should().Be(NSwagClassHelper.NoPropertiesAttributed.ExampleString);
    }

    [Test]
    public async Task Test_ClassParameterWithAttributeSomePropertiesAttributed()
    {
        var result = await this.client.ClassParameterWithAttributeSomePropertiesAttributedAsync(NSwagClassHelper.SomePropertiesHeaderAttributed);
        result.ExampleDouble.Should().Be(NSwagClassHelper.SomePropertiesHeaderAttributed.ExampleDouble);
        result.ExampleString.Should().Be(NSwagClassHelper.SomePropertiesHeaderAttributed.ExampleString);
    }

    [Test]
    public async Task Test_PrimitiveParameterWithAttributeNamed()
    {
        var result = await this.client.PrimitiveParameterWithAttributeNamedAsync(NSwagClassHelper.NoPropertiesAttributed.ExampleString);
        result.Should().Be(NSwagClassHelper.NoPropertiesAttributed.ExampleString);
    }
}
