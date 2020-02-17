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
        }

        public Task<IEnumerable<Host>> GetHosts() => _u
            .ToConnection()
            .ListHosts()
            .SubmitQuery(ListHostExtensions.ToHost);

        public Task<IEnumerable<Time>> GetTimes(
            Guid host, 
            long start, 
            long end) => _u
            .ToConnection()
            .ListTimes(host, start, end)
            .SubmitQuery(ListTimesExtensions.ToMeet);
    }
}
