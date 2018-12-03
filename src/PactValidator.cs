using System;
using Newtonsoft.Json.Linq;

namespace Thon.Hotels.PactVerifier
{
    public class PactValidator
    {
        private JObject PactObject { get; }

        public PactValidator(JObject pactObject)
        {
            PactObject = pactObject;
        }

        internal void Validate(string version, string consumerName, string providerName)
        {
            ValidateVersion(version);
            ValidateConsumer(consumerName);
            ValidateProvider(providerName);
        }

        private void ValidateVersion(string expectedVersion)
        {
            var version = (string)(PactObject["metadata"]?["pactSpecification"]?["version"]);
            if (string.IsNullOrEmpty(version))
            {
                throw new Exception("Pact version not specified in pact file!");
            }
            if (version != expectedVersion)
            {
                throw new Exception($"Pact version ({version}) is not supported. Only version {expectedVersion} is supported!");
            }
        }

        private void ValidateConsumer(string expectedConsumer)
        {
            var consumer = (string)PactObject["consumer"]?["name"];
            if (string.IsNullOrEmpty(consumer))
            {
                throw new Exception("No consumer in pact file!");
            }
            if (consumer != expectedConsumer)
            {
                throw new Exception($"Consumer '{expectedConsumer}' does not match consumer specified in pact file ('{consumer}')");
            }
        }

        private void ValidateProvider(string expectedProviderName)
        {
            var provider = (string)PactObject["provider"]?["name"];
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