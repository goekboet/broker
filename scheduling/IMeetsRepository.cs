using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PublicCallers.Scheduling
{
    public interface IMeetsRepository
    {
        // GET    hosts/{id}/times?from={from}&to={to}
        Task<IEnumerable<Meet>> GetMeets(
            Guid host,
            long start,
            long end
        );
        Task<IEnumerable<Host>> GetHosts();

        // GET    bookings
        Task<IEnumerable<Booking>> GetBookings(
            Guid guest
        );

        // POST   bookings <- sub via jwt
        Task<int> Book(
            Guid g,
            Guid h,
            long s
        );
        
        // DELETE bookings <- sub via jet (host, start) via body
        Task<int> UnBook(
            Guid g,
            Guid h,
            long s
        );
    }
}