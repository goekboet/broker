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

    public interface IHostsRepository
    {
        // GET    hosts/{id}/times?from={from}&to={to}
        Task<IEnumerable<Time>> GetTimes(
            Guid host,
            long start,
            long end
        );

        Task AddTime(Time t);

        // GET hosts
        Task<IEnumerable<Host>> GetHosts();

        Task AddHost(Host h);

        Task<IEnumerable<Host>> GetHost(Guid sub);
        // GET    bookings
        Task<IEnumerable<Time>> GetBookedTimes(
            Guid guest
        );

        // POST   bookings <- sub via jwt
        Task<Result<int>> Book(
            Guid g,
            Guid h,
            long s
        );
        
        // DELETE bookings <- sub via jwt (host, start) via body
        Task<int> UnBook(
            Guid g,
            long s
        );
    }
}