using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NodaTime;

namespace makeMeets
{
    public class Host
    {
        public Guid Sub { get; set; }
        public string Handle { get; set; }
        public string Name { get; set; }

        public override string ToString() =>
            $"{Handle} - {Name}";
    }

    public class Time
    {
        public long Start { get; set; }
        public long End { get; set; }
        public string Host { get; set; }
        public string Record { get; set; }
    }

    class Program
    {
        static Random Rng { get; } = new Random();
        static IEnumerable<LocalDate> AllDatesInYear(int year)
        {
            var date = new LocalDate(year, 1, 1);
            while (date.Year == year)
            {
                yield return date;
                date = date.PlusDays(1);
            }
        }

        static IEnumerable<LocalTime> Times(
            LocalTime first,
            int forHours,
            double keepPct)
        {
            var time = first;
            while (forHours-- > 0)
            {
                if (Rng.NextDouble() < keepPct)
                {
                    yield return time;
                }
                time = time.PlusHours(1);
            }
        }

        static IEnumerable<Host> ReadHosts(string path)
        {
            var lines = File.ReadAllLines(path);

            return lines
                .Skip(1)
                .Select(x => x.Split(","))
                .Select(x => new Host
                {
                    Sub = Guid.Parse(x[0]),
                    Handle = x[1],
                    Name = x[2]
                });
        }

        public static IEnumerable<LocalDateTime> ForDay(LocalDate d) =>
            from t in Times(new LocalTime(8, 0), 10, 0.8)
            select new LocalDateTime(d.Year, d.Month, d.Day, t.Hour, t.Minute);

        public static IEnumerable<Time> ForHost(Host h, int year)
        {
            var tz = DateTimeZoneProviders.Tzdb["Europe/Stockholm"];
            
            return AllDatesInYear(year)
                .SelectMany(x => ForDay(x))
                .Select(x => new Time
                {
                    Start = x.InZoneLeniently(tz).ToInstant().ToUnixTimeSeconds(),
                    End = x.PlusMinutes(45).InZoneLeniently(tz).ToInstant().ToUnixTimeSeconds(),
                    Host = h.Handle,
                    Record = x.InZoneLeniently(tz).ToString()
                });
        }

        static int Main(string[] args)
        {
            var hosts = ReadHosts("hosts/hosts.csv");
            var times = hosts
                .SelectMany(x => ForHost(x, 2020));

            Console.WriteLine(
                string.Join("\n", 
                (from t in times
                select $"{t.Start},{t.End},{t.Host},{t.Record}")));

            return 0;
        }
    }
}
