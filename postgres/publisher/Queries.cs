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
                "select handle, name from hosts",
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

        public static NewHost ToHost(IDataRecord r) => 
            new NewHost(
                handle: r.GetString(r.GetOrdinal("handle")),
                name: r.GetString(r.GetOrdinal("name"))
                );
    }

    public static class ListPublisherTimesExtensions
    {
        private static string Sql { get; } = string.Join("\n", new[]
            {
                "select t.start, t.host, t.end, t.record, t.booked from times as t",
                "join hosts as h on h.handle = t.host",
                "where h.sub = @sub",
                "and t.host = @handle",
                "and t.start between @from and @to",
                "order by start asc"
            });

        public static NpgsqlCommand ListPublisherTimes(
            this NpgsqlConnection c,
            Guid sub,
            string handle,
            long from,
            long to
        )
        {
            var cmd = new NpgsqlCommand(Sql, c);
            cmd.Parameters.AddMany(
                new (string n, object v)[]
                {
                    ("sub", sub),
                    ("handle", handle),
                    ("from", from),
                    ("to", to)
                });

            return cmd;
        }

        public static PublishedTime ToPublishedTime(IDataRecord r) => 
            new PublishedTime(
                start: r.GetInt64(r.GetOrdinal("start")),
                end: r.GetInt64(r.GetOrdinal("end")),
                hostHandle: r.GetString(r.GetOrdinal("host")),
                record: r.GetString(r.GetOrdinal("record")),
                booked: r.IsDBNull(r.GetOrdinal("booked"))
                    ? (Guid?) null
                    : r.GetGuid(r.GetOrdinal("booked"))
                );
    }

    public static class GetPublishedTimesExtensions
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
                hostHandle: r.GetString(r.GetOrdinal("host")),
                record: r.GetString(r.GetOrdinal("record")),
                booked: r.IsDBNull(r.GetOrdinal("booked"))
                    ? (Guid?) null
                    : r.GetGuid(r.GetOrdinal("booked"))
                );
    }

    public static class GetPublishedTimeExtensions
    {
        private static string Sql { get; } = string.Join("\n", new[]
            {
                "select t.host, t.start, t.end, t.record, t.booked from times as t",
                "join hosts as h on t.host = h.handle",
                "where h.sub = @sub",
                "and t.host = @handle",
                "and t.start = @start"
            });

        public static NpgsqlCommand GetPublishedTime(
            this NpgsqlConnection c,
            Guid sub,
            string handle,
            long start
        )
        {
            var cmd = new NpgsqlCommand(Sql, c);
            cmd.Parameters.AddMany(
                new (string n, object v)[]
                {
                    ("sub", sub),
                    ("handle", handle),
                    ("start", start)
                });

            return cmd;
        }

        public static PublishedTime ToPublishedTime(IDataRecord r) => 
            new PublishedTime(
                start: r.GetInt64(r.GetOrdinal("start")),
                end: r.GetInt64(r.GetOrdinal("end")),
                hostHandle: r.GetString(r.GetOrdinal("host")),
                record: r.GetString(r.GetOrdinal("record")),
                booked: r.IsDBNull(r.GetOrdinal("booked"))
                    ? (Guid?) null
                    : r.GetGuid(r.GetOrdinal("booked"))
                );
    }

    public static class GetBookedTimesExtensions
    {
        private static string Sql { get; } = string.Join("\n", new[]
            {
                "select t.start, t.end, t.record, t.booked from times as t",
                "join hosts as h on h.handle = t.host",
                "where h.sub = @sub",
                "and t.host = @handle",
                "and t.booked is not null",
                "and t.start between @from and @to",
                "order by start asc"
            });

        public static NpgsqlCommand GetBookedTimes(
            this NpgsqlConnection c,
            Guid sub,
            string handle,
            long from,
            long to
        )
        {
            var cmd = new NpgsqlCommand(Sql, c);
            cmd.Parameters.AddMany(
                new (string n, object v)[]
                {
                    ("sub", sub),
                    ("handle", handle),
                    ("from", from),
                    ("to", to)
                });

            return cmd;
        }

        public static BookedTime ToBookedTime(IDataRecord r) => 
            new BookedTime(
                start: r.GetInt64(r.GetOrdinal("start")),
                end: r.GetInt64(r.GetOrdinal("end")),
                record: r.GetString(r.GetOrdinal("record")),
                booker: r.GetGuid(r.GetOrdinal("booked")).ToString()
                );
    }
}