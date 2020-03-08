using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PublicCallers.Scheduling;
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

        public async Task<AddHostResult<NewHost>> AddPublisher(Guid sub, NewHost h)
        {
            using (var conn = _u.ToConnection())
            {
                var sql = await conn
                    .AddPublisher(sub, h.Handle, h.Name)
                    .SubmitCommandReturning(AddPublisherExtensions.ToHost);

                switch (sql)
                {
                    case Success<NewHost> query:
                        return new Accepted<NewHost>(query.Result);
                    case Fail<NewHost> f:
                        if (f.Exception.ConstraintName == "unique_handle")
                        {
                            return new Conflict<NewHost>();
                        }
                        throw f.Exception;
                    default:
                        throw new IndexOutOfRangeException(nameof(sql));
                }
            }
        }

        public async Task AddTime(Guid sub, PublishedTime t)
        {
            using (var conn = _u.ToConnection())
            {
                await conn.AddTime(sub, t)
                    .SubmitCommand();
            }
        }


        public async Task<IEnumerable<BookedTime>> GetBookedTimes(
            Guid sub, 
            string handle,
            long from, 
            long to)
        {
            using (var conn = _u.ToConnection())
            {
                return await conn
                    .GetBookedTimes(sub, handle, from, to)
                    .SubmitQuery(GetBookedTimesExtensions.ToBookedTime);
            }
        }


        public async Task<IEnumerable<NewHost>> GetPublisher(
            Guid sub)
        {
            using (var conn = _u.ToConnection())
            {
                return await conn
                    .GetPublisher(sub)
                    .SubmitQuery(GetPublisherExtensions.ToHost);
            }
        }


        public async Task<PublishedTime> GetTime(Guid sub, string handle, long start)
        {
            using (var conn = _u.ToConnection())
            {
                return await conn
                    .GetPublishedTime(sub, handle, start)
                    .SubmitSingleQuery(GetPublishedTimesExtensions.ToPublishedTime);
            }
        }


        public async Task<IEnumerable<PublishedTime>> ListPublishedTimes(
            Guid sub,
            string handle,
            long from,
            long to)
        {
            using (var conn = _u.ToConnection())
            {
                return await conn
                    .ListPublisherTimes(sub, handle, from, to)
                    .SubmitQuery(ListPublisherTimesExtensions.ToPublishedTime);
            }
        }
    }
}