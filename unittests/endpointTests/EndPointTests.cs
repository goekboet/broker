using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace endpointTests
{
    public class Access
    {
        public string Accesstoken { get; set; }
        public string Refreshtoken { get; set; }
        public long ValidTo { get; set; }
    }

    [TestClass]
    public class EndpointTests
    {
        private static WebApplicationFactory<http.Startup> _factory;
        private static ServiceProvider Services()
        {
            var collection = new ServiceCollection();
            collection.AddDbContext<ApplicationDbContext>();

            collection.AddIdentity<ApplicationUser, IdentityRole>(o =>
            {
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequireUppercase = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>();
            collection.AddLogging();

            return collection.BuildServiceProvider();
        }

        [AssemblyInitialize]
        public static Task AssemblyInit(TestContext context)
        {
            _factory = WebApp.Get();

            return Task.CompletedTask;
        }

        static Task<Access> RandomAccess(
         long now,
         ServiceProvider sp
     )
        {
            var creds = Creds.RandomUser();
            return Token.FetchToken(sp, now, creds.Email, creds.Password);
        }

        [TestMethod]
        public async Task PostThenGetPublisher()
        {
            var access = await RandomAccess(
                DateTimeOffset.Now.ToUnixTimeMilliseconds(), 
                Services());

            var c = _factory.CreateClient();
            c.SetBearerToken(access.Accesstoken);
            var p = JsonSerializer.Serialize(
                new { name = Guid.NewGuid().ToString() });
            
            var post = await c.PostAsync(
                $"publishers", 
                new StringContent(p, Encoding.UTF8, "application/json"));
            Assert.IsTrue(post.IsSuccessStatusCode);

            var get = await c.GetAsync(post.Headers.Location);
            Assert.IsTrue(get.IsSuccessStatusCode);

            var list = await c.GetAsync("publishers");
            Assert.IsTrue(list.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task PostPublisherTwoTimes()
        {
            var access = await RandomAccess(
                DateTimeOffset.Now.ToUnixTimeMilliseconds(), 
                Services());

            var p1 = JsonSerializer.Serialize(
                new { name = Guid.NewGuid().ToString() });
            var p2 = JsonSerializer.Serialize(
                new { name = Guid.NewGuid().ToString() });

            var c = _factory.CreateClient();
            c.SetBearerToken(access.Accesstoken);
            var r1 = await c.PostAsync(
                $"publishers", 
                new StringContent(p1, Encoding.UTF8, "application/json"));
            var r2 = await c.PostAsync(
                $"publishers", 
                new StringContent(p2, Encoding.UTF8, "application/json"));

            Assert.IsTrue(
                new [] {r1, r2}.Select(x => x.IsSuccessStatusCode).All(x => x));

        }

        [TestMethod]
        public async Task PostPublisherTime()
        {
            var access = await RandomAccess(
                DateTimeOffset.Now.ToUnixTimeMilliseconds(), 
                Services());
            var c = _factory.CreateClient();
            c.SetBearerToken(access.Accesstoken);
             
            var now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var dur = (long)TimeSpan.FromMinutes(30).TotalMilliseconds;
            var time = new {
                    start = now,
                    name = "test",
                    end = now + dur 
                };

            var name = Guid.NewGuid().ToString();
            var host = await c.PostAsJsonAsync(
                "publishers",
                new { name = name }
            );
            Assert.IsTrue(host.IsSuccessStatusCode);

            var r = await c.PostAsJsonAsync(
                    $"publishers/{name}/times",
                    time);
            Assert.IsTrue(r.IsSuccessStatusCode);

            var r2 = await c.GetAsync(r.Headers.Location);
            Assert.IsTrue(r2.IsSuccessStatusCode);

            var json = await r2.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<object[]>(json);

            Assert.IsTrue(data?.Length > 0);
        }
    }
}