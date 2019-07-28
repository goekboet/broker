using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace http.Controllers
{
    public static class MockValues
    {
        public static long SomeStartDate = new DateTimeOffset(
            new DateTime(2019, 12, 24),
            TimeSpan.Zero).ToUnixTimeSeconds();
    }
    
    public class Schedule
    {
        public string TimeZone { get; }
        public string ClientId { get; }
        public string Handle { get; }
        public string Start { get; }
        public string End { get; }
        public Period[] Periods { get; }
    }

    public class ScheduleEntry
    {
        public string TimeZone { get; set; }
        public string Handle { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
    }

    public class Period
    {
        public string Year { get; set; }
        public string Day { get; set; }
        public string OClock { get; set; }
        public int Recur { get; set; }
    }

    public class PeriodHandle
    {
        public string Year { get; set; }
        public string Day { get; set; }
        public string OClock { get; set; }
    }

    public class Meet
    {
        public long Start { get; }
    }

    public class Booking
    {
        public string Meet { get; }
    }

    [ApiController]
    public class SchedulesController : ControllerBase
    {
        // GET a
        [HttpGet("schedules")]  
        public ActionResult<IEnumerable<object>> Get()
        {
            return new [] 
            { 
                new  
                { 
                    handle = "someHandle",
                    hostId = "someHostId"
                }
            };
        }

        [HttpPost("schedules")]
        public ActionResult<string> AddSchedule(
            [FromBody] ScheduleEntry entry)
        {
            return Created($"schedules/{entry.Handle}", new object());
        }

        [HttpDelete("schedules/{handle}")]
        public ActionResult DeleteSchedule(
            string handle)
        {
            return Ok();   
        }

        [HttpGet("schedules/{handle}")]
        public ActionResult<object> GetSchedule(string handle)
        {
            return new 
            {
                timeZone = "Europe/Stockholm",
                hostId = "someHost",
                handle = "someHandle",
                start = "2019/32",
                end = "2019/52",
                periods = new []
                {
                    new 
                    {
                        id = "someId",
                        year = "2019",
                        week = "33",
                        day = "mon",
                        oclock = "15:00",
                        recur = 20
                    }
                }
            };
        }

        // POST api/values
        [HttpPost("schedules/{handle}/periods")]
        public ActionResult AddPeriod(
            string handle,
            [FromBody] Period period)
        {
            return Created($"schedules/{handle}", "someId" );
        }

        [HttpDelete("schedules/{handle}/periods/{id}")]
        public ActionResult RemovePeriod(string handle, string id)
        {
            return NoContent();
        }

        [HttpGet("schedules/{handle}/meets")]
        public ActionResult<object> GetMeets(string handle)
        {
            return new []
            {
                new
                {
                    start = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    dur = 30,
                    id = "someMeetId"
                }
            };
        }

        [HttpGet("schedules/{handle}/bookings")]
        public ActionResult<object> ListBookings(
            string handle)
        {
            return new []
            {
                "someMeetId"
            };
        }

        [HttpPost("schedules/{handle}/bookings")]
        public ActionResult AddBooking(
            string handle,
            [FromBody] string meetId)
        {
            return Ok();
        }

        [HttpDelete("schedules/{handle}/bookings/{id}")]
        public ActionResult DeleteBooking(
            string handle,
            string id)
        {
            return Ok();   
        }

        [HttpGet("schedules/{handle}/bookings/{meetId}")]
        public ActionResult<object> GetBooking()
        {
            return new
            {
                id = "someMeetId",
                hostName = "someHostName",
                start = MockValues.SomeStartDate,
                dur = 30
            };
        }
    }
}
