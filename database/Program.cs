using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using postgres.test;
using postgres;

namespace database
{
    class Program
    {
        public static string Random =>
            new string(Guid.NewGuid().ToString().Where(x => x != '-').Take(8).ToArray());
            
        static async Task Main(string[] args)
        {
            var jsonconfig = File.ReadAllText("config.json");
            var schema = File.ReadAllText("tablesUp.sql");
            var creds = JsonSerializer.Deserialize<PgresUser>(jsonconfig);
            var dbname = $"test_broker_{Random}";
            var newDb = creds.ToConnection().NewTestDatabase(dbname);
            
            var newDbR = await newDb.SubmitCommand();
            creds.Db = dbname;
            var tablesUp = creds.ToConnection().RunSql(schema);
            var tablesUpR = await tablesUp.SubmitCommand();
            
            Console.WriteLine(dbname);
        }
    }
}
