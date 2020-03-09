using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PublicCallers.Scheduling
{
    public abstract class Result<T> {}
    public class OK<T> : Result<T>
    {
        public OK(T r)
        {
            Result = r;
        }

        public T Result { get; }
    }

    public class Err<T> : Result<T>
    {
        public Err(Exception e)
        {
            Exception = e;
        }
        public Exception Exception { get; }
    }

    public interface IBookingsRepository
    {
        // POST   bookings <- sub via jwt
        Task<Result<int>> Book(
            Guid sub,
            string handle,
            long start
        );
        
        // DELETE bookings <- sub via jwt (host, start) via body
        Task<int> UnBook(
            Guid g,
            long s
        );
    }
}