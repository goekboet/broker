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
        
        [HttpGet("hosts")]
        public async Task<ActionResult<IEnumerable<object>>> GetHosts()
        {
            var r = await _repo.GetHosts();

            return Ok(r.Select(x => new HostJson
            {
                Id = x.Id.ToString(),
                Name = x.Name,
                TimeZone = "n/a"
            }));
        }

        [HttpGet("hosts/{host}/times")]
        public async Task<ActionResult<IEnumerable<TimeJson>>> ListTimes(
            Guid host,
            long from,
            long to)
        {
            var r = await _repo.GetTimes(host, from, to);

            return Ok(r.Select(x => new TimeJson
            {
                HostId = x.Host.ToString(),
                Name = x.Name,
                Start = x.Start,
                Dur = (int)((x.End - x.Start) / (1000 * 60))
            }));
        }
    }
}