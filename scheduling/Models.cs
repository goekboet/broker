using System;

namespace PublicCallers.Scheduling
{
    public class Host
    {
        public Host(
            Guid id,
            string name
        )
        {
            Id = id;
            Name = name;
        }
        
        public Guid Id { get; }
        public string Name { get; }
    }

    public class HostListing
    {
        public HostListing(
            string handle,
            string name
        )
        {
            Handle = handle;
            Name = name;
        }
        
        public string Handle { get; }
        public string Name { get; }
    }

    public class Time
    {
        public Time(
            long start,
            string name,
            string host,
            long end
            )
        {
            Start = start;
            Name = name;
            Host = host;
            End = end;
        }

        public long Start { get; }
        public string Name { get; }
        public string Host { get; }
        public long End { get; }
    }

    public class PublishedTime
    {
        public PublishedTime(
            long start,
            long end,
            string hostHandle,
            string record,
            Guid? booked = null
        )
        {
            Start = start;
            End = end;
            HostHandle = hostHandle;
            Record = record;
            Booked = booked;
        }

        public long Start { get; }
        public long End { get; }
        public string HostHandle {get;}
        public string Record { get; }
        public Guid? Booked;
    }

    public class BookedTime
    {
        public BookedTime(
            long start,
            string host,
            long end,
            string record,
            string booker
        )
        {
            Start = start;
            Host = host;
            End = end;
            Record = record;
            Booker = booker;
        }

        public long Start { get; }
        public string Host { get; }
        public long End { get; }
        public string Record { get; }
        public string Booker {get;}
    }

    public class NewHost
    {
        public NewHost(
            string handle,
            string name)
        {
            Name = name;
            Handle = handle;
        }

        public string Name { get; }
        public string Handle {get;}
    }

    public class Conflict
    {
        public Conflict(
            string handle,
            long start,
            long end)
        {
            Handle = handle;
            Start = start;
            End = end;
        }

        public string Handle { get; }
        public long Start {get;}
        public long End {get;}
    }
}