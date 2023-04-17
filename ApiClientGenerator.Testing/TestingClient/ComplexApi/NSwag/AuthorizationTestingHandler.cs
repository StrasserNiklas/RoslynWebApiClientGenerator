using FluentAssertions;
using NSwag.ComplexTestingApi.CSharp;
using NSwag.SimpleTestingApi.CSharp;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingClient.ComplexApi.NSwag;

[TestFixture]
public class AuthorizationTestingHandler
{
    private AuthorizationClient client;
    private HealthCheckOptions healthCheckOptions;

    [SetUp]
    public void Setup()
    { 
        var httpClient = HttpClientHelper.CreateHttpClient();
        var client = new AuthorizationClient(httpClient);
        //client.
        //client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
        this.client = client;
        //this.healthCheckOptions = new HealthCheckOptions() { Interval = 1, Path = "test" };

    }

    [Test]
    public async Task Test_PostHealthCheckOptions()
    {
        
        //var result = await this.healthCheckClient.PostHealthCheckOptionsAsync(this.healthCheckOptions);
        //healthCheckOptions.Should().BeEquivalentTo(result);
    }

}

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
        var httpClient = HttpClientHelper.CreateHttpClient();
        var client = new CombinedAttributesClient(httpClient);
        //client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
        this.client = client;

    }

    [Test]
    public async Task Test_ComplexHeaderSimpleRouteSimpleQuerySimpleBody()
    {
        var result = await this.client.ComplexHeaderSimpleRouteSimpleQuerySimpleBodyAsync(
            combinedClass.Id, 
            combinedClass.Name, 
            combinedClass.SimpleBodyClass, 
            combinedClass.Intervall);

        combinedClass.Should().BeEquivalentTo(result);
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

        combinedClass.Should().BeEquivalentTo(result);
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

        combinedClass.Should().BeEquivalentTo(result);
    }

    [Test]
    public async Task Test_SimpleHeaderSimpleRouteSimpleQueryComplexBody()
    {
        var result = await this.client.SimpleHeaderSimpleRouteSimpleQueryComplexBodyAsync(
            combinedClass.Id, 
            combinedClass.Name, 
            combinedClass.Intervall, 
            combinedClass.SimpleBodyClass);

        combinedClass.Should().BeEquivalentTo(result);
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

        combinedClass.Should().BeEquivalentTo(result);
    }
}

[TestFixture]
internal class ComplexAndContractsTestingHandler
{
    //private IHealthCheckClient client;
    //private HealthCheckOptions healthCheckOptions;

    [SetUp]
    public void Setup()
    {
        //var client = new HealthCheckClient(new HttpClient() { BaseAddress = new Uri("https://localhost:7205") });
        //client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
        //this.client = client;
        //this.healthCheckOptions = new HealthCheckOptions() { Interval = 1, Path = "test" };

    }

    [Test]
    public async Task Test_PostHealthCheckOptions()
    {
        //var result = await this.healthCheckClient.PostHealthCheckOptionsAsync(this.healthCheckOptions);
        //healthCheckOptions.Should().BeEquivalentTo(result.SuccessResponse);
    }
}

[TestFixture]
internal class FromBodyTestingHandler
{
    //private IHealthCheckClient client;
    //private HealthCheckOptions healthCheckOptions;

    [SetUp]
    public void Setup()
    {
        //var client = new HealthCheckClient(new HttpClient() { BaseAddress = new Uri("https://localhost:7205") });
        //client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
        //this.client = client;
        //this.healthCheckOptions = new HealthCheckOptions() { Interval = 1, Path = "test" };

    }

    [Test]
    public async Task Test_PostHealthCheckOptions()
    {
        //var result = await this.healthCheckClient.PostHealthCheckOptionsAsync(this.healthCheckOptions);
        //healthCheckOptions.Should().BeEquivalentTo(result.SuccessResponse);
    }
}

[TestFixture]
internal class FromFormTestingHandler
{
    //private IHealthCheckClient client;
    //private HealthCheckOptions healthCheckOptions;

    [SetUp]
    public void Setup()
    {
        //var client = new HealthCheckClient(new HttpClient() { BaseAddress = new Uri("https://localhost:7205") });
        //client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
        //this.healthCheckClient = client;
        //this.healthCheckOptions = new HealthCheckOptions() { Interval = 1, Path = "test" };

    }

