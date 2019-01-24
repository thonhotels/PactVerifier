using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Thon.Hotels.PactVerifier;
using Xunit;

namespace PactVerifierTests
{
    public class PactFetcherTests
    {
        [Fact]
        public void GetPactParsesJsonFile()
        {
            const string filename = @"pactUri.json";
            const string json = @"
                {
                    ""consumer"": {
                        ""name"": ""a consumer name""
                    },
                    ""provider"": {
                        ""name"": ""a provider name""
                    },
                    ""metadata"": {
                    ""pactSpecification"": {
                        ""version"": ""2.0.0""
                    }
                }
            ";
            File.WriteAllText(filename, json);

            var result = new FilePactFetcher(filename).GetPact("a consumer name", "a provider name", "a tag name");
        }

        [Fact(Skip = "user-secrets required")]
        public async Task HttpFetcherGetsFile()
        {
            string base64Encode(string plainText)
            {
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                return System.Convert.ToBase64String(plainTextBytes);
            }

            var (username, password, url) = GetConfigSettings();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", base64Encode($"{username}:{password}"));
            client.BaseAddress = new Uri(url);

            var result = await new HttpPactFetcher(client)
            .GetPact("Atlas.hotel.app.backend", "Atlas.Company", "dev");
        }

        private (string username, string password, string url)
            GetConfigSettings()
        {
            var config = new ConfigurationBuilder()
                                .AddUserSecrets<PactFetcherTests>()
                                .Build(); ;

            var username = config["username"];
            var password = config["password"];
            var url = config["pactbroker-url"];
            return (username, password, url);
        }
    }
}
