using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;

namespace postgres
{
    public class PgresUser
    {
        public PgresUser(
            string host,
            string port,
            string handle,
            string pwd,
            string db
            )
        {
            Host = host;
            Port = port;
            Handle = handle;
            Pwd = pwd;
            Db = db;
        }
        public string Host { get; set; }
        public string Port { get; set; }
        public string Handle { get; set; }
        public string Pwd { get; set; }
        public string Db { get; set; }

        public override string ToString() =>
            $"Host={Host};Port={Port};Username={Handle};Password={Pwd};Database={Db}";
    }

    public static class PGres
    {
        public static async Task<IEnumerable<S>> SubmitQuery<S>(
            this NpgsqlCommand cmd,
            Func<IDataRecord, S> selector)
        {
            var rs = new List<S>();

            using (cmd)
            {
                await cmd.Connection.OpenAsync();
                using (var rd = await cmd.ExecuteReaderAsync())
                {
                    while (await rd.ReadAsync())
                    {
                        rs.Add(selector(rd));
                    }
                }
                cmd.Connection.Close();
            }

            return rs;
        }

        public static async Task<int> SubmitCommand(
            this NpgsqlCommand cmd)
        {
            var a = 0;
            using (cmd)
            {
                await cmd.Connection.OpenAsync();
                a = await cmd.ExecuteNonQueryAsync();
                cmd.Connection.Close();
            }

            return a;
        }

        public static NpgsqlConnection ToConnection(
            this PgresUser u) =>
            new NpgsqlConnection(u.ToString());

        public static NpgsqlParameterCollection AddMany(
            this NpgsqlParameterCollection c,
            IEnumerable<(string n, object v)> ps
        ) => ps
            .Select(x => new NpgsqlParameter(x.n, x.v))
            .Aggregate(c, (acc, x) =>
            {
                acc.Add(x);
                return acc;
            });
    }
}
