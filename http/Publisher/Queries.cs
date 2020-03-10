using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using PublicCallers.Scheduling;
using scheduling;

namespace http.Publisher
{
    public class ListPublishers : IQuery<NewHost>
    {
        public ListPublishers(Guid sub)
        {
            Parameters = new (string n, object v)[]
                {
                    ("sub", sub),
                };
        }
        public string Sql { get; } = string.Join("\n", new[]
            {
                "select handle, name from hosts",
                "where sub = @sub"
            });

        public (string n, object v)[] Parameters { get; }

        NewHost Parse(IDataRecord r) =>
            new NewHost(
                handle: r.GetString(r.GetOrdinal("handle")),
                name: r.GetString(r.GetOrdinal("name"))
                );

        public async Task<IEnumerable<NewHost>> Read(
            DbDataReader r)
        {
            var l = new List<NewHost>();
            while (await r.ReadAsync())
            {
                l.Add(Parse(r));
            }

            return l;
        }
    }

    public class ListPublishedTimes : IQuery<PublishedTime>
    {
        public ListPublishedTimes(
            Guid sub,
            string handle,
            long from,
            long to)
        {
            Parameters = new (string n, object v)[]
                {
                    ("sub", sub),
                    ("handle", handle),
                    ("from", from),
                    ("to", to)
                };
        }

        public string Sql { get; } = string.Join("\n", new[]
            {
                "select t.start, t.host, t.end, t.record, t.booked from times as t",
                "join hosts as h on h.handle = t.host",
                "where h.sub = @sub",
                "and t.host = @handle",
                "and t.start between @from and @to",
                "order by start asc"
            });

        public (string n, object v)[] Parameters { get; }

        PublishedTime Parse(IDataRecord r) =>
            new PublishedTime(
                start: r.GetInt64(r.GetOrdinal("start")),
                end: r.GetInt64(r.GetOrdinal("end")),
                hostHandle: r.GetString(r.GetOrdinal("host")),
                record: r.GetString(r.GetOrdinal("record")),
                booked: r.IsDBNull(r.GetOrdinal("booked"))
                    ? (Guid?)null
                    : r.GetGuid(r.GetOrdinal("booked"))
                );

        public async Task<IEnumerable<PublishedTime>> Read(DbDataReader r)
        {
            var l = new List<PublishedTime>();
            while (await r.ReadAsync())
            {
                l.Add(Parse(r));
            }

            return l;
        }
    }

    public class GetPublishedTime : IQuery<PublishedTime>
    {
        public GetPublishedTime(
            Guid sub,
            string host,
            long start
        )
        {
            Parameters = new (string n, object v)[]
            {
                    ("sub", sub),
                    ("start", start),
                    ("host", host)
            };
        }
        public string Sql { get; } = string.Join("\n", new[]
        {
                "select t.start, t.end, t.host, t.record, t.booked from times as t",
                "join hosts as h on t.host = h.handle",
                "where h.sub = @sub",
                "and t.host = @host",
                "and t.start = @start",
                "order by start asc"
            });

        public (string n, object v)[] Parameters { get; }

        PublishedTime Parse(IDataRecord r) =>
        new PublishedTime(
            start: r.GetInt64(r.GetOrdinal("start")),
            end: r.GetInt64(r.GetOrdinal("end")),
            hostHandle: r.GetString(r.GetOrdinal("host")),
            record: r.GetString(r.GetOrdinal("record")),
            booked: r.IsDBNull(r.GetOrdinal("booked"))
                ? (Guid?)null
                : r.GetGuid(r.GetOrdinal("booked"))
            );

        public async Task<IEnumerable<PublishedTime>> Read(DbDataReader r)
        {
            var l = new List<PublishedTime>();
            while (await r.ReadAsync())
            {
                l.Add(Parse(r));
            }

            return l;
        }
    }

    public class GetBookedPublishedTimes : IQuery<BookedTime>
    {
        public GetBookedPublishedTimes(
            Guid sub,
            string handle,
            long from,
            long to
        )
        {
            Parameters = new (string n, object v)[]
                {
                    ("sub", sub),
                    ("handle", handle),
                    ("from", from),
                    ("to", to)
                };
        }

        public string Sql { get; } = string.Join("\n", new[]
            {
                "select t.start, t.end, t.record, t.booked from times as t",
                "join hosts as h on h.handle = t.host",
                "where h.sub = @sub",
                "and t.host = @handle",
                "and t.booked is not null",
                "and t.start between @from and @to",
                "order by start asc"
            });

        public (string n, object v)[] Parameters { get; }

        BookedTime Parse(IDataRecord r) => 
            new BookedTime(
                start: r.GetInt64(r.GetOrdinal("start")),
                end: r.GetInt64(r.GetOrdinal("end")),
                record: r.GetString(r.GetOrdinal("record")),
                booker: r.GetGuid(r.GetOrdinal("booked")).ToString()
                );

        public async Task<IEnumerable<BookedTime>> Read(DbDataReader r)
        {
            var l = new List<BookedTime>();
            while (await r.ReadAsync())
            {
                l.Add(Parse(r));
            }

            return l;
        }
    }
}
