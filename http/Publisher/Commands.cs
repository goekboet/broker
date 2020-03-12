using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using PublicCallers.Scheduling;

namespace http.Publisher
{
    public class AddPublisher : ICommand
    {
        public AddPublisher(
            Guid sub,
            string handle,
            string name
        )
        {
            Parameters = new (string n, object v)[]
                {
                    ("sub", sub),
                    ("handle", handle),
                    ("name", name)
                };
        }
        public string Sql { get; } = string.Join("\n", new[]
            {
                "insert into hosts (sub, handle, name)",
                "values(@sub, @handle, @name)",
                "on conflict (sub,handle) do nothing"
            });

        public (string n, object v)[] Parameters { get; }
    }

    public class GetHandle : IQuery<NewHost>
    {
        public GetHandle(
            Guid sub,
            string handle
        )
        {
            Parameters = new (string n, object v)[]
                {
                    ("sub", sub),
                    ("handle", handle)
                };
        }
        public string Sql { get; } = string.Join("\n", new[]
            {
                "select handle, name from hosts",
                "where sub = @sub",
                "and handle = @handle"
            });

        public (string n, object v)[] Parameters { get; }

        NewHost Parse(IDataRecord r) =>
            new NewHost(
                handle: r.GetString(r.GetOrdinal("handle")),
                name: r.GetString(r.GetOrdinal("name"))
                );

        public async Task<IEnumerable<NewHost>> Read(DbDataReader r)
        {
            var l = new List<NewHost>();
            while (await r.ReadAsync())
            {
                l.Add(Parse(r));
            }

            return l;
        }
    }

    public class PublishedTimeConflict : IQuery<Conflict>
    {
        public PublishedTimeConflict(
            Guid sub,
            long start,
            long end
        )
        {
            Parameters = new (string n, object v)[]
                {
                    ("sub", sub),
                    ("start", start),
                    ("end", end)
                };
        }

        public string Sql { get; } = string.Join("\n", new[]
            {
                "select t.host, t.start, t.end from times as t",
                "join hosts as h on t.host = h.handle",
                "where h.sub = @sub",
                "and (@start between t.start and t.\"end\"",
                "or @end between t.start and t.\"end\")"
            });

        public (string n, object v)[] Parameters { get; }

        Conflict Parse(IDataRecord r) =>
            new Conflict(
                handle: r.GetString(r.GetOrdinal("host")),
                start: r.GetInt64(r.GetOrdinal("start")),
                end: r.GetInt64(r.GetOrdinal("end"))
                );

        public async Task<IEnumerable<Conflict>> Read(DbDataReader r)
        {
            var l = new List<Conflict>();
            while (await r.ReadAsync())
            {
                l.Add(Parse(r));
            }

            return l;
        }
    }

    public class AddTime : ICommand
    {
        public AddTime(
            Guid sub,
            PublishedTime t
        )
        {
            Parameters = new (string n, object v)[]
                {
                    ("handle", t.HostHandle),
                    ("sub", sub),
                    ("start", t.Start),
                    ("end", t.End),
                    ("record", t.Record)
                };
        }
        public string Sql { get; } = string.Join("\n", new[]
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

        public (string n, object v)[] Parameters { get; }
    }
}