    [Test]
    public async Task Test_PostHealthCheckOptions()
    {
        //var result = await this.healthCheckClient.PostHealthCheckOptionsAsync(this.healthCheckOptions);
        //healthCheckOptions.Should().BeEquivalentTo(result.SuccessResponse);
    }
}

[TestFixture]
internal class FromHeaderTestingHandler
{
    //private IHealthCheckClient client;
    //private HealthCheckOptions healthCheckOptions;

    [SetUp]
    public void Setup()
    {
        //var client = new HealthCheckClient(new HttpClient() { BaseAddress = new Uri("https://localhost:7205") });
        //client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
        //this.client = client;
        //this.healthCheckOptions = new HealthCheckOptions() { Interval = 1, Path = "test" };

    }

    [Test]
    public async Task Test_PostHealthCheckOptions()
    {
        //var result = await this.healthCheckClient.PostHealthCheckOptionsAsync(this.healthCheckOptions);
        //healthCheckOptions.Should().BeEquivalentTo(result.SuccessResponse);
    }
}

[TestFixture]
internal class FromQueryTestingHandler
{
    //private IHealthCheckClient client;
    //private HealthCheckOptions healthCheckOptions;

    [SetUp]
    public void Setup()
    {
        //var client = new HealthCheckClient(new HttpClient() { BaseAddress = new Uri("https://localhost:7205") });
        //client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
        //this.client = client;
        //this.healthCheckOptions = new HealthCheckOptions() { Interval = 1, Path = "test" };

    }

    [Test]
    public async Task Test_PostHealthCheckOptions()
    {
        //var result = await this.healthCheckClient.PostHealthCheckOptionsAsync(this.healthCheckOptions);
        //healthCheckOptions.Should().BeEquivalentTo(result.SuccessResponse);
    }
}

[TestFixture]
internal class FromRouteTestingHandler
{
    //private IHealthCheckClient client;
    //private HealthCheckOptions healthCheckOptions;

    [SetUp]
    public void Setup()
    {
        //var client = new HealthCheckClient(new HttpClient() { BaseAddress = new Uri("https://localhost:7205") });
        //client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
        //this.client = client;
        //this.healthCheckOptions = new HealthCheckOptions() { Interval = 1, Path = "test" };

    }

    [Test]
    public async Task Test_PostHealthCheckOptions()
    {
        //var result = await this.healthCheckClient.PostHealthCheckOptionsAsync(this.healthCheckOptions);
        //healthCheckOptions.Should().BeEquivalentTo(result.SuccessResponse);
    }
}

[TestFixture]
internal class ProducesResponseTypesTestingHandler
{
    //private IHealthCheckClient client;
    //private HealthCheckOptions healthCheckOptions;

    [SetUp]
    public void Setup()
    {
        //var client = new HealthCheckClient(new HttpClient() { BaseAddress = new Uri("https://localhost:7205") });
        //client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
        //this.client = client;
        //this.healthCheckOptions = new HealthCheckOptions() { Interval = 1, Path = "test" };

    }

    [Test]
    public async Task Test_PostHealthCheckOptions()
    {
        //var result = await this.healthCheckClient.PostHealthCheckOptionsAsync(this.healthCheckOptions);
        //healthCheckOptions.Should().BeEquivalentTo(result.SuccessResponse);
    }
}

[TestFixture]
internal class MinimalApiTestingHandler
{
    //private IHealthCheckClient client;
    //private HealthCheckOptions healthCheckOptions;

    [SetUp]
    public void Setup()
    {
        //var client = new HealthCheckClient(new HttpClient() { BaseAddress = new Uri("https://localhost:7205") });
        //client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
        //this.client = client;
        //this.healthCheckOptions = new HealthCheckOptions() { Interval = 1, Path = "test" };

    }

    [Test]
    public async Task Test_PostHealthCheckOptions()
    {
        //var result = await this.healthCheckClient.PostHealthCheckOptionsAsync(this.healthCheckOptions);
        //healthCheckOptions.Should().BeEquivalentTo(result.SuccessResponse);
    }
}
