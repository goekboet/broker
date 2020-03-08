using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        IPublicMeetsRepository _repo;
        // GET a

        public PublicMeetsController(
            IPublicMeetsRepository repo
        )
        {
            _repo = repo;
        }
        
        int ParsePageParam(string s) => 
            int.TryParse(s, out int p) ? p : 0;
        

        [HttpGet("hosts")]
        public async Task<ActionResult<IEnumerable<object>>> GetHosts(
            string notBeforeName,
            string p
        )
        {
            var page = ParsePageParam(p);

            var r = await _repo.GetHosts(page * 100, notBeforeName ?? string.Empty);
            var listing = r.Select(x => new HostListingJson
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
            var r = await _repo.GetTimes(host, from, to);

            return Ok(r.Select(x => new TimeJson
            {
                Host = x.Host.ToString(),
                Name = x.Name,
                Start = x.Start,
                Dur = (int)((x.End - x.Start) / 60)
            }));
        }
    }
}