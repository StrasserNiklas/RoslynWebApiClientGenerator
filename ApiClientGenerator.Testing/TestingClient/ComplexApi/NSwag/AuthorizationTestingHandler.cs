using NSwag.ComplexTestingApi.CSharp;
using NSwag.SimpleTestingApi.CSharp;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingClient.ComplexApi.NSwag
{
    [TestFixture]
    public class AuthorizationTestingHandler
    {
        private AuthorizationClient authorizationClient;
        private HealthCheckOptions healthCheckOptions;

        [SetUp]
        public void Setup()
        { 
            var httpClient = new HttpClient() { BaseAddress = new Uri("https://localhost:7205") };
            var client = new AuthorizationClient(httpClient);
            //client.
            //client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
            this.authorizationClient = client;
            //this.healthCheckOptions = new HealthCheckOptions() { Interval = 1, Path = "test" };

        }

        [Test]
        public async Task Test_PostHealthCheckOptions()
        {
            
            //var result = await this.healthCheckClient.PostHealthCheckOptionsAsync(this.healthCheckOptions);
            //healthCheckOptions.Should().BeEquivalentTo(result);
        }

    }
}
