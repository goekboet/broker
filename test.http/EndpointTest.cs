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
using Newtonsoft.Json;
using IdentityModel.Client;
using System.Net;

namespace test.http
{
    [TestClass]
    public class Endpoint
    {
        private static WebApplicationFactory<Sut.Startup> _factory;
        
        private static string AccessToken; 

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
        public static async Task AssemblyInit(TestContext context)
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

            var idpclient = new HttpClient();
            var idpreply  = await idpclient.RequestPasswordTokenAsync(
                new PasswordTokenRequest
                {
                    Address = "https://ids.ego/connect/token",

                    ClientId = "dev",
                    ClientSecret = "dev",
                    Scope = "openid bookings",

                    UserName = "dev",
                    Password = "dev"
                });

            AccessToken = idpreply.AccessToken;

            Assert.IsFalse(idpreply.IsError, "Failed to aquire token from idp.");
        }

        [TestMethod]
        public async Task GetHosts()
        {
            var c = _factory.CreateClient();

            var res = await c.GetAsync("hosts");
            Assert.IsTrue(res.IsSuccessStatusCode);
            
            var bdy = await res.Content.ReadAsStringAsync();
            var rep = JsonConvert.DeserializeObject<object[]>(bdy);
            Assert.IsTrue(rep.Length > 0);
        }

        [TestMethod]
        public async Task GetMeets()
        {
            var host = Guid.Parse("b40eb02e-8783-454b-9784-fd56ecdf5bc6");
            long from = 1568592000000;
            long to = 1569196800000;

            var c = _factory.CreateClient();

            var res = await c.GetAsync($"hosts/{host}/times?from={from}&to={to}");
            Assert.IsTrue(res.IsSuccessStatusCode);

            var bdy = await res.Content.ReadAsStringAsync();
            var rep = JsonConvert.DeserializeObject<object[]>(bdy);
            Assert.IsTrue(rep.Length > 0);
        }

        public const long SomeStart = 1568635200000;

        [TestMethod]
        public async Task Book()
        {
            var apiclient = _factory.CreateClient();
            apiclient.SetBearerToken(AccessToken);

            var r = await apiclient.PostAsJsonAsync("bookings", 
                new 
                {
                    HostId = "b40eb02e-8783-454b-9784-fd56ecdf5bc6",
                    Start = SomeStart
                });

            Assert.IsTrue(r.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task DoubleBook()
        {
            var apiclient = _factory.CreateClient();
            apiclient.SetBearerToken(AccessToken);

            var r = await apiclient.PostAsJsonAsync("bookings", 
                new 
                {
                    HostId = "4c640f8c-90b6-4c9e-ade9-ed4652550338",
                    Start = 1568998800000
                });

            Assert.IsTrue(r.StatusCode == HttpStatusCode.Conflict);
        }

        [TestMethod]
        public async Task UnBook()
        {
            var apiclient = _factory.CreateClient();
            
            apiclient.SetBearerToken(AccessToken);

            var r = await apiclient.DeleteAsync($"bookings/{SomeStart}");

            Assert.IsTrue(r.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task GetMyBookings()
        {
            var apiclient = _factory.CreateClient();
            apiclient.SetBearerToken(AccessToken);

            var r = await apiclient.GetAsync("bookings");
            r.EnsureSuccessStatusCode();

            var bdy = await r.Content.ReadAsStringAsync();
            var rep = JsonConvert.DeserializeObject<object[]>(bdy);
            Assert.IsNotNull(rep);
        }
    }
}
