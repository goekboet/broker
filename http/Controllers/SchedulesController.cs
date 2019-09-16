using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublicCallers.Scheduling;

namespace http.Controllers
{
    public class BookRequest
    {
        public Guid HostId { get;set;}
        public long Start { get;set;}
    }

    [ApiController]
    public class SchedulesController : ControllerBase
    {
        IMeetsRepository _repo;
        public SchedulesController(
            IMeetsRepository repo
        ) 
        {
            _repo = repo;
        }
        // GET a
        [AllowAnonymous]
        [HttpGet("hosts")]  
        public async Task<ActionResult<IEnumerable<object>>> GetHosts()
        {
            var r = await _repo.GetHosts();

            return Ok(r.Select(x => new
            {
                hostId = x.Id,
                nostName = x.Name
            }));
        }

        [AllowAnonymous]
        [HttpGet("hosts/{host}/times")]
        public async Task<ActionResult<IEnumerable<object>>> GetMeets(
            Guid host,
            long from,
            long to)
        {
            var r = await _repo.GetMeets(host, from, to);

            return Ok(r.Select(x => new 
            {
                hostId = x.Host,
                meetName = x.Name,
                start = x.Start,
                dur = (int)(x.End - x.Start / (1000 * 60))
            }));
        }


        [Authorize("call")]
        [HttpPost("bookings")]
        public async Task<ActionResult> Book(
            BookRequest req)
        {
            var user = User.Claims
                .First(x => x.Type == "sub")
                .Value;

            var r = await _repo.Book(
                Guid.Parse(user),
                req.HostId,
                req.Start
            );

            return Created("bookings", new object());   
        }

        [Authorize("call")]
        [HttpGet("bookings")]
        public async Task<ActionResult<IEnumerable<object>>> GetBookings()
        {
            var user = User.Claims
                .First(x => x.Type == "sub")
                .Value;

            var r = await _repo
                .GetBookings(Guid.Parse(user));

            return Ok(r.Select(x => new 
            {
                hostId = x.Hostid,
                meetName = x.HostHandle,
                start = x.Start,
                dur = (int)(x.End - x.Start / (1000 * 60))
            }));           
        }

        // POST api/values
        [Authorize("call")]
        [HttpDelete("bookings")]
        public async Task<ActionResult> UnBook(
            BookRequest req)
        {
            var user = User.Claims
                .First(x => x.Type == "sub")
                .Value;

            var r = await _repo.UnBook(
                Guid.Parse(user),
                req.HostId,
                req.Start
            );

            return NoContent();
        }

    }
}
