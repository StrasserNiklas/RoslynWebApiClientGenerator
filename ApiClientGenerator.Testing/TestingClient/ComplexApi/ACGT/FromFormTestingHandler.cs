using ComplexTestingApi.CSharp;
using FluentAssertions;
using NUnit.Framework;

namespace TestingClient.ComplexApi.ACGT;

[TestFixture]
internal class FromFormTestingHandler
{
    private FromFormClient client;

    [SetUp]
    public void Setup()
    {
        var httpClient = HttpHelper.CreateHttpClient();
        var client = new FromFormClient(httpClient);
        client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
        this.client = client;
    }

    [Test]
    public async Task Test_ClassParameterWithAttribute()
    {
        var result = await this.client.ClassParameterWithAttributeAsync(ACGTClassHelper.NoPropertiesAttributed);
        result.SuccessResponse.Should().BeEquivalentTo(ACGTClassHelper.NoPropertiesAttributed);
    }

    [Test]
    public async Task Test_SimpleParameterWithAttribute()
    {
        var result = await this.client.SimpleParameterWithAttributeAsync(ACGTClassHelper.NoPropertiesAttributed.ExampleString);
        result.SuccessResponse.Should().Be(ACGTClassHelper.NoPropertiesAttributed.ExampleString);
    }
}
