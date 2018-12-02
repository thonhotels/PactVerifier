using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Thon.Hotels.PactVerifier
{
    public class PactFetcher
    {
        public static JObject GetPact(string pactUri, string consumerName, string providerName)
        {
            var pactFile = File.ReadAllText(pactUri);
            var pactObject = JsonConvert.DeserializeObject<JObject>(pactFile);
            ValidateVersion(pactObject, "2.0.0");
            ValidateConsumer(pactObject, consumerName);
            ValidateProvider(pactObject, providerName);
            return pactObject;
        }

        private static void ValidateVersion(JObject json, string expectedVersion)
        {
            var version = (string)(json["metadata"]?["pactSpecification"]?["version"]);
            if (string.IsNullOrEmpty(version))
            {
                throw new Exception("Pact version not specified in pact file!");
            }
            if (version != expectedVersion)
            {
                throw new Exception($"Pact version ({version}) is not supported. Only version {expectedVersion} is supported!");
            }
        }

        private static void ValidateConsumer(JObject json, string expectedConsumer)
        {
            var consumer = (string)json["consumer"]?["name"];
            if (string.IsNullOrEmpty(consumer))
            {
                throw new Exception("No consumer in pact file!");
            }
            if (consumer != expectedConsumer)
            {
                throw new Exception($"Consumer '{expectedConsumer}' does not match consumer specified in pact file ('{consumer}')");
            }
        }

        private static void ValidateProvider(JObject json, string expectedProviderName)
        {
            var provider = (string)json["provider"]?["name"];
            if (string.IsNullOrEmpty(provider))
            {
                throw new Exception("No provider in pact file!");
            }
            if (provider != expectedProviderName)
            {
                throw new Exception($"Provider '{expectedProviderName}' does not match provider specified in pact file ('{provider}')");
            }
        }
    }
}
