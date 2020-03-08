using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace wordlists
{
    class Program
    {
        static (List<string> first, List<string> last) GetNames(string[] names) =>
            names.Aggregate(
                (first: new List<string>(),last: new List<string>()), 
                (acc, x) => 
                { 
                    var ss = x.Split(' ');
                    acc.first.Add(ss[0]);
                    acc.last.Add(ss[1]);
                    
                    return acc;
                });

        private static Random Rng {get;} = new Random();

        private static T PullOne<T>(List<T> xs) =>
            xs[Rng.Next(xs.Count - 1)];

        public static void Shuffle<T>(IList<T> list)  
        {  
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = Rng.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  
        }
        static void Main(string[] args)
        {
            var (firstNames, lastNames) = GetNames(File.ReadAllLines("names.txt"));
            var nouns = File.ReadAllLines("nouns.txt").ToList();
            var adjectives = File.ReadAllLines("adjectives.txt").ToList();
            var handles = (from ad in adjectives
                          from ns in nouns
                          select $"{ad}_{ns}").ToList();

            Shuffle(handles);

            var r = from i in Enumerable.Range(0, 999)
                select new 
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"{PullOne(firstNames)} {PullOne(lastNames)}",
                    Handle = handles[i]
                };

            var csv = string.Join(
                '\n', 
                from h in r
                select $"{h.Id},{h.Handle},{h.Name}");

            Console.WriteLine(csv);
            
            // Console.WriteLine($"firstnames: {firstNames.Count}");
            // Console.WriteLine(string.Join('\n', firstNames.Take(5)));
            // Console.WriteLine();
            // Console.WriteLine($"lastnames: {lastNames.Count}");
            // Console.WriteLine(string.Join('\n', lastNames.Take(5)));
            // Console.WriteLine();
            // Console.WriteLine($"nouns: {nouns.Count}");
            // Console.WriteLine(string.Join('\n', nouns.Take(5)));
            // Console.WriteLine();
            // Console.WriteLine($"adjectives: {adjectives.Count}");
            // Console.WriteLine(string.Join('\n', adjectives.Take(5)));
        }
    }
}
