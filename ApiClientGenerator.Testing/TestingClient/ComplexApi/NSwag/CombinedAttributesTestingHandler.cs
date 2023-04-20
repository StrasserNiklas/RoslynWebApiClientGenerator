using FluentAssertions;
using NSwag.ComplexTestingApi.CSharp;
using NUnit.Framework;

namespace TestingClient.ComplexApi.NSwag;

[TestFixture]
internal class CombinedAttributesTestingHandler
{
    private CombinedAttributesClient client;
    private CombinedClass combinedClass = new CombinedClass()
    {
        Id = 27,
        Intervall = 10,
        Name = "CombinedTest",
        SimpleBodyClass = new NoPropertiesAttributedClass()
        {
            ExampleInteger = 69,
            ExampleString = "CombinedSimpleClass"
        }
    };

    [SetUp]
    public void Setup()
    {
        var httpClient = HttpHelper.CreateHttpClient();
        var client = new CombinedAttributesClient(httpClient);
        client.JsonSerializerSettings.WithAllPossiblyNecessarySettings();
        this.client = client;

    }

    [Test]
    public async Task Test_ComplexHeaderSimpleRouteSimpleQuerySimpleBody()
    {
        var result = await this.client.ComplexHeaderSimpleRouteSimpleQuerySimpleBodyAsync(
            combinedClass.Id,
            combinedClass.Name,
            new AllPropertiesHeaderAttributed() { ExampleInteger = combinedClass.SimpleBodyClass.ExampleInteger, ExampleString = combinedClass.SimpleBodyClass.ExampleString },
            combinedClass.Intervall);

        result.Should().BeEquivalentTo(combinedClass);
    }

    [Test]
    public async Task Test_SimpleHeaderComplexRouteSimpleQuerySimpleBody()
    {
        var result = await this.client.SimpleHeaderComplexRouteSimpleQuerySimpleBodyAsync(
            combinedClass.Id,
            combinedClass.Name,
            combinedClass.SimpleBodyClass.ExampleString,
            combinedClass.SimpleBodyClass.ExampleInteger,
            combinedClass.Intervall);

        result.Should().BeEquivalentTo(combinedClass);
    }

    [Test]
    public async Task Test_SimpleHeaderSimpleRouteComplexQuerySimpleBody()
    {
        var result = await this.client.SimpleHeaderComplexRouteSimpleQuerySimpleBodyAsync(
            combinedClass.Id,
            combinedClass.Name,
            combinedClass.SimpleBodyClass.ExampleString,
            combinedClass.SimpleBodyClass.ExampleInteger,
            combinedClass.Intervall);

        result.Should().BeEquivalentTo(combinedClass);
    }

    [Test]
    public async Task Test_SimpleHeaderSimpleRouteSimpleQueryComplexBody()
    {
        var result = await this.client.SimpleHeaderSimpleRouteSimpleQueryComplexBodyAsync(
            combinedClass.Id,
            combinedClass.Name,
            combinedClass.Intervall,
            combinedClass.SimpleBodyClass);

        result.Should().BeEquivalentTo(combinedClass);
    }

    [Test]
    public async Task Test_ComplexHeaderComplexRouteComplexQueryComplexBody()
    {
        var result = await this.client.ComplexHeaderComplexRouteComplexQueryComplexBodyAsync(
            combinedClass.SimpleBodyClass.ExampleString,
            combinedClass.SimpleBodyClass.ExampleInteger,
            combinedClass.SimpleBodyClass.ExampleString,
            combinedClass.SimpleBodyClass.ExampleInteger,
            new AllPropertiesHeaderAttributed() { ExampleInteger = combinedClass.SimpleBodyClass.ExampleInteger, ExampleString = combinedClass.SimpleBodyClass.ExampleString },
            combinedClass.SimpleBodyClass);

        result.Should().BeEquivalentTo(combinedClass);
    }
}
