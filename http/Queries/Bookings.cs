using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using PublicCallers.Scheduling;

namespace http.Queries
{
    public sealed class GetMyBookings : IQuery<Time>
    {
        public GetMyBookings(
            Guid sub)
        {
            Parameters = new (string n, object v)[]
                {
                    ("sub", sub),
                };
        }

        public string Sql { get; } = string.Join("\n", new[]
        {
            $"select t.host, t.start, t.end, t.record from times t",
            $"where t.booked = @sub",
            $"order by t.start"
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

    public sealed class GetBooking : IQuery<Time>
    {
        public GetBooking(
            Guid sub,
            long start)
        {
            Parameters = new (string n, object v)[]
                {
                    ("sub", sub),
                    ("start", start)
                };
        }

        public string Sql { get; } = string.Join("\n", new[]
            {
                $"select t.host, t.start, t.end, t.record from times as t",
                $"where t.booked = @sub",
                $"and start = @start"
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
}