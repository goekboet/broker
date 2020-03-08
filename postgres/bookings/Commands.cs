using System;
using System.Data;
using Npgsql;
using scheduling;

namespace postgres
{
    public static class GetBookingConflictExtensions
    {
        private static string Sql { get; } = string.Join("\n", new[]
            {
                "select t.host, t.start, t.end from times as t",
                "where t.booked = @sub",
                "and @start between t.start and t.\"end\"",
                "or @end between t.start and t.\"end\""
            });

        public static NpgsqlCommand GetBookingConflict(
            this NpgsqlConnection c,
            Guid sub,
            long start,
            long end)
        {
            var cmd = new NpgsqlCommand(Sql, c);
            cmd.Parameters.AddMany(
                new (string n, object v)[]
                {
                    ("sub", sub),
                    ("start", start),
                    ("end", end)
                });

            return cmd;
        }

        public static Conflict ToHost(IDataRecord r) =>
            new Conflict(
                handle: r.GetString(r.GetOrdinal("handle")),
                start: r.GetInt64(r.GetOrdinal("start")),
                end: r.GetInt64(r.GetOrdinal("end"))
                );
    }



    public static class BookExtensions
    {
        private static string Sql { get; } = string.Join("\n", new[]
            {
                $"update times set booked = @sub",
                $"where host = @host",
                $"and start = @start",
                $"and booked is null or booked = @sub"
            });


        public static NpgsqlCommand Book(
            this NpgsqlConnection c,
            Guid sub,
            string host,
            long start
        )
        {
            var cmd = new NpgsqlCommand(Sql, c);
            cmd.Parameters.AddMany(
                new (string n, object v)[]
                {
                    ("sub", sub),
                    ("host", host),
                    ("start", start)
                });

            return cmd;
        }
    }



    public static class UnBookExtensions
    {
        const string guest = "guest";
        const string start = "start";

        private static string Sql { get; } = string.Join("\n", new[]
            {
                $"update times set booked = null",
                $"where start = @{start}",
                $"and booked = @{guest}"
            });
        public static NpgsqlCommand UnBook(
            this NpgsqlConnection c,
            Guid g,
            long s
        )
        {
            var cmd = new NpgsqlCommand(Sql, c);
            cmd.Parameters.AddMany(
                new (string n, object v)[]
                {
                    (guest, g),
                    (start, s)
                });

            return cmd;
        }
    }
}