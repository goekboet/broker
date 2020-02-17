using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PublicCallers.Scheduling;

namespace http.Controllers
{
    [ApiController]
    public class BookingsController : ControllerBase
    {
        IBookingsRepository _repo;
        ILogger<BookingsController> _log;

        public BookingsController(
            IBookingsRepository repo,
            ILogger<BookingsController> log
        ) 
        {
            _repo = repo;
            _log = log;
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

            var sw = Stopwatch.StartNew();
            var r = await _repo.UnBook(
                Guid.Parse(user),
                start
            );
            _log.LogInformation("UnBook took {ElapsedMs} ms", sw.ElapsedMilliseconds);

            if (r == 0)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
