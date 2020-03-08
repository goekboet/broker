using System;
using System.Data;
using Npgsql;
using scheduling;

namespace postgres
{
    public static class AddPublisherExtensions
    {
        private static string Sql { get; } = string.Join("\n", new[]
            {
                "insert into hosts (sub, handle, name)",
                "values(@sub, @handle, @name)",
                "on conflict (sub) do nothing",
                "returning handle, name"
            });

        public static NpgsqlCommand AddPublisher(
            this NpgsqlConnection c,
            Guid sub,
            string handle,
            string name
        )
        {
            var cmd = new NpgsqlCommand(Sql, c);
            cmd.Parameters.AddMany(
                new (string n, object v)[]
                {
                    ("sub", sub),
                    ("handle", handle),
                    ("name", name)
                });

            return cmd;
        }

        public static NewHost ToHost(IDataRecord r) => 
            new NewHost(
                handle: r.GetString(r.GetOrdinal("handle")),
                name: r.GetString(r.GetOrdinal("name"))
                );
    }

    public static class AddTimeExtensions
    {
        private static string Sql() => 
            string.Join("\n", new[]
            {
                "insert into times (start, \"end\", host, record, booked)",
                "select",
                "@start as start,",
                "@end as \"end\",",
                "h.handle as host,",
                "@record as record,",
                "null as booked",
                "from hosts as h",
                "where h.sub = @sub and h.handle = @handle",
                "on conflict (host, start) do nothing;"
            });

        public static NpgsqlCommand AddTime(
            this NpgsqlConnection c,
            Guid sub,
            PublishedTime t
        )
        {
            var cmd = new NpgsqlCommand(Sql(), c);
            cmd.Parameters.AddMany(
                new (string n, object v)[]
                {
                    ("handle", t.HostHandle),
                    ("sub", sub),
                    ("start", t.Start),
                    ("end", t.End),
                    ("record", t.Record)
                });

            return cmd;
        }
    }
}