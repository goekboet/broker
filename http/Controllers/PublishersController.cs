using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PublicCallers.Scheduling;
using scheduling;

namespace http.Controllers
{
    [ApiController]
    public class PublishersController : ControllerBase
    {
        IPublisherRepository _repo;
        ILogger<PublishersController> _log;

        public PublishersController(
            IPublisherRepository repo,
            ILogger<PublishersController> log
        )
        {
            _repo = repo;
            _log = log;
        }
        

        [HttpPost("publishers")]
        [Authorize("publish")]
        public async Task<ActionResult> AddHost(
            PublisherJson p)
        {
            var sub = User.Claims.First(x => x.Type == "sub").Value;
            await _repo.AddPublisher(Guid.Parse(sub), new Publisher(p.Name));

            return Created($"publishers/{p.Name}", p);
        }

        [HttpGet("publishers")]
        [Authorize("publish")]
        public async Task<ActionResult> ListPublishers()
        {
            var sub = User.Claims.First(x => x.Type == "sub").Value;
            var r = (await _repo.GetPublisher(Guid.Parse(sub)));

            return Ok(r);
        }

        [HttpGet("publishers/{name}")]
        [Authorize("publish")]
        public async Task<ActionResult> GetHost(string name)
        {
            var sub = User.Claims.First(x => x.Type == "sub").Value;
            var r = (await _repo.GetPublisher(Guid.Parse(sub)));

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
            await _repo.AddTime(Guid.Parse(sub), new PublishedTime(
                start: payload.Start,
                end: payload.End,
                record: payload.Name
            ));

            return Created($"hosts/{sub}/times?from={payload.Start}&to={payload.End}", payload);
        }

        [HttpGet("publishers/{name}/times")]
        [Authorize("publish")]
        public async Task<ActionResult> ListPublishedTimes(
            long from,
            long to
        )
        {
            var sub = User.Claims.First(x => x.Type == "sub").Value;
            var r = await _repo.ListPublishedTimes(Guid.Parse(sub), from, to);

            return Ok(from p in r select new PublishTimeJson
            {
                Start = p.Start,
                End = p.End,
                Name = p.Record,
                Booked = p.Booked.HasValue
            });
        }

        [HttpGet("publishers/{name}/times/{start}")]
        [Authorize("publish")]
        public async Task<ActionResult> GetPublishedTime(
            string name,
            long start
        )
        {
            var sub = User.Claims.First(x => x.Type == "sub").Value;
            var r = await _repo.GetTime(Guid.Parse(sub), start);
            return Ok(
                new PublishTimeJson
                {
                    Start = r.Start,
                    End = r.End,
                    Name = r.Record,
                    Booked = r.Booked.HasValue
                });
        }
    }
}
