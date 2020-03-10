using System;
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
    }
}