using System;
using System.Data;
using Npgsql;
using PublicCallers.Scheduling;
using scheduling;

namespace postgres
{
    public static class GetPublisherExtensions
    {
        private static string Sql { get; } = string.Join("\n", new[]
            {
                "select handle from hosts",
                "where sub = @sub"
            });

        public static NpgsqlCommand GetPublisher(
            this NpgsqlConnection c,
            Guid sub
        )
        {
            var cmd = new NpgsqlCommand(Sql, c);
            cmd.Parameters.AddMany(
                new (string n, object v)[]
                {
                    ("sub", sub),
                });

            return cmd;
        }

        public static Publisher ToHost(IDataRecord r) => 
            new Publisher(
                name: r.GetString(r.GetOrdinal("handle"))
                );
    }

    public static class ListPublisherTimesExtensions
    {
        private static string Sql { get; } = string.Join("\n", new[]
            {
                "select t.start, t.end, t.record, t.booked from times as t",
                "where t.host = @sub",
                "and t.start between @from and @to",
                "order by start asc"
            });

        public static NpgsqlCommand ListPublisherTimes(
            this NpgsqlConnection c,
            Guid sub,
            long from,
            long to
        )
        {
            var cmd = new NpgsqlCommand(Sql, c);
            cmd.Parameters.AddMany(
                new (string n, object v)[]
                {
                    ("sub", sub),
                    ("from", from),
                    ("to", to)
                });

            return cmd;
        }

        public static PublishedTime ToPublishedTime(IDataRecord r) => 
            new PublishedTime(
                start: r.GetInt64(r.GetOrdinal("start")),
                end: r.GetInt64(r.GetOrdinal("end")),
                record: r.GetString(r.GetOrdinal("record")),
                booked: r.IsDBNull(r.GetOrdinal("booked"))
                    ? (Guid?) null
                    : r.GetGuid(r.GetOrdinal("booked"))
                );
    }

    public static class GetPublisherTimesExtensions
    {
        private static string Sql { get; } = string.Join("\n", new[]
            {
                "select t.start, t.end, t.record, t.booked from times as t",
                "where t.host = @sub",
                "and t.start = @start",
                "order by start asc"
            });

        public static NpgsqlCommand GetPublisherTimes(
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

        public static PublishedTime ToPublishedTime(IDataRecord r) => 
            new PublishedTime(
                start: r.GetInt64(r.GetOrdinal("start")),
                end: r.GetInt64(r.GetOrdinal("end")),
                record: r.GetString(r.GetOrdinal("record")),
                booked: r.IsDBNull(r.GetOrdinal("booked"))
                    ? (Guid?) null
                    : r.GetGuid(r.GetOrdinal("booked"))
                );
    }
}