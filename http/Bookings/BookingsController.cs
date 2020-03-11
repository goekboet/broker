using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using http.Twilio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PublicCallers.Scheduling;

namespace http.Bookings
{
    [ApiController]
    public class BookingsController : ControllerBase
    {
        PgresUser _creds;
        IDataSource _db;
        ILogger<BookingsController> _log;
        TwilioOptions _twilio;

        public BookingsController(
            ILogger<BookingsController> log,
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

        [Authorize("bookings")]
        [HttpPost("bookings")]
        public async Task<ActionResult> Book(
            TimeJson req)
        {
            if (!req.Valid)
            {
                _log.LogWarning("Rejected input {input}", req);
                return BadRequest();
            }

            var getTime = new GetTime(
                sub: GetUserId(),
                host: req.Host,
                start: req.Start
            );
            var getTimeResult = (await _db.Submit(_creds, getTime))
                .SingleOrDefault();

            if (getTimeResult == null)
            {
                return NotFound();
            }

            var conflictsQuery = new GetBookingConflict(
                sub: GetUserId(),
                host: getTimeResult.Host,
                start: getTimeResult.Start,
                end: getTimeResult.End
            );
            var conflictsResult = (await _db.Submit(_creds, conflictsQuery))
                .SingleOrDefault();

            if (conflictsResult == null)
            {
                var book = new BookCommand(
                    sub: GetUserId(),
                    host: getTimeResult.Host,
                    start: getTimeResult.Start
                );
                var r = await _db.Submit(_creds, book);

                return Created(
                    $"bookings/{getTimeResult.Start}", 
                    r.Single());
            }
            else
            {
                return Conflict(conflictsResult);
            }
        }

        [Authorize("bookings")]
        [HttpGet("bookings")]
        public async Task<ActionResult<IEnumerable<TimeJson>>> GetBookings()
        {
            var query = new GetMyBookings(GetUserId());
            var result = await _db.Submit(_creds, query);

            return Ok(result.Select(x => new TimeJson
            {
                Host = x.Host.ToString(),
                Name = x.Name,
                Start = x.Start,
                Dur = (int)((x.End - x.Start) / (1000 * 60))
            }));           
        }

        [Authorize("bookings")]
        [HttpDelete("bookings/{start}")]
        public async Task<ActionResult> UnBook(
            long start)
        {
            var unbook = new UnBookCommand(
                sub: GetUserId(),
                start: start
            );
            var result = await _db.SubmitCommand(
                _creds, 
                unbook);

            if (result == 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        [Authorize("bookings")]
        [HttpGet("bookings/{start}")]
        public async Task<ActionResult> GetBooking(
            long start,
            string name = "n/a")
        {
            var sub = GetUserId();
            var query = new GetBooking(sub, start);
            var result = await _db.Submit(_creds, query);
            var r = result.SingleOrDefault();

            if (r == null)
            {
                return NotFound();
            }

            var token = _twilio.GetTwilioToken(name, r.Host.ToString(), r.Start);
            if (token == null)
            {
                _log.LogError("could not generate token with {twilioAccount}", _twilio.AccountSid);
                return StatusCode(500);
            }

            return Ok(new AppointmentJson
            {
                Counterpart = r.Host.ToString(),
                Token = token,
                Start = r.Start,
                Dur = (int)(r.End - r.Start)
            });
        }
    }
}
