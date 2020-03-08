using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PublicCallers.Scheduling;

namespace postgres
{
    public class PublicMeetRepo : IPublicMeetsRepository
    {
        private PgresUser _u;

        public PublicMeetRepo(
            PgresUser u)
        {
            _u = u;
            PageSize = 100;
        }

        public int PageSize { get; }

        public async Task<IEnumerable<HostListing>> GetHosts(
            int offset,
            string notBeforeName = "")
        {
            using (var conn = _u.ToConnection())
            {
                return await conn
                    .ListHosts(offset, PageSize, notBeforeName)
                    .SubmitQuery(ListHostExtensions.ToHost);
            }
        }


        public async Task<IEnumerable<Time>> GetTimes(
            string host,
            long start,
            long end)
        {
            using (var conn = _u.ToConnection())
            {
                return await conn
                    .ListTimes(host, start, end)
                    .SubmitQuery(ListTimesExtensions.ToMeet);
            }
        }

    }
}
