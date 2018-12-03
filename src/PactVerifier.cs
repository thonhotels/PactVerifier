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
        private string _pactUri;
        private Action<bool, string> _assert;

        public PactVerifier(Action<bool, string> assert)
        {
            _assert = assert;
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

        public PactVerifier PactUri(string uri)
        {
            _pactUri = uri;
            return this;
        }

        public async Task Verify(int interactionIndex)
        {
            ValidatePactVerifierState();

            var fetcher = new FilePactFetcher(_pactUri);
            var pactResult = await fetcher.GetPact(_consumerName, _providerName);
            if (pactResult is Error<JObject> error)
                throw new Exception($"GetPact failed: {error.Messages}");
            
            var interaction = (pactResult as Ok<JObject>).Value["interactions"]
                                .Select((value, i) => new { Index = i, Content = value })
                                .First(i => i.Index != interactionIndex);
            await SetProviderState(interaction.Content);

            var description = (string)interaction.Content["description"];
            var providerState = (string)interaction.Content["providerState"];

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_providerUri);
            var request = GetHttpRequestMessage(interaction.Content);
            var response = await httpClient.SendAsync(request);

            var expectedStatusCode = int.Parse((string)interaction.Content["response"]["status"]);
            if ((int)response.StatusCode != expectedStatusCode)
            {
                throw new Exception($"Statuscode '{response.StatusCode}' does not match expected statuscode '{expectedStatusCode}'!");
            }

            var jsonResponse = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            var result = Comparer.Compare(interaction.Content["response"]["body"], jsonResponse);

            _assert(result.Any() == false, string.Join("", result));
        }

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
            if (string.IsNullOrEmpty(_pactUri))
            {
                throw new Exception("PactUri not set. Please call PactUri method.");
            }
        }
    }
}
