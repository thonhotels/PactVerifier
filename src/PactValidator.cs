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

        internal Result<bool> Validate(string version, string consumerName, string providerName)
        {
            Result<bool> r;
            r = ValidateVersion(version);
            if (r is Error<bool>)
                return r;

            r = ValidateConsumer(consumerName);
            if (r is Error<bool>)
                return r;
            r = ValidateProvider(providerName);
            return r;
        }

        private Result<bool> ValidateVersion(string expectedVersion)
        {
            var version = (string)(PactObject["metadata"]?["pactSpecification"]?["version"]);
            if (string.IsNullOrEmpty(version))
            {
                return new Error<bool>(Errors.Validation, "Pact version not specified in pact file!");
            }
            if (version != expectedVersion)
            {
                return new Error<bool>(Errors.Validation, $"Pact version ({version}) is not supported. Only version {expectedVersion} is supported!");
            }
            return new Ok<bool>(true);
        }

        private Result<bool> ValidateConsumer(string expectedConsumer)
        {
            var consumer = (string)PactObject["consumer"]?["name"];
            if (string.IsNullOrEmpty(consumer))
            {
                return new Error<bool>(Errors.Validation, "No consumer in pact file!");
            }
            if (consumer != expectedConsumer)
            {
                return new Error<bool>(Errors.Validation, $"Consumer '{expectedConsumer}' does not match consumer specified in pact file ('{consumer}')");
            }
            return new Ok<bool>(true);
        }

        private Result<bool> ValidateProvider(string expectedProviderName)
        {
            var provider = (string)PactObject["provider"]?["name"];
            if (string.IsNullOrEmpty(provider))
            {
                return new Error<bool>(Errors.Validation, "No provider in pact file!");
            }
            if (provider != expectedProviderName)
            {
                return new Error<bool>(Errors.Validation, $"Provider '{expectedProviderName}' does not match provider specified in pact file ('{provider}')");
            }
            return new Ok<bool>(true);
        }
    }
}