using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using PublicCallers.Scheduling;

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
        public string Host { get; }
        public string Port { get; }
        public string Handle { get; }
        public string Pwd { get; }
        public string Db { get; }

        public override string ToString() =>
            $"Host={Host};Port={Port};Username={Handle};Password={Pwd};Database={Db}";
    }

    public static class PGres
    {
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

        public static NpgsqlCommand SelectUnbookedTimesInWindow(
            this NpgsqlConnection c,
            Guid host,
            long start,
            long end)
        {
            var sql = string.Join("\n", new[]
            {
                "select h.handle as host, t.start, t.end, t.record from times t",
                "join hosts h on t.host = h.Id",
                "where t.host = @host",
                "and t.start between @start and @end",
                "and t.booked is null",
                "order by t.start"
            });
            var cmd = new NpgsqlCommand(sql, c);
            cmd.Parameters.AddMany(
                new (string n, object v)[]
                {
                    ("host", host),
                    ("start", start),
                    ("end", end)
                });

            return cmd;
        }

        public static Meet ToMeet(IDataRecord r) => new Meet(
            r.GetInt64(r.GetOrdinal("start")),
            r.GetString(r.GetOrdinal("record")),
            r.GetString(r.GetOrdinal("host")),
            (int)(r.GetInt64(r.GetOrdinal("end")) - r.GetInt64(r.GetOrdinal("start")))
        );


        public static async Task<IEnumerable<S>> GetResults<S>(
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
            }

            return rs;
        }
    }
}
