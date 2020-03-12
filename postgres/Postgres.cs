using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using postgres.Pgres2;
using PublicCallers.Scheduling;

namespace postgres.Pgres2
{
    public class Postgres : IDataSource
    {
        public async Task<IEnumerable<T>> Submit<T>(
            PgresUser creds,
            IQuery<T> q)
        {
            using var c = creds.ToConnection();
            using var cmd = new NpgsqlCommand(q.Sql, c);
            cmd.Parameters.AddMany(q.Parameters);

            IEnumerable<T> r = Enumerable.Empty<T>();
            await c.OpenAsync();

            using var rd = await cmd.ExecuteReaderAsync();
            r = await q.Read(rd);

            return r;
        }

        public async Task<int> SubmitCommand(
            PgresUser creds,
            ICommand command)
        {
            using var c = creds.ToConnection();
            using var cmd = new NpgsqlCommand(
                command.Sql, c);
            cmd.Parameters.AddMany(command.Parameters);

            await c.OpenAsync();
            var r = await cmd.ExecuteNonQueryAsync();

            return r;
        }
    }
}