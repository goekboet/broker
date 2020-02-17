using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using scheduling;

namespace postgres
{
    public class PublisherRepo : IPublisherRepository
    {
        private PgresUser _u;

        public PublisherRepo(
            PgresUser u)
        {
            _u = u;
        }

        public Task AddPublisher(Guid sub, Publisher h) => _u
                .ToConnection()
                .AddPublisher(sub, h.Name)
                .SubmitCommand();

        public Task AddTime(Guid sub, PublishedTime t) => _u
            .ToConnection()
            .AddTime(sub, t)
            .SubmitCommand();

        public Task<IEnumerable<Publisher>> GetPublisher(
            Guid sub) => _u
            .ToConnection()
            .GetPublisher(sub)
            .SubmitQuery(GetPublisherExtensions.ToHost);

        public Task<PublishedTime> GetTime(Guid sub, long start) => _u
            .ToConnection()
            .GetPublisherTimes(sub, start)
            .SubmitSingleQuery(GetPublisherTimesExtensions.ToPublishedTime);

        public Task<IEnumerable<PublishedTime>> ListPublishedTimes(
            Guid sub,
            long from,
            long to) => _u
            .ToConnection()
            .ListPublisherTimes(sub, from, to)
            .SubmitQuery(ListPublisherTimesExtensions.ToPublishedTime);
    }
}