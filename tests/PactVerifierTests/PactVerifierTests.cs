using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Newtonsoft.Json;
using Thon.Hotels.PactVerifier;
using Xunit;

namespace PactVerifierTests
{
    public class FakeHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        JsonConvert.SerializeObject(
                            new {
                                TheCode = "someCode",
                                SomeProperty = "someValue"
                            }), Encoding.UTF8, "application/json")
                }
            );
        }
    }
    public class PactVerifierTests
    {
        [Fact]
        public async Task GetPactParsesJsonFile()
        {
            const string ServiceUri = "http://localhost:9222";
            var fetcher = new FilePactFetcher("TestPacts/Test1.json");
            var pactVerifier = new PactVerifier((condition, message) => Assert.True(condition, message), fetcher);
            await pactVerifier
                .ProviderState($"{ServiceUri}/provider-states")
                .ServiceProvider("theProvider", ServiceUri)
                .HonoursPactWith("theConsumer")
                .Verify(0, () => 
                            new HttpClient(new FakeHandler()) 
                            { 
                                BaseAddress = new System.Uri(ServiceUri)
                            }
                        );
        }
    }
}
