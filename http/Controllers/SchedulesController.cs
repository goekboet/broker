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
    public class BookRequest
    {
        public Guid hostId { get;set;}
        public long start { get;set;}
    }

    [ApiController]
    public class SchedulesController : ControllerBase
    {
        IMeetsRepository _repo;
        ILogger<SchedulesController> _log;

        public SchedulesController(
            IMeetsRepository repo,
            ILogger<SchedulesController> log
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


        [Authorize("bookings")]
        [HttpPost("bookings")]
        public async Task<ActionResult> Book(
            BookRequest req)
        {
            var user = User.Claims
                .First(x => x.Type == "sub")
                .Value;

            var sw = Stopwatch.StartNew();
            var r = await _repo.Book(
                Guid.Parse(user),
                req.hostId,
                req.start
            );
            _log.LogInformation("Book took {ElapsedMs} ms", sw.ElapsedMilliseconds);
           
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

                _log.LogInformation($"{user} created booking for {req.hostId}/{req.start}");
                return Created("bookings", new object());
            }
        }

        [Authorize("bookings")]
        [HttpGet("bookings")]
        public async Task<ActionResult<IEnumerable<object>>> GetBookings()
        {
            var user = User.Claims
                .First(x => x.Type == "sub")
                .Value;

            var sw = Stopwatch.StartNew();
            var r = await _repo
                .GetBookings(Guid.Parse(user));
            _log.LogInformation("GetBookings took {ElapsedMs} ms", sw.ElapsedMilliseconds);

            return Ok(r.Select(x => new 
            {
                hostId = x.Hostid,
                meetName = x.HostHandle,
                start = x.Start,
                dur = (int)((x.End - x.Start) / (1000 * 60))
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
