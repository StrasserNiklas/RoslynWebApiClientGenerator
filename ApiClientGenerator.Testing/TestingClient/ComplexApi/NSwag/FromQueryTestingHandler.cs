using FluentAssertions;
using NSwag.ComplexTestingApi.CSharp;
using NUnit.Framework;

namespace TestingClient.ComplexApi.NSwag;

[TestFixture]
internal class FromQueryTestingHandler
{
    private FromQueryClient client;

    [SetUp]
    public void Setup()
    {
        var httpClient = HttpHelper.CreateHttpClient();
        var client = new FromQueryClient(httpClient);
        client.JsonSerializerSettings.WithAllPossiblyNecessarySettings();
        this.client = client;
    }

    [Test]
    public async Task Test_ClassParameterWithAttributeAllPropertiesAttributed()
    {
        var result = await this.client.ClassParameterWithAttributeAllPropertiesAttributedAsync(NSwagClassHelper.AllPropertiesQueryAttributed.ExampleString, NSwagClassHelper.AllPropertiesQueryAttributed.ExampleInteger);
        result.Should().BeEquivalentTo(NSwagClassHelper.AllPropertiesQueryAttributed);
    }

    [Test]
    public async Task Test_ClassParameterWithAttributeNoPropertiesAttributed()
    {
        var result = await this.client.ClassParameterWithAttributeAsync(NSwagClassHelper.NoPropertiesAttributed.ExampleString, NSwagClassHelper.NoPropertiesAttributed.ExampleInteger);
        result.Should().BeEquivalentTo(NSwagClassHelper.NoPropertiesAttributed);
    }

    [Test]
    public async Task Test_ClassParameterWithAttributeSomePropertiesAttributed()
    {
        var result = await this.client.ClassParameterWithAttributeSomePropertiesAttributedAsync(NSwagClassHelper.SomePropertiesQueryAttributed.ExampleString, NSwagClassHelper.SomePropertiesQueryAttributed.ExampleInteger);
        result.Should().BeEquivalentTo(NSwagClassHelper.SomePropertiesQueryAttributed);
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
        var result = await this.client.PrimitiveParameterNoAttributeAsync(NSwagClassHelper.NoPropertiesAttributed.ExampleString);
        result.Should().Be(NSwagClassHelper.NoPropertiesAttributed.ExampleString);
    }
}
