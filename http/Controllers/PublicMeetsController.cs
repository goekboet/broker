using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using http.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PublicCallers.Scheduling;

namespace http.Controllers
{
    [EnableCors("PublicData")]
    [AllowAnonymous]
    [ApiController]
    public class PublicMeetsController : ControllerBase
    {
        PgresUser _creds;
        IDataSource _db;

        public PublicMeetsController(
            IDataSource db,
            PgresUser creds
        )
        {
            _db = db;
            _creds = creds;
        }
        
        int ParsePageParam(string s) => 
            int.TryParse(s, out int p) ? p : 0;
        

        [HttpGet("hosts")]
        public async Task<ActionResult<HostListingJson[]>> GetHosts(
            string notBeforeName,
            string p
        )
        {
            var page = ParsePageParam(p);
            var query = new HostListingPagedByHandle(
                notBeforeName ?? string.Empty,
                100,
                page * 100
            );

            var result = await _db.Submit(_creds, query);

            var listing = result.Select(x => new HostListingJson
                {
                    Name = x.Name,
                    Handle = x.Handle
                }).ToArray();

            return Ok(listing);
        }

        [HttpGet("hosts/{host}/times")]
        public async Task<ActionResult<IEnumerable<TimeJson>>> ListTimes(
            string host,
            long from,
            long to)
        {
            var query = new TimesListingByStart(
                host,
                from,
                to
            );

            var result = await _db.Submit(_creds, query);

            return Ok(result.Select(x => new TimeJson
            {
                Host = x.Host.ToString(),
                Name = x.Name,
                Start = x.Start,
                Dur = (int)((x.End - x.Start) / 60)
            }));
        }
    }
}