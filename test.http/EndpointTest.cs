using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Sut = http;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using postgres;
using System.Threading.Tasks;
using System;

namespace test.http
{
    [TestClass]
    public class Endpoint
    {
        private static WebApplicationFactory<Sut.Startup> _factory;
        private static HttpClient _http;

        public static PgresUser Broker => new PgresUser(
                "localhost",
                "5432",
                "broker",
                "trtLAqkGY3nE3DyA",
                "meets"
            );
            
        private static IEnumerable<KeyValuePair<string,string>> TestConf = new 
            Dictionary<string, string>()
            {
                ["Pgres:Host"] = "localhost",
                ["Pgres:Port"] = "5432",
                ["Pgres:Handle"] = "broker",
                ["Pgres:Pwd"] = "trtLAqkGY3nE3DyA",
                ["Pgres:Db"] = "meets"
            };

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

                    opts.ConfigureAppConfiguration((ctx, cnf) => 
                    {
                        cnf.AddInMemoryCollection(TestConf);
                    });
                });
            _http = new HttpClient();
        }

        [TestMethod]
        public async Task GetHosts()
        {
            var c = _factory.CreateClient();

            var res = await c.GetAsync("hosts");
            var bdy = await res.Content.ReadAsStringAsync();
            
            Assert.IsTrue(res.IsSuccessStatusCode);
            Assert.IsTrue(bdy.Length > 0);
        }

        [TestMethod]
        public async Task GetMeets()
        {
            var host = Guid.Parse("b40eb02e-8783-454b-9784-fd56ecdf5bc6");
            long from = 1568592000000;
            long to = 1569196800000;

            var c = _factory.CreateClient();

            var res = await c.GetAsync($"hosts/{host}/times?from={from}&to={to}");
            var bdy = await res.Content.ReadAsStringAsync();
            
            Assert.IsTrue(res.IsSuccessStatusCode);
            Assert.IsTrue(bdy.Length > 0);
        }
    }
}
