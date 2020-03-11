using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using PublicCallers.Scheduling;
using scheduling;

namespace http.Bookings
{
    public class GetTime : IQuery<Time>
    {
        public GetTime(
            Guid sub,
            string host,
            long start
        )
        {
            Parameters = new (string n, object v)[]
                {
                    ("sub", sub),
                    ("host", host),
                    ("start", start),
                };
        }

        public string Sql { get; } = string.Join("\n", new[]
            {
                "select t.host, t.start, t.host, t.end, t.record from times t",
                "where t.host = @host",
                "and t.start = @start",
                "and (t.booked is null",
                "or t.booked = @sub)"
            });

        public (string n, object v)[] Parameters {get;}

        Time Parse(IDataRecord r) => new Time(
            r.GetInt64(r.GetOrdinal("start")),
            r.GetString(r.GetOrdinal("record")),
            r.GetString(r.GetOrdinal("host")),
            r.GetInt64(r.GetOrdinal("end")));

        public async Task<IEnumerable<Time>> Read(DbDataReader r)
        {
            var l = new List<Time>();
            while (await r.ReadAsync())
            {
                l.Add(Parse(r));
            }
            
            return l;
        }
    }

    public class GetBookingConflict : IQuery<Conflict>
    {
        public GetBookingConflict(
            Guid sub,
            string host,
            long start,
            long end
        )
        {
            Parameters = new (string n, object v)[]
                {
                    ("sub", sub),
                    ("host", host),
                    ("start", start),
                    ("end", end)
                };
        }
        public string Sql { get; } = string.Join("\n", new[]
            {
                "select t.host, t.start, t.end from times as t",
                "where t.booked = @sub",
                "and (@start between t.start and t.\"end\"",
                "or @end between t.start and t.\"end\")"
            });

        public (string n, object v)[] Parameters {get;}

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

    public class BookCommand : IQuery<Time>
    {
        public BookCommand(
            Guid sub,
            string host,
            long start
        )
        {
            Parameters = new (string n, object v)[]
            {
                ("sub", sub),
                ("host", host),
                ("start", start)
            };
        }
        public string Sql { get; } = string.Join("\n", new[]
            {
                "update times",
                "set booked = @sub",
                "where host = @host",
                "and start = @start",
                "returning start, record, host, \"end\""
            });

        public (string n, object v)[] Parameters {get;}

        Time Parse(IDataRecord r) => new Time(
            r.GetInt64(r.GetOrdinal("start")),
            r.GetString(r.GetOrdinal("record")),
            r.GetString(r.GetOrdinal("host")),
            r.GetInt64(r.GetOrdinal("end"))
        );

        public async Task<IEnumerable<Time>> Read(DbDataReader r)
        {
            var l = new List<Time>();
            while (await r.ReadAsync())
            {
                l.Add(Parse(r));
            }
            
            return l;
        }
    }

    public class UnBookCommand : ICommand
    {
        public UnBookCommand(
            Guid sub,
            long start
        )
        {
            Parameters = new (string n, object v)[]
                {
                    ("sub", sub),
                    ("start", start)
                };
        }
        public string Sql { get; } = string.Join("\n", new[]
            {
                $"update times set booked = null",
                $"where start = @start",
                $"and booked = @sub"
            });

        public (string n, object v)[] Parameters { get; }
    }
}