using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using PublicCallers.Scheduling;

namespace http.Public
{
    public sealed class HostListingPagedByHandle : IQuery<HostListing>
    {
        public HostListingPagedByHandle(
            string notBeforeName,
            int pageSize,
            int offset)
        {
            Parameters = new (string n, object v)[]
                {
                    ("notBeforeName", notBeforeName ?? string.Empty),
                    ("offset", offset),
                    ("pageSize", pageSize)
                };
        }

        public string Sql { get; } = string.Join("\n", new[]
            {
                "select handle, name from hosts",
                "where name > @notBeforeName",
                "order by name asc, handle asc",
                "limit @pageSize offset @offset"
            });

        public (string n, object v)[] Parameters {get;}

        public HostListing Parse(IDataRecord r) => new HostListing(
            r.GetString(r.GetOrdinal("handle")),
            r.GetString(r.GetOrdinal("name"))
            );

        public async Task<IEnumerable<HostListing>> Read(
            DbDataReader r)
        {
            var l = new List<HostListing>();
            while (await r.ReadAsync())
            {
                l.Add(Parse(r));
            }
            
            return l;
        }
    }

    public sealed class TimesListingByStart : IQuery<Time>
    {
        public TimesListingByStart(
            string host,
            long start,
            long end)
        {
            Parameters = new (string n, object v)[]
                {
                    ("host", host),
                    ("start", start),
                    ("end", end)
                };
        }

        public string Sql { get; } = string.Join("\n", new[]
            {
                $"select t.host, t.start, t.end, t.record from times t",
                $"where t.host = @host",
                $"and t.start between @start and @end",
                $"and t.booked is null",
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
}