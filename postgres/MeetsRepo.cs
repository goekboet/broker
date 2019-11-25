using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Npgsql;
using PublicCallers.Scheduling;

namespace postgres
{
    public class PostGresRepo : IHostsRepository
    {
        private PgresUser _u;

        public PostGresRepo(
            PgresUser u)
        {
            _u = u;
        }

        public async Task<Result<int>> Book(Guid g, Guid h, long s)
        {
            try 
            {
                var r = await _u
                    .ToConnection()
                    .Book(g, h, s)
                    .SubmitCommand();

                return new OK<int>(r);
            }
            catch (PostgresException e) when (e.ConstraintName == "one_booking_per_start")
            {
                return new Err<int>(e);
            }
        }

        public Task<IEnumerable<Time>> GetBookedTimes(Guid guest) => _u
            .ToConnection()
            .GetBookings(guest)
            .SubmitQuery(ListBookingsExtensions.ToBooking);

        public Task<IEnumerable<Host>> GetHosts() => _u
            .ToConnection()
            .ListHosts()
            .SubmitQuery(ListHostExtensions.ToHost);

        public Task<IEnumerable<Time>> GetTimes(
            Guid host, 
            long start, 
            long end) => _u
            .ToConnection()
            .ListTimes(host, start, end)
            .SubmitQuery(ListTimesExtensions.ToMeet);

        public Task<int> UnBook(Guid g, long s) => _u
            .ToConnection()
            .UnBook(g, s)
            .SubmitCommand();
    }

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

    public static class ListHostExtensions
    {
        private static string Sql { get; } = string.Join("\n", new[]
            {
                "select id, handle as name, tz from hosts",
                "order by handle",
                "limit 100"
            });

        public static NpgsqlCommand ListHosts(
            this NpgsqlConnection c) => new NpgsqlCommand(Sql, c);
        public static Host ToHost(IDataRecord r) => new Host(
            r.GetGuid(r.GetOrdinal("id")),
            r.GetString(r.GetOrdinal("name")),
            r.GetString(r.GetOrdinal("tz"))
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
}