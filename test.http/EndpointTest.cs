using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Sut = http;

namespace test.http
{
     [TestClass]
    public class Endpoint
    {
        private static WebApplicationFactory<Sut.Startup> _factory;


        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            _factory = new WebApplicationFactory<Sut.Startup>();
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
           var r = await client.GetAsync(url);

           Assert.IsTrue(r.IsSuccessStatusCode);
        }

        public static long SomeStartDate = new DateTimeOffset(
            new DateTime(2019, 12, 24),
            TimeSpan.Zero).ToUnixTimeSeconds();

        Dictionary<string, string> Mockpayloads = new Dictionary<string, string>
        {
            ["/schedules"] = JObject.FromObject(
                new 
                {
                    timezone = "Europe/Stockholm",
                    handle = "someHandle",
                    start = "2019/32",
                    end = "2019/52"
                }
            ).ToString(),
            ["/schedules/someHandle/periods"] = JObject.FromObject(
                new 
                {
                    year = "2019",
                    day = "mon",
                    oclock = "15:00",
                    recur = 1
                }
            ).ToString(),
            ["/schedules/someHandle/bookings"] = "\"someMeet\""
        };

        async Task<string> ShowHttpResponse(HttpResponseMessage m)
        {
            var payload = await m.Content.ReadAsStringAsync();
            string headerval(IEnumerable<string> vs) => string.Join(" ", vs);

            var headers = m.Headers.Select(x => $"{x}: {headerval(x.Value)}");

            return string.Join(Environment.NewLine, new []
            {
                $"{m.StatusCode} {m.ReasonPhrase}",
                payload
            }); 
        } 

        [TestMethod]
        [DataRow("/schedules")]
        [DataRow("/schedules/someHandle/periods")]
        [DataRow("/schedules/someHandle/bookings")]
        public async Task PostEndpoints(string url)
        {
            var client = _factory.CreateClient();

            var payload = new StringContent(
                Mockpayloads[url],
                Encoding.UTF8,
                "application/json");

            var r = await client.PostAsync(url, payload);

            Assert.IsTrue(r.IsSuccessStatusCode, 
            $"{await ShowHttpResponse(r)}");
        }

        [TestMethod]
        [DataRow("/schedules/somehandle")]
        [DataRow("/schedules/someHandle/periods/someId")]
        [DataRow("/schedules/someHandle/bookings/someId")]
        public async Task DeleteEndpoints(string url)
        {
            var client = _factory.CreateClient();

            var r = await client.DeleteAsync(url);

            Assert.IsTrue(r.IsSuccessStatusCode, 
            $"{await ShowHttpResponse(r)}");
        } 
    }
}
