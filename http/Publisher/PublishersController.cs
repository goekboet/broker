using System;
using System.Linq;
using System.Threading.Tasks;
using http.Twilio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PublicCallers.Scheduling;

namespace http.Publisher
{
    [ApiController]
    public class PublishersController : ControllerBase
    {
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
            ILogger<PublishersController> log,
            IOptions<TwilioOptions> opts,
            IDataSource db,
            PgresUser creds
        )
        {
            _db = db;
            _creds = creds;
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

            var addPublisher = new AddPublisher(
                sub: GetUserId(),
                handle: p.Handle,
                name: p.Name
            );
            var result = await _db.SubmitCommand(
                _creds,
                addPublisher);

            if (result == 1)
            {
                return Created($"publishers/{p.Handle}", p);
            }
            else
            {
                return Conflict();
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

        [HttpGet("publishers/{handle}")]
        [Authorize("publish")]
        public async Task<ActionResult> GetHost(
            string handle)
        {
            var found = new GetHandle(
                sub: GetUserId(),
                handle: handle
            );
            var r = (await _db.Submit(_creds, found))
                .SingleOrDefault();

            if (r != null)
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("times")]
        [Authorize("publish")]
        public async Task<ActionResult> PublishTime(
            PublishTimeJson payload)
        {
            var getHost = new GetHandle(
                sub: GetUserId(),
                handle: payload.Handle
            );
            var foundHost = (await _db.Submit(_creds, getHost))
                .SingleOrDefault();
            if (foundHost == null)
            {
                return NotFound();
            }

            var getConflict = new PublishedTimeConflict(
                sub: GetUserId(),
                start: payload.Start,
                end: payload.End
            );
            var conflict = (await _db.Submit(_creds, getConflict))
                .SingleOrDefault();

            if (conflict != null)
            {
                return Conflict();
            }

            var addTime = new AddTime(
                sub: GetUserId(),
                t: new PublishedTime(
                    start: payload.Start,
                    end: payload.End,
                    hostHandle: payload.Handle,
                    record: payload.Name
                )
            );
            var r = await _db.SubmitCommand(_creds, addTime);

            if (r == 1)
            {
                return Created($"times/{payload.Start}", payload);
            }
            else
            {
                _log.LogWarning($"Failed to insert time for {payload.Handle}/{payload.Start}");
                return BadRequest();
            }
        }

        [HttpGet("times")]
        [Authorize("publish")]
        public async Task<ActionResult> ListPublishedTimes(
            long from,
            long to
        )
        {
            var query = new ListPublishedTimes(
                sub: GetUserId(),
                from: from,
                to: to
            );
            var result = await _db.Submit(_creds, query);

            return Ok(from p in result
                      select new PublishTimeJson
                      {
                          Start = p.Start,
                          Handle = p.HostHandle,
                          End = p.End,
                          Name = p.Record,
                          Booked = p.Booked.HasValue
                      });
        }

        [HttpGet("times/{start}")]
        [Authorize("publish")]
        public async Task<ActionResult> GetPublishedTime(
            long start
        )
        {
            var sub = GetUserId();
            var query = new GetPublishedTime(
                sub: sub,
                start: start
            );
            var r = await _db.Submit(_creds, query);
            var result = r.Single();

            var token = _twilio.GetTwilioToken(sub.ToString(), result.HostHandle, result.Start);

            return Ok(
                new AppointmentJson
                {
                    Start = result.Start,
                    Host = result.HostHandle,
                    Dur = (int)((result.End - result.Start) / 60),
                    Counterpart = result.Booked?.ToString(),
                    Token = token
                });
        }

        [HttpGet("appointments")]
        [Authorize("publish")]
        public async Task<ActionResult> GetBookedTimes(
            long from,
            long to
        )
        {
            var query = new GetBookedPublishedTimes(
                sub: GetUserId(),
                from: from,
                to: to
            );
            var result = await _db.Submit(_creds, query);

            return Ok(from p in result
                      select new BookedTimeJson
                      {
                          Start = p.Start,
                          Host = p.Host,
                          End = p.End,
                          Name = p.Record,
                          Booker = p.Booker
                      });
        }
    }
}
