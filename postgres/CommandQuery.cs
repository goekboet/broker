using System;
using System.Data;
using Npgsql;
using PublicCallers.Scheduling;
using scheduling;

namespace postgres
{
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
            r.GetGuid(r.GetOrdinal("host")),
            r.GetInt64(r.GetOrdinal("end"))
        );

        public static NpgsqlCommand ListTimes(
            this NpgsqlConnection c,
            Guid host,
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

    public static class AddTimeExtensions
    {
        private static string Sql { get; } = string.Join("\n", new[]
            {
                "insert into times (host, start, \"end\", record, booked)",
                "values(@host, @start, @end, @record, null)",
                "on conflict (host, start) do nothing"
            });

        public static NpgsqlCommand AddTime(
            this NpgsqlConnection c,
            Guid sub,
            PublishedTime t
        )
        {
            var cmd = new NpgsqlCommand(Sql, c);
            cmd.Parameters.AddMany(
                new (string n, object v)[]
                {
                    ("host", sub),
                    ("start", t.Start),
                    ("end", t.End),
                    ("record", t.Record)
                });

            return cmd;
        }
    }

    public static class ListHostExtensions
    {
        private static string Sql { get; } = string.Join("\n", new[]
            {
                "select sub, handle as name from hosts",
                "order by handle",
                "limit 100"
            });

        public static NpgsqlCommand ListHosts(
            this NpgsqlConnection c) => new NpgsqlCommand(Sql, c);
        public static Host ToHost(IDataRecord r) => new Host(
            r.GetGuid(r.GetOrdinal("sub")),
            r.GetString(r.GetOrdinal("name"))
            );
    }

    

    

    public static class BookExtensions
    {
        const string host = "host";
        const string guest = "guest";
        const string start = "start";

        private static string Sql { get;} = string.Join("\n", new[]
            {
                $"update times set booked = @{guest}",
                $"where host = @{host}",
                $"and start = @{start}",
                $"and booked is null or booked = @{guest}"
            });

        
        public static NpgsqlCommand Book(
            this NpgsqlConnection c,
            Guid g,
            Guid h,
            long s
        )
        {
            var cmd = new NpgsqlCommand(Sql, c);
            cmd.Parameters.AddMany(
                new (string n, object v)[]
                {
                    (guest, g),
                    (host, h),
                    (start, s)
                });

            return cmd;
        }
    }

    public static class ListBookingsExtensions
    {
        const string guest = "guest";

        private static string Sql {get;} = string.Join("\n", new []
        {
            $"select t.host, t.start, t.end, t.record from times t",
            $"where t.booked = @{guest}",
            $"order by t.start"
        });

        public static NpgsqlCommand GetBookings(
            this NpgsqlConnection c,
            Guid g
        )
        {
            var cmd = new NpgsqlCommand(Sql, c);
            cmd.Parameters.AddMany(
                new (string n, object v)[]
                {
                    (guest, g),
                });

            return cmd;
        }

        public static Time ToBooking(IDataRecord r) => new Time(
            r.GetInt64(r.GetOrdinal("start")),
            r.GetString(r.GetOrdinal("record")),
            r.GetGuid(r.GetOrdinal("host")),
            r.GetInt64(r.GetOrdinal("end"))
        );
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

    public static class GetBookingQuery
    {
        private static string Sql { get; } = string.Join("\n", new[]
            {
                $"select t.host, t.start, t.end, t.record from times as t",
                $"where t.booked = @sub",
                $"and start = @start"
            });
        public static NpgsqlCommand GetBooking(
            this NpgsqlConnection c,
            Guid sub,
            long start
        )
        {
            var cmd = new NpgsqlCommand(Sql, c);
            cmd.Parameters.AddMany(
                new (string n, object v)[]
                {
                    ("sub", sub),
                    ("start", start)
                });

            return cmd;
        }

        public static Time ToBooking(IDataRecord r) => new Time(
            r.GetInt64(r.GetOrdinal("start")),
            r.GetString(r.GetOrdinal("record")),
            r.GetGuid(r.GetOrdinal("host")),
            r.GetInt64(r.GetOrdinal("end"))
        );
    }
}