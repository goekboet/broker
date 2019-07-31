using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Sut = http;

namespace test.http
{
    [TestClass]
    public class Endpoint
    {
        private static WebApplicationFactory<Sut.Startup> _factory;
        private static HttpClient _http;


        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            _factory = new WebApplicationFactory<Sut.Startup>()
                .WithWebHostBuilder(opts => 
                {
                    opts.ConfigureLogging((hostingContext, logging) =>
                    {
                        // Requires `using Microsoft.Extensions.Logging;`
                        logging.ClearProviders();
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Information);
                    });
                });
            _http = new HttpClient();
        }

        public static Dictionary<string, string> Scopes = new Dictionary<string, string>
        {
            ["/schedules"] = "openid schedule",
            ["/schedules/someHandle"] = "openid schedule",
            ["/schedules/someHandle/meets"] = "",
            ["/schedules/someHandle/bookings"] = "openid call",
            ["/schedules/someHandle/bookings/someBooking"] = "openid call"
        };

        public async Task<HttpRequestMessage> GetRequest(
            string url,
            string scopes)
        {
            var m = new HttpRequestMessage();
                m.Method = HttpMethod.Get;
                m.RequestUri = new Uri(url, UriKind.Relative);

            if (scopes == null && scopes.Length > 0)
            {
                return m;
            }
            else
            {
                var token = await GetAccessTokenWithScopes(scopes);
                m.SetBearerToken(token);

                return m;
            }
        }

        // Mark that this is a unit test method. (Required)
        [TestMethod]
        [DataRow("/schedules")]
        [DataRow("/schedules/someHandle")]
        [DataRow("/schedules/someHandle/meets")]
        [DataRow("/schedules/someHandle/bookings")]
        [DataRow("/schedules/someHandle/bookings/someBooking")]
        public async Task GetEndpoints(string url)
        {
            var client = _factory.CreateClient();
            var req = await GetRequest(url, Scopes[url]);
            var r = await client.SendAsync(req);
            
            Assert.IsTrue(
                r.IsSuccessStatusCode,
                $"{await ShowHttpResponse(r)}");
        }

        public static long SomeStartDate = new DateTimeOffset(
            new DateTime(2019, 12, 24),
            TimeSpan.Zero).ToUnixTimeSeconds();

        Dictionary<string, (string payload, string scopes)> Mockpayloads = new Dictionary<string, (string payload, string claim)>
        {
            ["/schedules"] = (JObject.FromObject(
                new
                {
                    timezone = "Europe/Stockholm",
                    handle = "someHandle",
                    start = "2019/32",
                    end = "2019/52"
                }).ToString(),
                "schedule"),
            ["/schedules/someHandle/periods"] = (
                JObject.FromObject(new
                {
                    year = "2019",
                    day = "mon",
                    oclock = "15:00",
                    recur = 1
                }).ToString(),"schedule"),
            ["/schedules/someHandle/bookings"] = (
                "\"someMeet\"", 
                "call")
        };

        static ConcurrentDictionary<string, string> cache =
            new ConcurrentDictionary<string, string>();
        async Task<string> GetAccessTokenWithScopes(string scopes)
        {
            if (!cache.ContainsKey(scopes))
            {
                var response = await _http.RequestPasswordTokenAsync(
                new PasswordTokenRequest
                {
                    Address = "https://ids.ego/connect/token",
                    ClientId = "dev",
                    ClientSecret = "dev",
                    Scope = scopes,
                    UserName = "dev",
                    Password = "dev"
                });
                cache[scopes] = response.AccessToken;
            }
            
            return cache[scopes];
        }

        async Task<string> ShowHttpResponse(HttpResponseMessage m)
        {
            var payload = await m.Content.ReadAsStringAsync();
            string headerval(IEnumerable<string> vs) => string.Join(" ", vs);

            var headers = m.Headers.Select(x => $"{x}: {headerval(x.Value)}");

            return string.Join(Environment.NewLine, new[]
            {
                $"{m.StatusCode.ToString("D")} {m.ReasonPhrase}",
                payload
            });
        }

        async Task<HttpRequestMessage> PostMessage(
            string url, 
            string content, 
            string scopes)
        {
            var m = new HttpRequestMessage();
                m.Method = HttpMethod.Post;
                m.RequestUri = new Uri(url, UriKind.Relative);
                m.Content = new StringContent(
                    content, 
                    Encoding.UTF8,
                    "application/json");

            if (scopes == null && scopes.Length > 0)
            {
                return m;
            }
            else
            {
                var token = await GetAccessTokenWithScopes(scopes);
                m.SetBearerToken(token);

                return m;
            }
        }

        [TestMethod]
        [DataRow("/schedules")]
        [DataRow("/schedules/someHandle/periods")]
        [DataRow("/schedules/someHandle/bookings")]
        public async Task PostEndpoints(string url)
        {
            var client = _factory.CreateClient();
            var data = Mockpayloads[url];
            var request = await PostMessage(
                url,
                data.payload,
                data.scopes
            );

            var r = await client.SendAsync(request);

            Assert.IsTrue(r.IsSuccessStatusCode,
            $"{await ShowHttpResponse(r)}");
        }

        Dictionary<string, string> DeleteScopes = new Dictionary<string, string>
        {
            ["/schedules/somehandle"] = "schedule",
            ["/schedules/someHandle/periods/someId"] = "schedule",
            ["/schedules/someHandle/bookings/someId"] = "call"
        };

        async Task<HttpRequestMessage> DeleteMessage(
            string url, 
            string scopes)
        {
            var m = new HttpRequestMessage();
                m.Method = HttpMethod.Delete;
                m.RequestUri = new Uri(url, UriKind.Relative);

            if (scopes == null && scopes.Length > 0)
            {
                return m;
            }
            else
            {
                var token = await GetAccessTokenWithScopes(scopes);
                m.SetBearerToken(token);

                return m;
            }
        }

        [TestMethod]
        [DataRow("/schedules/somehandle")]
        [DataRow("/schedules/someHandle/periods/someId")]
        [DataRow("/schedules/someHandle/bookings/someId")]
        public async Task DeleteEndpoints(string url)
        {
            var client = _factory.CreateClient();
            var req = await DeleteMessage(url, DeleteScopes[url]);

            var r = await client.SendAsync(req);;

            Assert.IsTrue(r.IsSuccessStatusCode,
            $"{await ShowHttpResponse(r)}");
        }
    }
}
