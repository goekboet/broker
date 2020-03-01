using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using http.Twilio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PublicCallers.Scheduling;

namespace http.Controllers
{
    [ApiController]
    public class BookingsController : ControllerBase
    {
        IBookingsRepository _repo;
        ILogger<BookingsController> _log;

        TwilioOptions _twilio;

        public BookingsController(
            IBookingsRepository repo,
            ILogger<BookingsController> log,
            IOptions<TwilioOptions> opts
        ) 
        {
            _repo = repo;
            _log = log;
            _twilio = opts.Value;
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

            var user = User.Claims
                .First(x => x.Type == "sub")
                .Value;

            var r = await _repo.Book(
                Guid.Parse(user),
                Guid.Parse(req.HostId),
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

                _log.LogInformation($"{user} created booking for {req.HostId}/{req.Start}");
                return Created("bookings", req);
            }
        }

        [Authorize("bookings")]
        [HttpGet("bookings")]
        public async Task<ActionResult<IEnumerable<TimeJson>>> GetBookings()
        {
            var user = User.Claims
                .First(x => x.Type == "sub")
                .Value;

            var sw = Stopwatch.StartNew();
            var r = await _repo
                .GetBookedTimes(Guid.Parse(user));
            _log.LogInformation("GetBookings took {ElapsedMs} ms", sw.ElapsedMilliseconds);

            return Ok(r.Select(x => new TimeJson
            {
                HostId = x.Host.ToString(),
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
            var sub = User.Claims
                .First(x => x.Type == "sub")
                .Value;

            var r = await _repo.GetBooking(Guid.Parse(sub), start);

            if (r == null)
            {
                _log.LogWarning("No booking found for {sub}/{start}", sub, start);
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
                Host = r.Host.ToString(),
                Token = token,
                Start = r.Start,
                Dur = (int)(r.End - r.Start)
            });
        }
    }
}
