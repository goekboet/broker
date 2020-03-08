using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace scheduling
{
    public class PublishedTime
    {
        public PublishedTime(
            long start,
            long end,
            string hostHandle,
            string record,
            Guid? booked = null
        )
        {
            Start = start;
            End = end;
            HostHandle = hostHandle;
            Record = record;
            Booked = booked;
        }

        public long Start { get; }
        public long End { get; }
        public string HostHandle {get;}
        public string Record { get; }
        public Guid? Booked;
    }

    public class BookedTime
    {
        public BookedTime(
            long start,
            long end,
            string record,
            string booker
        )
        {
            Start = start;
            End = end;
            Record = record;
            Booker = booker;
        }

        public long Start { get; }
        public long End { get; }
        public string Record { get; }
        public string Booker {get;}
    }

    public class NewHost
    {
        public NewHost(
            string handle,
            string name)
        {
            Name = name;
            Handle = handle;
        }

        public string Name { get; }
        public string Handle {get;}
    }

    public class Conflict
    {
        public Conflict(
            string handle,
            long start,
            long end)
        {
            Handle = handle;
            Start = start;
            End = end;
        }

        public string Handle { get; }
        public long Start {get;}
        public long End {get;}
    }

    public abstract class AddHostResult<T>
    {
    }

    public sealed class Accepted<T> : AddHostResult<T>
    {
        public Accepted(T v)
        {
            Value = v;
        }

        public T Value {get;}
    }

    public sealed class Conflict<T> : AddHostResult<T>
    {
        public static Conflict<T> Instance {get;} = new Conflict<T>();
    }

    public interface IPublisherRepository
    {
        // GET    hosts/{id}/times?from={from}&to={to}
        Task AddTime(Guid sub, PublishedTime t);
        Task<IEnumerable<PublishedTime>> ListPublishedTimes(
            Guid sub,
            string handle,
            long from,
            long to);

        Task<PublishedTime> GetTime(Guid sub, string handle, long start);

        Task<AddHostResult<NewHost>> AddPublisher(Guid sub, NewHost h);

        Task<IEnumerable<NewHost>> GetPublisher(Guid sub);
        Task<IEnumerable<BookedTime>> GetBookedTimes(
            Guid sub,
            string handle,
            long from,
            long to);
    }
}