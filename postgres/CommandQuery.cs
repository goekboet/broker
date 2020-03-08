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

    

    

    public static class BookExtensions
    {
        private static string Sql { get;} = string.Join("\n", new[]
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
            r.GetString(r.GetOrdinal("host")),
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
            r.GetString(r.GetOrdinal("host")),
            r.GetInt64(r.GetOrdinal("end"))
        );
    }
}