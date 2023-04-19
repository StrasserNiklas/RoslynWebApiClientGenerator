using ComplexTestingApi.CSharp;
using FluentAssertions;
using NUnit.Framework;

namespace TestingClient.ComplexApi.ACGT;

[TestFixture]
internal class FromQueryTestingHandler
{
    private FromQueryClient client;

    [SetUp]
    public void Setup()
    {
        var httpClient = HttpHelper.CreateHttpClient();
        var client = new FromQueryClient(httpClient);
        client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
        this.client = client;

    }

    [Test]
    public async Task Test_ClassParameterWithAttributeAllPropertiesAttributed()
    {
        var result = await this.client.ClassParameterWithAttributeAllPropertiesAttributedAsync(ACGTClassHelper.AllPropertiesQueryAttributed);
        result.SuccessResponse.Should().BeEquivalentTo(ACGTClassHelper.AllPropertiesQueryAttributed);
    }

    [Test]
    public async Task Test_ClassParameterWithAttributeNoPropertiesAttributed()
    {
        var result = await this.client.ClassParameterWithAttributeAsync(ACGTClassHelper.NoPropertiesAttributed);
        result.SuccessResponse.Should().BeEquivalentTo(ACGTClassHelper.NoPropertiesAttributed);
    }

    [Test]
    public async Task Test_ClassParameterWithAttributeSomePropertiesAttributed()
    {
        var result = await this.client.ClassParameterWithAttributeSomePropertiesAttributedAsync(ACGTClassHelper.SomePropertiesQueryAttributed);
        result.SuccessResponse.Should().BeEquivalentTo(ACGTClassHelper.SomePropertiesQueryAttributed);
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
        var result = await this.client.PrimitiveParameterNoAttributeAsync(ACGTClassHelper.NoPropertiesAttributed.ExampleString);
        result.SuccessResponse.Should().Be(ACGTClassHelper.NoPropertiesAttributed.ExampleString);
    }
}
