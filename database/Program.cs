using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using postgres.test;
using postgres;
using System.Collections.Generic;

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
            var newDb = creds.ToConnection(false).NewTestDatabase(dbname);

            var newDbR = await newDb.SubmitCommand();
            creds.Db = dbname;
            var tablesUp = creds.ToConnection(false).RunSql(schema);
            var tablesUpR = await tablesUp.SubmitCommand();

            var hostData = File.ReadAllText("data/hosts.csv");
            await creds.ToConnection(false).Copy("hosts", new[] { "sub", "handle" }, hostData);

            creds.Db = "broker_test";
            var dropDb = creds.ToConnection(false).DropDatabase(dbname);
            var dropDbR = await dropDb.SubmitCommand();

            Console.WriteLine(dbname);
        }
    }
}
