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

        IBookingsRepository _repo;
        ILogger<BookingsController> _log;

        TwilioOptions _twilio;

        public BookingsController(
            IBookingsRepository repo,
            ILogger<BookingsController> log,
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

            var sub = GetUserId();

            var r = await _repo.Book(
                sub,
                req.Host,
                req.Start
            );
           
            if (r is Err<int> err)
            {
                _log.LogWarning("Booking request conflict with db-state.", err.Exception);

                return Conflict();
            }
            else 
            {
                if (r is OK<int> effect && effect.Result == 0)
                {
                    return NotFound();
                }

                _log.LogInformation($"{sub} created booking for {req.Host}/{req.Start}");
                return Created("bookings", req);
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

        // POST api/values
        [Authorize("bookings")]
        [HttpDelete("bookings/{start}")]
        public async Task<ActionResult> UnBook(
            long start)
        {
            var user = User.Claims
                .First(x => x.Type == "sub")
                .Value;
            
            var r = await _repo.UnBook(
                Guid.Parse(user),
                start
            );

            if (r == 0)
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
