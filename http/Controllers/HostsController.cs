using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PublicCallers.Scheduling;

namespace http.Controllers
{
    [ApiController]
    public class HostsController : ControllerBase
    {
        IHostsRepository _repo;
        ILogger<HostsController> _log;

        public HostsController(
            IHostsRepository repo,
            ILogger<HostsController> log
        ) 
        {
            _repo = repo;
            _log = log;
        }
        // GET a
        [EnableCors("PublicData")]
        [AllowAnonymous]
        [HttpGet("hosts")]  
        public async Task<ActionResult<IEnumerable<object>>> GetHosts()
        {
            var r = await _repo.GetHosts();

            return Ok(r.Select(x => new HostJson
            {
                Id = x.Id.ToString(),
                Name = x.Name,
                TimeZone = x.Timezone
            }));
        }

        [EnableCors("PublicData")]
        [AllowAnonymous]
        [HttpGet("hosts/{host}/times")]
        public async Task<ActionResult<IEnumerable<TimeJson>>> GetTimes(
            Guid host,
            long from,
            long to)
        {
            var sw = Stopwatch.StartNew();
            var r = await _repo.GetTimes(host, from, to);
            _log.LogInformation("GetMeets took {ElapsedMs} ms", sw.ElapsedMilliseconds);

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
