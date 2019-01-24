using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Thon.Hotels.PactVerifier
{
    public abstract class PactFetcher
    {
        protected abstract Task<Result<string>> ReadPact(string consumerName, string providerName, string tag);

        public async Task<Result<JObject>> GetPact(string consumerName, string providerName, string tag)
        {
            var result = await ReadPact(consumerName, providerName, tag);
            if (result is Error<string> e)
                return new Error<JObject>(Errors.Unknown, e.Messages.ToArray());
            var pactFile = result as Ok<string>;
            var pactObject = JsonConvert.DeserializeObject<JObject>(pactFile.Value);
            var validator = new PactValidator(pactObject);
            var validationResult = validator.Validate("2.0.0", consumerName, providerName);

            if (validationResult is Error<bool> er)
                return new Error<JObject>(Errors.Validation, er.Messages.ToArray());

            return new Ok<JObject>(pactObject);
        }
    }
}
