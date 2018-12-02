using System;
using System.IO;
using Thon.Hotels.PactVerifier;
using Xunit;

namespace PactVerifierTests
{
    public class PactFetcherTests
    {
        [Fact]
        public void Test1()
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

            var result = PactFetcher.GetPact(filename, "a consumer name", "a provider name");
        }
    }
}
