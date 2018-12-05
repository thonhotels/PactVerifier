using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Thon.Hotels.PactVerifier
{
    public class PactVerifier
    {
        private string _providerStateUrl;
        private string _providerName;
        private string _providerUri;
        private string _consumerName;
        private Action<bool, string> Assert { get; }
        private PactFetcher Fetcher { get; }

        public PactVerifier(Action<bool, string> assert, PactFetcher fetcher)
        {
            if (fetcher == null) throw new ArgumentNullException(nameof(fetcher));
            if (fetcher == null) throw new ArgumentNullException(nameof(fetcher));

            Assert = assert;
            Fetcher = fetcher;
        }

        public PactVerifier ProviderState(string providerStateUrl)
        {
            _providerStateUrl = providerStateUrl;
            return this;
        }
        public PactVerifier ServiceProvider(string providerName, string providerUri)
        {
            _providerName = providerName;
            _providerUri = providerUri;
            return this;
        }
        public PactVerifier HonoursPactWith(string consumerName)
        {
            _consumerName = consumerName;
            return this;
        }

        public async Task Verify(int interactionIndex, Func<HttpClient> clientFactory = null)
        {
            ValidatePactVerifierState();
            
            var pactResult = await Fetcher.GetPact(_consumerName, _providerName);
            if (pactResult is Error<JObject> error)
                throw new Exception($"GetPact failed: {string.Join(Environment.NewLine, error.Messages)}");
            
            var interaction = (pactResult as Ok<JObject>).Value["interactions"].ToArray()[interactionIndex];
            await SetProviderState(interaction);

            var description = (string)interaction["description"];
            var providerState = (string)interaction["providerState"];

            var client = CreateHttpClient(clientFactory);
            var request = GetHttpRequestMessage(interaction);
            var response = await client.SendAsync(request);

            var expectedStatusCode = int.Parse((string)interaction["response"]["status"]);
            if ((int)response.StatusCode != expectedStatusCode)
            {
                throw new Exception($"Statuscode '{response.StatusCode}' does not match expected statuscode '{expectedStatusCode}'!");
            }

            var jsonResponse = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            var result = Comparer.Compare(interaction["response"]["body"], jsonResponse);

            if (result.Any())
            {
                result = new [] { 
                    $"Interaction description: {description}{Environment.NewLine}",
                    $"Path: {request.RequestUri.AbsolutePath}{Environment.NewLine}",
                }.Concat(result);
            }

            Assert(result.Any() == false, string.Join("", result));
        }

        private HttpClient CreateHttpClient(Func<HttpClient> clientFactory) =>
            (clientFactory != null) ? 
                clientFactory():
                new HttpClient { BaseAddress = new Uri(_providerUri) };

        private async Task SetProviderState(JToken interactionToken)
        {
            var providerState = (string)interactionToken["providerState"];
            if (!string.IsNullOrEmpty(providerState))
            {
                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri($"{_providerUri}");
                var url = new Uri($"/{_providerStateUrl.Trim('/')}");
                var request = GetHttpRequestMessage(interactionToken);
                request.Content = new StringContent(JsonConvert.SerializeObject(new ProviderState { Consumer = _consumerName, State = providerState }), Encoding.UTF8, "application/json");
                var response = await httpClient.SendAsync(request);
                var jsonResponse = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());
            }
        }

        private static HttpRequestMessage GetHttpRequestMessage(JToken interactionToken)
        {
            var request = HttpRequestMessageFactory.Create(interactionToken);
            foreach (var header in interactionToken["request"]["headers"].ToObject<JObject>())
            {
                if (header.Key.ToLower() != "content-type")
                {
                    request.Headers.Add(header.Key, (string)header.Value);
                }
            }

            return request;
        }

        private void ValidatePactVerifierState()
        {
            if (string.IsNullOrEmpty(_providerName))
            {
                throw new Exception("ProviderName not set. Please call ServiceProvider method.");
            }
            if (string.IsNullOrEmpty(_providerUri))
            {
                throw new Exception("ProviderUri not set. Please call ServiceProvider method.");
            }
            if (string.IsNullOrEmpty(_consumerName))
            {
                throw new Exception("ConsumerName not set. Please call HonoursPactWith method.");
            }
        }
    }
}
