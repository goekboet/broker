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
                TimeZone = "n/a"
            }));
        }

        [HttpPost("publishers")]
        [Authorize("publish")]
        public async Task<ActionResult> AddHost(
            PublisherJson p)
        {
            var sub = User.Claims.First(x => x.Type == "sub").Value;
            await _repo.AddHost(new Host(Guid.Parse(sub), p.Name));

            return Created($"publishers/{p.Name}", p);
        }

        [HttpGet("publishers")]
        [Authorize("publish")]
        public async Task<ActionResult> ListPublishers()
        {
            var sub = User.Claims.First(x => x.Type == "sub").Value;
            var r = (await _repo.GetHost(Guid.Parse(sub)));

            return Ok(r);
        }

        [HttpGet("publishers/{name}")]
        [Authorize("publish")]
        public async Task<ActionResult> GetHost(string name)
        {
            var sub = User.Claims.First(x => x.Type == "sub").Value;
            var r = (await _repo.GetHost(Guid.Parse(sub)));

            var p = r.SingleOrDefault();
            return p == null
                ? NotFound()
                : Ok(p) as ActionResult;
        }

        [HttpPost("publishers/{name}/times")]
        [Authorize("publish")]
        public async Task<ActionResult> PublishTime(
            PublishTimeJson payload)
        {
            var sub = User.Claims.First(x => x.Type == "sub").Value;
            await _repo.AddTime(new Time(
                payload.Start, 
                payload.Name, 
                Guid.Parse(sub),
                payload.End));

            return Created($"hosts/{sub}/times?from={payload.Start}&to={payload.End}", payload);
        }

        [HttpGet("publishers/{name}/times")]
        [Authorize("publish")]
        public async Task<ActionResult> ListPublishedTimes()
        {
            return await Task.FromResult(NotFound() as ActionResult);
        }

        [HttpGet("publishers/{name}/times/{start}")]
        [Authorize("publish")]
        public async Task<ActionResult> GetPublishedTime()
        {
            return await Task.FromResult(NotFound() as ActionResult);
        }

        [EnableCors("PublicData")]
        [AllowAnonymous]
        [HttpGet("hosts/{host}/times")]
        public async Task<ActionResult<IEnumerable<TimeJson>>> ListTimes(
            Guid host,
            long from,
            long to)
        {
            var sw = Stopwatch.StartNew();
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
