using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace cli
{
    public class Access
    {
        public string Accesstoken { get; set; }
        public string Refreshtoken { get; set; }
        public long ValidTo { get; set; }
    }

    class Program
    {
        static Task<Access> RandomAccess(
         long now,
         ServiceProvider sp
     )
        {
            var creds = Creds.RandomUser();
            return Token.FetchToken(sp, now, creds);
        }



        static async Task<int> Main(string[] args)
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
            var sp = collection.BuildServiceProvider();

            var seed = Creds.RandomUser();
            var usr = seed.Email;
            if (args.Length > 0)
            {
                usr = args[0];
            }
            Console.WriteLine($"username: {seed.Email} pwd: {seed.Password}");

            var now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var access = await Token.GetAccess(sp, now, seed);

            Console.WriteLine(access.Accesstoken);
            return 0;
        }
    }
}
