using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Thon.Hotels.PactVerifier
{
    public class HttpPactFetcher : PactFetcher
    {
        private HttpClient Client { get; }

        public HttpPactFetcher(HttpClient client)
        {
            Client = client;
        }

        protected override async Task<Result<string>> ReadPact(string consumerName, string providerName)
        {
            var uri = $"pacts/provider/{providerName}/consumer/{consumerName}/latest";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await Client.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                return new Error<string>(Errors.Http, $"Http failed. {response.StatusCode} : {response.ReasonPhrase}");
            return new Ok<string>(await response.Content.ReadAsStringAsync());
        }
    }

    
}
