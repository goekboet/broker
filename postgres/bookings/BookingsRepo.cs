using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using postgres;
using PublicCallers.Scheduling;

public class BookingsRepo : IBookingsRepository
{
    private PgresUser _u;

    public BookingsRepo(
        PgresUser u)
    {
        _u = u;
    }

    public async Task<Result<int>> Book(
        Guid g, 
        Guid h, 
        long s)
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

        

        public Task<int> UnBook(Guid g, long s) => _u
            .ToConnection()
            .UnBook(g, s)
            .SubmitCommand();
}