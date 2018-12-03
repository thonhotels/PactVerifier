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
            var validator = new PactValidator(pactObject);
            validator.Validate("2.0.0", consumerName, providerName);
            
            return pactObject;
        }
    }
}
