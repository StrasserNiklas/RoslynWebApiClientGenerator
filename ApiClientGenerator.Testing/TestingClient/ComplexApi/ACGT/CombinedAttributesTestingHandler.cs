using ComplexTestingApi.CSharp;
using FluentAssertions;
using NUnit.Framework;

namespace TestingClient.ComplexApi.ACGT;

[TestFixture]
internal class CombinedAttributesTestingHandler
{
    private CombinedAttributesClient client;
    private CombinedClass combinedClass = new CombinedClass()
    {
        Id = 27,
        Intervall = 10,
        Name = "CombinedTest",
        SimpleBodyClass = ACGTClassHelper.NoPropertiesAttributed
    };

    [SetUp]
    public void Setup()
    {
        var httpClient = HttpHelper.CreateHttpClient();
        var client = new CombinedAttributesClient(httpClient);
        client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
        this.client = client;
    }

    [Test]
    public async Task Test_ComplexHeaderSimpleRouteSimpleQuerySimpleBody()
    {
        var result = await this.client.ComplexHeaderSimpleRouteSimpleQuerySimpleBodyAsync(
            combinedClass.Id,
            combinedClass.Name,
            combinedClass.Intervall,
            new AllPropertiesHeaderAttributed() { ExampleInteger = combinedClass.SimpleBodyClass.ExampleInteger, ExampleString = combinedClass.SimpleBodyClass.ExampleString });

        result.SuccessResponse.Should().BeEquivalentTo(combinedClass);
    }

    [Test]
    public async Task Test_SimpleHeaderComplexRouteSimpleQuerySimpleBody()
    {
        var result = await this.client.SimpleHeaderComplexRouteSimpleQuerySimpleBodyAsync(
            combinedClass.Id,
            combinedClass.Name,
            combinedClass.Intervall,
            combinedClass.SimpleBodyClass);

        result.SuccessResponse.Should().BeEquivalentTo(combinedClass);
    }

    [Test]
    public async Task Test_SimpleHeaderSimpleRouteComplexQuerySimpleBody()
    {
        var result = await this.client.SimpleHeaderComplexRouteSimpleQuerySimpleBodyAsync(
            combinedClass.Id,
            combinedClass.Name,
            combinedClass.Intervall,
            combinedClass.SimpleBodyClass);

        result.SuccessResponse.Should().BeEquivalentTo(combinedClass);
    }

    [Test]
    public async Task Test_SimpleHeaderSimpleRouteSimpleQueryComplexBody()
    {
        var result = await this.client.SimpleHeaderSimpleRouteSimpleQueryComplexBodyAsync(
            combinedClass.Id,
            combinedClass.Name,
            combinedClass.Intervall,
            combinedClass.SimpleBodyClass);

        result.SuccessResponse.Should().BeEquivalentTo(combinedClass);
    }

    [Test]
    public async Task Test_ComplexHeaderComplexRouteComplexQueryComplexBody()
    {
        var result = await this.client.ComplexHeaderComplexRouteComplexQueryComplexBodyAsync(
            combinedClass.SimpleBodyClass,
            combinedClass.SimpleBodyClass,
            new AllPropertiesHeaderAttributed() { ExampleInteger = combinedClass.SimpleBodyClass.ExampleInteger, ExampleString = combinedClass.SimpleBodyClass.ExampleString },
            combinedClass.SimpleBodyClass);

        result.SuccessResponse.SimpleBodyClass1.Should().BeEquivalentTo(combinedClass.SimpleBodyClass);
        result.SuccessResponse.SimpleBodyClass2.Should().BeEquivalentTo(combinedClass.SimpleBodyClass);
        result.SuccessResponse.SimpleBodyClass3.Should().BeEquivalentTo(combinedClass.SimpleBodyClass);
        result.SuccessResponse.SimpleBodyClass4.Should().BeEquivalentTo(combinedClass.SimpleBodyClass);
    }
}
