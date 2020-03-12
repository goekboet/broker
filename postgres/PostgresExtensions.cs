using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using PublicCallers.Scheduling;

namespace postgres
{
    public static class PostgresExensions
    {
        public static async Task<IEnumerable<S>> SubmitQuery<S>(
            this NpgsqlCommand cmd,
            Func<IDataRecord, S> selector)
        {
            var rs = new List<S>();

            using (cmd)
            {
                await cmd.Connection.OpenAsync();
                using (var rd = await cmd.ExecuteReaderAsync())
                {
                    while (await rd.ReadAsync())
                    {
                        rs.Add(selector(rd));
                    }
                }
            }

            return rs;
        }

        public static async Task<int> SubmitCommand(
            this NpgsqlCommand cmd)
        {
            var a = 0;
            using (cmd)
            {
                await cmd.Connection.OpenAsync();
                a = await cmd.ExecuteNonQueryAsync();
            }

            return a;
        }

        static string PoolingKVP => ";Pooling=false";
        public static NpgsqlConnection ToConnection(
            this PgresUser u,
            bool pooling = true) =>
            new NpgsqlConnection($"{u.ToString()}{(pooling ? string.Empty : PoolingKVP)}");

        public static NpgsqlParameterCollection AddMany(
            this NpgsqlParameterCollection c,
            IEnumerable<(string n, object v)> ps
        ) => ps
            .Select(x => new NpgsqlParameter(x.n, x.v))
            .Aggregate(c, (acc, x) =>
            {
                acc.Add(x);
                return acc;
            });
    }
}
