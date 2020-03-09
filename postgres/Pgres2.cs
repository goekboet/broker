using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using PublicCallers.Scheduling;

namespace postgres.Pgres2
{
    public class PgresDb : IDataSource
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
    }
}