using System;
using Npgsql;

namespace postgres
{
    public static class AddPublisherExtensions
    {
        private static string Sql { get; } = string.Join("\n", new[]
            {
                "insert into hosts (sub, handle)",
                "values(@sub, @handle)",
                "on conflict (sub) do nothing"
            });

        public static NpgsqlCommand AddPublisher(
            this NpgsqlConnection c,
            Guid sub,
            string name
        )
        {
            var cmd = new NpgsqlCommand(Sql, c);
            cmd.Parameters.AddMany(
                new (string n, object v)[]
                {
                    ("sub", sub),
                    ("handle", name),
                });

            return cmd;
        }
    }
}