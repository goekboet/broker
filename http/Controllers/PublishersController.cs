using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using http.Twilio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PublicCallers.Scheduling;
using scheduling;

namespace http.Controllers
{
    [ApiController]
    public class PublishersController : ControllerBase
    {
        IPublisherRepository _repo;
        ILogger<PublishersController> _log;
        TwilioOptions _twilio;

        Guid GetUserId()
        {
            var sub = User.Claims.First(x => x.Type == "sub").Value;
            if (Guid.TryParse(sub, out var id))
            {
                return id;
            }
            else
            {
                throw new Exception("Unable to read userId");
            }
        }

        public PublishersController(
            IPublisherRepository repo,
            ILogger<PublishersController> log,
            IOptions<TwilioOptions> opts
        )
        {
            _repo = repo;
            _log = log;
            _twilio = opts.Value;
        }

        private static string ValidHostPayload(PublisherJson p)
        {
            var handle = string.IsNullOrWhiteSpace(p.Handle)
                ? "handle must be text"
                : string.Empty;
            var name = string.IsNullOrWhiteSpace(p.Name)
                ? "handle must be text"
                : string.Empty;

            return string.Join(", ", 
                from s in new[] { handle, name }
                where s != string.Empty 
                select s);
        }

        [HttpPost("publishers")]
        [Authorize("publish")]
        public async Task<ActionResult> AddHost(
            PublisherJson p)
        {
            var validation = ValidHostPayload(p);
            if (validation.Length > 0)
            {
                _log.LogWarning($"Invalid payload: {validation}");
                return BadRequest();
            }

            var created = await _repo.AddPublisher(GetUserId(), new NewHost(p.Handle, p.Name));

            switch (created)
            {
                case scheduling.Accepted<NewHost> a:
                    return a.Value == null
                        ? Conflict() as ActionResult
                        : Created($"publishers", a.Value);
                case scheduling.Conflict<NewHost> c:
                    return Conflict();
                default:
                    throw new ArgumentOutOfRangeException(nameof(created));
            }
        }

        [HttpGet("publishers")]
        [Authorize("publish")]
        public async Task<ActionResult> ListPublishers()
        {
            var sub = GetUserId();
            var r = await _repo.GetPublisher(sub);

            return Ok(r);
        }

        // [HttpGet("publishers/{name}")]
        // [Authorize("publish")]
        // public async Task<ActionResult> GetHost(string name)
        // {
        //     var sub = User.Claims.First(x => x.Type == "sub").Value;
        //     var r = (await _repo.GetPublisher(Guid.Parse(sub)));

        //     var p = r.SingleOrDefault();
        //     return p == null
        //         ? NotFound()
        //         : Ok(p) as ActionResult;
        // }

        [HttpPost("publishers/{handle}/times")]
        [Authorize("publish")]
        public async Task<ActionResult> PublishTime(
            string handle,
            PublishTimeJson payload)
        {
            var sub = GetUserId();
            await _repo.AddTime(sub, new PublishedTime(
                start: payload.Start,
                end: payload.End,
                hostHandle: handle,
                record: payload.Name
            ));

            return Created($"hosts/{handle}/times/{payload.Start}", payload);
        }

        [HttpGet("publishers/{handle}/times")]
        [Authorize("publish")]
        public async Task<ActionResult> ListPublishedTimes(
            string handle,
            long from,
            long to
        )
        {
            var sub = GetUserId();
            var r = await _repo.ListPublishedTimes(sub, handle, from, to);

            return Ok(from p in r
                      select new PublishTimeJson
                      {
                          Start = p.Start,
                          End = p.End,
                          Name = p.Record,
                          Booked = p.Booked.HasValue
                      });
        }

        [HttpGet("publishers/{handle}/times/{start}")]
        [Authorize("publish")]
        public async Task<ActionResult> GetPublishedTime(
            string handle,
            long start
        )
        {
            var sub = GetUserId();
            var r = await _repo.GetTime(sub, handle, start);
            var token = _twilio.GetTwilioToken(sub.ToString(), r.HostHandle, r.Start);
            return Ok(
                new AppointmentJson
                {
                    Start = r.Start,
                    Dur = (int)((r.End - r.Start) / 60),
                    Counterpart = r.Booked?.ToString(),
                    Token = token
                });
        }

        [HttpGet("publishers/{handle}/bookings")]
        [Authorize("publish")]
        public async Task<ActionResult> GetBookedTimes(
            string handle,
            long from,
            long to
        )
        {
            var sub = GetUserId();
            var r = await _repo.GetBookedTimes(sub, handle, from, to);

            return Ok(from p in r
                      select new BookedTimeJson
                      {
                          Start = p.Start,
                          End = p.End,
                          Name = p.Record,
                          Booker = p.Booker
                      });
        }
    }
}
