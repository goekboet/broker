using System.Data;
using Npgsql;
using PublicCallers.Scheduling;

namespace postgres
{
    public static class ListHostExtensions
    {
        private static string Sql { get; } = string.Join("\n", new[]
            {
                "select handle, name from hosts",
                "where name > @notBeforeName",
                "order by name asc, handle asc",
                "limit @pageSize offset @offset"
            });

        public static NpgsqlCommand ListHosts(
            this NpgsqlConnection c,
            int offset,
            int pageSize,
            string notBeforeName = "")
        {
            var cmd = new NpgsqlCommand(Sql, c);
            cmd.Parameters.AddMany(
                new (string n, object v)[]
                {
                    ("notBeforeName", notBeforeName ?? string.Empty),
                    ("offset", offset),
                    ("pageSize", pageSize)
                });

            return cmd;
        }
        public static HostListing ToHost(IDataRecord r) => new HostListing(
            r.GetString(r.GetOrdinal("handle")),
            r.GetString(r.GetOrdinal("name"))
            );
    }

    public static class ListTimesExtensions
    {
        private const string p_host = "host";
        private const string p_start = "start";
        private const string p_end = "end";
        private static string Sql { get; } = string.Join("\n", new[]
            {
                $"select t.host, t.start, t.end, t.record from times t",
                $"where t.host = @{p_host}",
                $"and t.start between @{p_start} and @{p_end}",
                $"and t.booked is null",
                $"order by t.start"
            });

        public static Time ToMeet(IDataRecord r) => new Time(
            r.GetInt64(r.GetOrdinal("start")),
            r.GetString(r.GetOrdinal("record")),
            r.GetString(r.GetOrdinal("host")),
            r.GetInt64(r.GetOrdinal("end"))
        );

        public static NpgsqlCommand ListTimes(
            this NpgsqlConnection c,
            string host,
            long start,
            long end)
        {
            var cmd = new NpgsqlCommand(Sql, c);
            cmd.Parameters.AddMany(
                new (string n, object v)[]
                {
                    (p_host, host),
                    (p_start, start),
                    (p_end, end)
                });

            return cmd;
        }
    }
}