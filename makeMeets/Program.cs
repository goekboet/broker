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
        public string Sub { get; set; }
        public string Tz { get; set; }
        public string Handle { get; set; }
        public Dictionary<Weekday, IEnumerable<(int h, int m, int dur)>> DailyTimes { get; set; }
    }

    static class Seed
    {
        public static IEnumerable<string> Tz =>
            from z in TimeZoneInfo.GetSystemTimeZones()
            select z.Id;

        public static IEnumerable<(int h, int m, int dur)> StandardTimes => new[]
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

        public static IEnumerable<MeetOwner> Host => new[]
        {
            new MeetOwner
            {
                Sub = "fea5983e-8124-43bf-b632-c5ec90c0003a",
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
                Sub = "00f29433-4fea-4a83-8883-c2fad9ced961",
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
                Sub = "969f3a55-546b-4e94-9b4a-143886f5142e",
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
                Sub = "05fde755-c776-4a65-9333-ea3b558ab94a",
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
                Sub = "d47d93a7-064b-49fc-96e2-9d4b1050c544",
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
                Sub = "3e9d9aa2-9dad-42bc-b623-fa87d502d9ff",
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
                Sub = "23f900d8-4b52-4e96-8ab9-7a610992b0d2",
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
                Sub = "37818de2-b07c-4c89-b464-5c293e68adeb",
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
                Sub = "2e330418-3e6f-4241-a0fe-4da5c9a79533",
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
                Sub = "e9418a96-39a0-404d-8f8f-d2df046d2790",
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
                Sub = "f3f0b3a7-f033-447c-afca-2076b5e48950",
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
                Sub = "26991761-2335-4578-915f-f9943ef90b53",
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
            if (!new[] { year, week, repeat }.All(x => x))
            {
                MeetUseage();
                return;
            }

            Console.WriteLine("host,start,end,record,booked");
            var data =
                from h in Seed.Host
                from s in Schedule.WeeklySchedule(y, w, h.DailyTimes, r)
                let d = s.ToTimeData(h.Sub, TimeZoneInfo.FindSystemTimeZoneById(h.Tz))
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
            Console.WriteLine("sub, handle");
            var data =
                from h in Seed.Host
                select $"{h.Sub},{h.Handle}";

            Console.WriteLine(string.Join(Environment.NewLine, data));
        }
    }
}
