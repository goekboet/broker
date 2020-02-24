using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using Npgsql;

namespace postgres.test
{
    public static class Database
    {
        
        public static NpgsqlCommand NewTestDatabase(
            this NpgsqlConnection c,
            string dbname)
        {
            var sql = $"create database {dbname};";
            var cmd = new NpgsqlCommand(sql, c);

            return cmd;
        }

        public static NpgsqlCommand DropDatabase(
            this NpgsqlConnection c,
            string dbname)
        {
            var sql = string.Join("\n", new []
            {
                $"SELECT pg_terminate_backend(pg_stat_activity.pid)",
                $"FROM pg_stat_activity",
                $"WHERE pg_stat_activity.datname = '{dbname}'",
                $"AND pid <> pg_backend_pid();",
                $"drop database {dbname};"
            });

            var cmd = new NpgsqlCommand(sql, c);

            return cmd;
        }

        public static NpgsqlCommand RunSql(
            this NpgsqlConnection c,
            string sql) => new NpgsqlCommand(sql, c);

        public static async Task Copy(
            this NpgsqlConnection c,
            string table,
            string[] columns, 
            string csv)
        {
            var cols = string.Join(", ", columns);
            var copy = $"COPY {table} ({cols}) FROM STDIN WITH csv header";
            await c.OpenAsync();
            using (var w = c.BeginTextImport(copy))
            {
                await w.WriteAsync(csv);
            }

            await c.CloseAsync();
        }
    }
}
