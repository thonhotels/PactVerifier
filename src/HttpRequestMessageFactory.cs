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
                var url = (string)interaction["request"]["path"];
                var method = (string)interaction["request"]["method"];
                switch (method.ToLower())
                {
                    case "get":
                        return new HttpRequestMessage(HttpMethod.Get, url);
                    case "post":
                        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                        AddBody(httpRequestMessage, interaction);
                        return httpRequestMessage;
                    case "put":
                        return new HttpRequestMessage(HttpMethod.Put, url);
                    case "delete":
                        return new HttpRequestMessage(HttpMethod.Delete, url);
                    default:
                        throw new Exception($"HttpMethod '{method.ToLower()}' not supported");
                }
            }
        }

        private static void AddBody(HttpRequestMessage httpRequestMessage, JToken interaction)
        {
            var body = interaction["request"]["body"];
            httpRequestMessage.Content = new StringContent(body.ToString(), Encoding.UTF8, "application/json");
        }
    }
}