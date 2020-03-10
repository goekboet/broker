using System;
using System.Linq;
using System.Threading.Tasks;
using http.Twilio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PublicCallers.Scheduling;
using scheduling;

namespace http.Publisher
{
    [ApiController]
    public class PublishersController : ControllerBase
    {
        IPublisherRepository _repo;
        ILogger<PublishersController> _log;
        TwilioOptions _twilio;
        IDataSource _db;
        PgresUser _creds;

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
            IOptions<TwilioOptions> opts,
            IDataSource db,
            PgresUser creds
        )
        {
            _db = db;
            _creds = creds;
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
            var query = new ListPublishers(GetUserId());
            var result = await _db.Submit(_creds, query);

            return Ok(result);
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
            var query = new ListPublishedTimes(
                sub: GetUserId(),
                handle: handle,
                from: from,
                to: to
            );
            var result = await _db.Submit(_creds, query);

            return Ok(from p in result
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
            var query = new GetPublishedTime(
                sub: sub,
                host: handle, 
                start: start
            );
            var r = await _db.Submit(_creds, query);
            var result = r.Single(); 

            var token = _twilio.GetTwilioToken(sub.ToString(), result.HostHandle, result.Start);
            return Ok(
                new AppointmentJson
                {
                    Start = result.Start,
                    Dur = (int)((result.End - result.Start) / 60),
                    Counterpart = result.Booked?.ToString(),
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
            var query = new GetBookedPublishedTimes(
                sub: GetUserId(),
                handle: handle,
                from: from,
                to: to
            );
            var result = await _db.Submit(_creds, query);

            return Ok(from p in result
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
