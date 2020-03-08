using System;
using System.Data;
using Npgsql;
using PublicCallers.Scheduling;

namespace postgres
{
    public static class ListBookingsExtensions
    {
        const string guest = "guest";

        private static string Sql { get; } = string.Join("\n", new[]
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