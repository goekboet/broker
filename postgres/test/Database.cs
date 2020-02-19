using System.Data.Common;
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
            cmd.Parameters.AddMany(
                new (string n, object v)[]
                {
                    ("dbname", dbname),
                });

            return cmd;
        }

        public static NpgsqlCommand RunSql(
            this NpgsqlConnection c,
            string sql) => new NpgsqlCommand(sql, c);

    }
}
