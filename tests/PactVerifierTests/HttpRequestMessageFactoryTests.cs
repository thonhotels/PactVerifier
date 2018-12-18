using System;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Thon.Hotels.PactVerifier;
using Xunit;

namespace PactVerifierTests
{
    public class HttpRequestMessageFactoryTests
    {
        [Fact]
        public void GetRequestWithSimplePath()
        {
            var m = HttpRequestMessageFactory.Create(JToken.FromObject(new {request = new {
                method = "get",
                path = "/api/something"
            }}));
            Assert.NotNull(m);
            Assert.Equal(HttpMethod.Get, m.Method);
            Assert.Equal("/api/something", m.RequestUri.ToString());
        }

        [Fact]
        public void GetRequestWithQuery()
        {
            var m = HttpRequestMessageFactory.Create(JToken.FromObject(new {request = new {
                method = "get",
                path = "/api/something",
                query = "a=abc"
            }}));
            Assert.NotNull(m);
            Assert.Equal(HttpMethod.Get, m.Method);
            Assert.Equal("/api/something?a=abc", m.RequestUri.ToString());
        }

        [Fact]
        public void GetRequestWithMultipleQuery()
        {
            var m = HttpRequestMessageFactory.Create(JToken.FromObject(new {request = new {
                method = "get",
                path = "/api/something",
                query = "a=abc&d=xyz"
            }}));
            Assert.NotNull(m);
            Assert.Equal(HttpMethod.Get, m.Method);
            Assert.Equal("/api/something?a=abc&d=xyz", m.RequestUri.ToString());
        }

        [Fact]
        public void PostRequestWithSimplePath()
        {
            var m = HttpRequestMessageFactory.Create(JToken.FromObject(new {request = new {
                method = "post",
                path = "/api/something"
            }}));
            Assert.NotNull(m);
            Assert.Equal(HttpMethod.Post, m.Method);
            Assert.Equal("/api/something", m.RequestUri.ToString());
        }
    }
}