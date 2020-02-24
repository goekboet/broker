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
            string record,
            Guid? booked = null
        )
        {
            Start = start;
            End = end;
            Record = record;
            Booked = booked;
        }

        public long Start { get; }
        public long End { get; }
        public string Record { get; }
        public Guid? Booked;
    }

    public class Publisher
    {
        public Publisher(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public interface IPublisherRepository
    {
        // GET    hosts/{id}/times?from={from}&to={to}
        Task AddTime(Guid sub, PublishedTime t);
        Task<IEnumerable<PublishedTime>> ListPublishedTimes(
            Guid sub,
            long from,
            long to);

        Task<PublishedTime> GetTime(Guid sub, long start);

        Task AddPublisher(Guid sub, Publisher h);

        Task<IEnumerable<Publisher>> GetPublisher(Guid sub);
    }
}