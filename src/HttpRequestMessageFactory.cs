using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Thon.Hotels.PactVerifier
{
    public class HttpRequestMessageFactory
    {
        public static HttpRequestMessage Create(JToken interaction)
        {
            {
                var method = (string)interaction["request"]["method"];
                switch (method.ToLower())
                {
                    case "get":
                        return new HttpRequestMessage(HttpMethod.Get, GetUrl(interaction));
                    case "post":
                        return HttpRequestMessage(HttpMethod.Post, interaction);
                    case "put":
                        return HttpRequestMessage(HttpMethod.Put, interaction);
                    case "delete":
                        return new HttpRequestMessage(HttpMethod.Delete, GetUrl(interaction));
                    default:
                        throw new Exception($"HttpMethod '{method.ToLower()}' not supported");
                }
            }
        }

        private static HttpRequestMessage HttpRequestMessage(HttpMethod httpMethod, JToken interaction)
        {
            var httpRequestMessage = new HttpRequestMessage(httpMethod, GetUrl(interaction));
            
            httpRequestMessage.Content = interaction["request"]["body"] != null ?
                new StringContent(interaction["request"]["body"].ToString(), Encoding.UTF8, "application/json") :
                null;
            return httpRequestMessage;
        }

        private static string GetUrl(JToken interaction)
        {
            return (string)interaction["request"]["path"] + 
                (string.IsNullOrEmpty((string)interaction["request"]["query"]) ? "" :
                    "?" + (string)interaction["request"]["query"]);
        }
    }
}