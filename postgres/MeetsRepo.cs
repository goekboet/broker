using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using PublicCallers.Scheduling;

namespace postgres
{
    public class PostGresRepo : IMeetsRepository
    {
        private PgresUser _u;

        public PostGresRepo(
            PgresUser u)
        {
            _u = u;
        }

        public Task<int> Book(Guid g, Guid h, long s) => _u
            .ToConnection()
            .Book(g, h, s)
            .SubmitCommand();

        public Task<IEnumerable<Booking>> GetBookings(Guid guest) => _u
            .ToConnection()
            .GetBookings(guest)
            .SubmitQuery(PGres.ToBooking);

        public Task<IEnumerable<Host>> GetHosts() => _u
            .ToConnection()
            .ListHosts()
            .SubmitQuery(PGres.ToHost);

        public Task<IEnumerable<Meet>> GetMeets(
            Guid host, 
            long start, 
            long end) => _u
            .ToConnection()
            .SelectUnbookedTimesInWindow(host, start, end)
            .SubmitQuery(PGres.ToMeet);

        public Task<int> UnBook(Guid g, Guid h, long s) => _u
            .ToConnection()
            .UnBook(g, h, s)
            .SubmitCommand();
    }
}