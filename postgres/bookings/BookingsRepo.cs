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
        Guid sub,
        string host,
        long start)
    {
        using (var conn = _u.ToConnection())
        {
            try
            {
                var r = await _u
                .ToConnection()
                .Book(sub, host, start)
                .SubmitCommand();

                return new OK<int>(r);
            }
            catch (PostgresException e) when (e.ConstraintName == "one_booking_per_start")
            {
                return new Err<int>(e);
            }
        }
    }

    public async Task<int> UnBook(Guid g, long s)
    {
        using (var conn = _u.ToConnection())
        {
            return await conn
                .UnBook(g, s)
                .SubmitCommand();
        }
    }
}