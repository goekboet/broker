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

        // GET    hosts/{id}/times?from={from}&to={to}
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
            r.GetInt64(r.GetOrdinal("end"))
        );

        public static NpgsqlCommand ListHosts(
            this NpgsqlConnection c)
        {
            var sql = string.Join("\n", new[]
            {
                "select id, handle as name from hosts",
                "order by handle",
                "limit 100"
            });

            return new NpgsqlCommand(sql, c);
        }

        public static Host ToHost(IDataRecord r) => new Host(
            r.GetGuid(r.GetOrdinal("id")),
            r.GetString(r.GetOrdinal("name"))
        );

        // GET    bookings
        public static NpgsqlCommand GetBookings(
            this NpgsqlConnection c,
            Guid g
        )
        {
            const string guest = "guest";

            var sql = string.Join("\n", new[]
            {
                $"select h.id as hostId, h.handle as host, t.start, t.end from times t",
                $"join hosts h on t.host = h.Id",
                $"where t.booked = @{guest}",
                $"order by t.start",
                $"limit 100;"
            });

            var cmd = new NpgsqlCommand(sql, c);
            cmd.Parameters.AddMany(
                new (string n, object v)[]
                {
                    (guest, g),
                });

            return cmd;
        }

        public static Booking ToBooking(IDataRecord r) => new Booking(
            r.GetGuid(r.GetOrdinal("hostId")),
            r.GetString(r.GetOrdinal("host")),
            r.GetInt64(r.GetOrdinal("host")),
            r.GetInt64(r.GetOrdinal("end"))
        );

        // POST   bookings <- sub via jwt
        public static NpgsqlCommand Book(
            this NpgsqlConnection c,
            Guid g,
            Guid h,
            long s
        )
        {
            const string host = "host";
            const string guest = "guest";
            const string start = "start";

            var sql = string.Join("\n", new[]
            {
                $"update times set booked = @{guest}",
                $"where host = @{host}",
                $"and start = @{start}"
            });

            var cmd = new NpgsqlCommand(sql, c);
            cmd.Parameters.AddMany(
                new (string n, object v)[]
                {
                    (guest, g),
                    (host, s),
                    (start, s)
                });

            return cmd;
        }
        // DELETE bookings <- sub via jet (host, start) via body
        public static NpgsqlCommand UnBook(
            this NpgsqlConnection c,
            Guid g,
            Guid h,
            long s
        )
        {
            const string host = "host";
            const string guest = "guest";
            const string start = "start";

            var sql = string.Join("\n", new[]
            {
                $"update times set booked = null",
                $"where host = @{host}",
                $"and start = @{start}",
                $"and booked = @{guest}"
            });

            var cmd = new NpgsqlCommand(sql, c);
            cmd.Parameters.AddMany(
                new (string n, object v)[]
                {
                    (guest, g),
                    (host, s),
                    (start, s)
                });

            return cmd;
        }
    }
}
