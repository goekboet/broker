using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PublicCallers.Scheduling;

using Schedule = PublicCallers.Scheduling.SchedulingExtensions;

namespace makeMeets
{
    public class MeetOwner
    {
        public string Id { get; set; }
        public string Tz { get; set; }
        public string Handle { get; set;}
        public Dictionary<Weekday, IEnumerable<(int h, int m, int dur)>> DailyTimes { get; set; }
    }

    static class Seed
    {
        public static IEnumerable<string> Tz => 
            from z in TimeZoneInfo.GetSystemTimeZones()
            select z.Id;

        public static IEnumerable<(int h, int m, int dur)> StandardTimes => new []
        {
            (10, 0, 45),
            (11, 0, 45),
            (13, 0, 45),
            (14, 0, 45),
            (15, 0, 45),
            (16, 0, 45),
            (17, 0, 45),
            (18, 0, 45)
        };

        public static IEnumerable<MeetOwner> Host => new []
        {
            new MeetOwner
            {
                Id = "fad10aa0-e78c-46f2-a071-fb64da28c4bf",
                Handle = "First",
                Tz = "Europe/Stockholm",
                DailyTimes = new Dictionary<Weekday, IEnumerable<(int h, int m, int dur)>>
                {
                    [Weekday.Mon] = StandardTimes,
                    [Weekday.Tue] = StandardTimes,
                    [Weekday.Wed] = StandardTimes,
                    [Weekday.Thu] = StandardTimes,
                    [Weekday.Fri] = StandardTimes
                }
            },
            new MeetOwner
            {
                Id = "264335aa-a163-44e5-a5da-f8ae04e1dffd",
                Handle = "Second",
                Tz = "Europe/Stockholm",
                DailyTimes = new Dictionary<Weekday, IEnumerable<(int h, int m, int dur)>>
                {
                    [Weekday.Mon] = StandardTimes,
                    [Weekday.Tue] = StandardTimes,
                    [Weekday.Wed] = StandardTimes,
                    [Weekday.Thu] = StandardTimes,
                    [Weekday.Fri] = StandardTimes
                }
            },
            new MeetOwner
            {
                Id = "100c899e-d945-4bfc-95ef-891587ae686b",
                Handle = "Third",
                Tz = "Europe/Stockholm",
                DailyTimes = new Dictionary<Weekday, IEnumerable<(int h, int m, int dur)>>
                {
                    [Weekday.Mon] = StandardTimes,
                    [Weekday.Tue] = StandardTimes,
                    [Weekday.Wed] = StandardTimes,
                    [Weekday.Thu] = StandardTimes,
                    [Weekday.Fri] = StandardTimes
                }
            },
            new MeetOwner
            {
                Id = "ffdf7237-4df9-44e9-a81a-4b50115d33a5",
                Handle = "Forth",
                Tz = "Europe/Helsinki",
                DailyTimes = new Dictionary<Weekday, IEnumerable<(int h, int m, int dur)>>
                {
                    [Weekday.Mon] = StandardTimes,
                    [Weekday.Tue] = StandardTimes,
                    [Weekday.Wed] = StandardTimes,
                    [Weekday.Thu] = StandardTimes,
                    [Weekday.Fri] = StandardTimes
                }
            },
            new MeetOwner
            {
                Id = "1ed8a38d-8d47-42d3-a5de-e8bcec9dca6a",
                Handle = "Fifth",
                Tz = "Europe/Helsinki",
                DailyTimes = new Dictionary<Weekday, IEnumerable<(int h, int m, int dur)>>
                {
                    [Weekday.Mon] = StandardTimes,
                    [Weekday.Tue] = StandardTimes,
                    [Weekday.Wed] = StandardTimes,
                    [Weekday.Thu] = StandardTimes,
                    [Weekday.Fri] = StandardTimes
                }
            },
            new MeetOwner
            {
                Id = "858d53e0-a818-4dfa-a420-579ebd5ccd5b",
                Handle = "Sixth",
                Tz = "Europe/Helsinki",
                DailyTimes = new Dictionary<Weekday, IEnumerable<(int h, int m, int dur)>>
                {
                    [Weekday.Mon] = StandardTimes,
                    [Weekday.Tue] = StandardTimes,
                    [Weekday.Wed] = StandardTimes,
                    [Weekday.Thu] = StandardTimes,
                    [Weekday.Fri] = StandardTimes
                }
            },
            new MeetOwner
            {
                Id = "4c640f8c-90b6-4c9e-ade9-ed4652550338",
                Handle = "Seventh",
                Tz = "Europe/London",
                DailyTimes = new Dictionary<Weekday, IEnumerable<(int h, int m, int dur)>>
                {
                    [Weekday.Mon] = StandardTimes,
                    [Weekday.Tue] = StandardTimes,
                    [Weekday.Wed] = StandardTimes,
                    [Weekday.Thu] = StandardTimes,
                    [Weekday.Fri] = StandardTimes
                }
            },
            new MeetOwner
            {
                Id = "23ab2835-c34b-4413-b5ab-03095641951f",
                Handle = "Eighth",
                Tz = "Europe/London",
                DailyTimes = new Dictionary<Weekday, IEnumerable<(int h, int m, int dur)>>
                {
                    [Weekday.Mon] = StandardTimes,
                    [Weekday.Tue] = StandardTimes,
                    [Weekday.Wed] = StandardTimes,
                    [Weekday.Thu] = StandardTimes,
                    [Weekday.Fri] = StandardTimes
                }
            },
            new MeetOwner
            {
                Id = "b40eb02e-8783-454b-9784-fd56ecdf5bc6",
                Handle = "Nineth",
                Tz = "Europe/London",
                DailyTimes = new Dictionary<Weekday, IEnumerable<(int h, int m, int dur)>>
                {
                    [Weekday.Mon] = StandardTimes,
                    [Weekday.Tue] = StandardTimes,
                    [Weekday.Wed] = StandardTimes,
                    [Weekday.Thu] = StandardTimes,
                    [Weekday.Fri] = StandardTimes
                }
            },
            new MeetOwner
            {
                Id = "9941dad8-8581-4585-8def-4a3c3cd2c102",
                Handle = "Tenth",
                Tz = "Europe/Moscow",
                DailyTimes = new Dictionary<Weekday, IEnumerable<(int h, int m, int dur)>>
                {
                    [Weekday.Mon] = StandardTimes,
                    [Weekday.Tue] = StandardTimes,
                    [Weekday.Wed] = StandardTimes,
                    [Weekday.Thu] = StandardTimes,
                    [Weekday.Fri] = StandardTimes
                }
            },
            new MeetOwner
            {
                Id = "29264b31-ba12-4110-b3fa-128536e70510",
                Handle = "Eleventh",
                Tz = "Europe/Moscow",
                DailyTimes = new Dictionary<Weekday, IEnumerable<(int h, int m, int dur)>>
                {
                    [Weekday.Mon] = StandardTimes,
                    [Weekday.Tue] = StandardTimes,
                    [Weekday.Wed] = StandardTimes,
                    [Weekday.Thu] = StandardTimes,
                    [Weekday.Fri] = StandardTimes
                }
            },
            new MeetOwner
            {
                Id = "eb3d89cb-07c7-42b5-acf5-b71338df68d2",
                Handle = "Twelfth",
                Tz = "Europe/Moscow",
                DailyTimes = new Dictionary<Weekday, IEnumerable<(int h, int m, int dur)>>
                {
                    [Weekday.Mon] = StandardTimes,
                    [Weekday.Tue] = StandardTimes,
                    [Weekday.Wed] = StandardTimes,
                    [Weekday.Thu] = StandardTimes,
                    [Weekday.Fri] = StandardTimes
                }
            }
        };
    }
    
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0 || args[0] == "")
            {
                Useage();
                return 0;
            }

            switch (args[0])
            {
                case "tz":
                    Tz();
                    break;
                case "hosts":
                    Hosts();
                    break;
                case "meets":
                    Meets(args.Skip(1));
                    break;
                default:
                    Useage();
                    break;
            }

            return 0;
        }

        private static void MeetUseage()
        {
            Console.WriteLine("meets year week repeat");
            Console.WriteLine("year: data begins this year. Must parse as int");
            Console.WriteLine("week: data begins this week. Must parse as int");
            Console.WriteLine("repeat: Number of week data covers. Must parse as int.");
        }

        private static void Meets(IEnumerable<string> args)
        {
            if (args.Count() != 3)
            {
                MeetUseage();
                return;
            }
            var year = int.TryParse(args.ElementAt(0), out var y);
            var week = int.TryParse(args.ElementAt(1), out int w);
            var repeat = int.TryParse(args.ElementAt(2), out int r);
            if (!new [] { year, week, repeat }.All(x => x))
            {
                MeetUseage();
                return;
            }

            Console.WriteLine("host,start,end,record,booked");
            var data = 
                from h in Seed.Host
                from s in Schedule.WeeklySchedule(y, w, h.DailyTimes, r)
                let d = s.ToTimeData(h.Id, TimeZoneInfo.FindSystemTimeZoneById(h.Tz))
                select $"{d.Host},{d.Start},{d.End},{d.Record},";

            Console.WriteLine(string.Join(Environment.NewLine, data));
        }

        static void Useage()
        {
            Console.WriteLine("List of data:");
            Console.WriteLine("tz");
            Console.WriteLine("hosts");
            Console.WriteLine("meets");
            Console.WriteLine();
            Console.WriteLine("Program will print .csv for any of these data in first arg.");
        }

        static void Tz()
        {
            Console.WriteLine("timezone");
            Console.WriteLine(string.Join(
                    Environment.NewLine, 
                    from z in TimeZoneInfo.GetSystemTimeZones()
                    select z.Id));
        }

        static void Hosts()
        {
            Console.WriteLine("id, tz, handle");
            var data = 
                from h in Seed.Host
                select $"{h.Id},{h.Tz},{h.Handle}";

            Console.WriteLine(string.Join(Environment.NewLine, data));
        }
    }
}
