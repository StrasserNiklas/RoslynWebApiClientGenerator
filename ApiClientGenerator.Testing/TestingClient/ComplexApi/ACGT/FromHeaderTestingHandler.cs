using ComplexTestingApi.CSharp;
using FluentAssertions;
using NUnit.Framework;

namespace TestingClient.ComplexApi.ACGT;

[TestFixture]
internal class FromHeaderTestingHandler
{
    private FromHeaderClient client;

    [SetUp]
    public void Setup()
    {
        var httpClient = HttpHelper.CreateHttpClient();
        var client = new FromHeaderClient(httpClient);
        client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
        this.client = client;

    }

    [Test]
    public async Task Test_ClassParameterWithAttributeAllPropertiesAttributed()
    {
        var result = await this.client.ClassParameterWithAttributeAllPropertiesAttributedAsync(ACGTClassHelper.AllPropertiesHeaderAttributed);
        result.SuccessResponse.Should().BeEquivalentTo(ACGTClassHelper.AllPropertiesHeaderAttributed);
    }

    [Test]
    public async Task Test_PrimitiveParameterWithAttribute()
    {
        var result = await this.client.PrimitiveParameterWithAttributeAsync(ACGTClassHelper.NoPropertiesAttributed.ExampleString);
        result.SuccessResponse.Should().Be(ACGTClassHelper.NoPropertiesAttributed.ExampleString);
    }

    [Test]
    public async Task Test_ClassParameterWithAttributeSomePropertiesAttributed()
    {
        var result = await this.client.ClassParameterWithAttributeSomePropertiesAttributedAsync(ACGTClassHelper.SomePropertiesHeaderAttributed.ExampleString, ACGTClassHelper.SomePropertiesHeaderAttributed.ExampleDouble);
        result.SuccessResponse.ExampleDouble.Should().Be(ACGTClassHelper.SomePropertiesHeaderAttributed.ExampleDouble);
        result.SuccessResponse.ExampleString.Should().Be(ACGTClassHelper.SomePropertiesHeaderAttributed.ExampleString);
    }

    [Test]
    public async Task Test_PrimitiveParameterWithAttributeNamed()
    {
        var result = await this.client.PrimitiveParameterWithAttributeNamedAsync(ACGTClassHelper.NoPropertiesAttributed.ExampleString);
        result.SuccessResponse.Should().Be(ACGTClassHelper.NoPropertiesAttributed.ExampleString);
    }
}
