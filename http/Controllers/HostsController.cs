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
            var sw = Stopwatch.StartNew();
            var r = await _repo.GetHosts();
            _log.LogInformation("GetHosts took {ElapsedMs} ms", sw.ElapsedMilliseconds);

            return Ok(r.Select(x => new
            {
                hostId = x.Id,
                hostName = x.Name
            }));
        }

        [EnableCors("PublicData")]
        [AllowAnonymous]
        [HttpGet("hosts/{host}/times")]
        public async Task<ActionResult<IEnumerable<object>>> GetMeets(
            Guid host,
            long from,
            long to)
        {
            var sw = Stopwatch.StartNew();
            var r = await _repo.GetMeets(host, from, to);
            _log.LogInformation("GetMeets took {ElapsedMs} ms", sw.ElapsedMilliseconds);

            return Ok(r.Select(x => new 
            {
                hostId = x.Host,
                meetName = x.Name,
                start = x.Start,
                dur = (int)((x.End - x.Start) / (1000 * 60))
            }));
        }
    }
}
