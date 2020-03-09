using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace PublicCallers.Scheduling
{
    public class PgresUser
    {
        public PgresUser() { }
        public PgresUser(
            string host,
            string port,
            string handle,
            string pwd,
            string db
            )
        {
            Host = host;
            Port = port;
            Handle = handle;
            Pwd = pwd;
            Db = db;
        }
        public string Host { get; set; }
        public string Port { get; set; }
        public string Handle { get; set; }
        public string Pwd { get; set; }
        public string Db { get; set; }

        public override string ToString() =>
            $"Host={Host};Port={Port};Username={Handle};Password={Pwd};Database={Db}";
    }

    public interface IQuery<T>
    {
        string Sql { get; }

        (string n, object v)[] Parameters {get;}
        
        Task<IEnumerable<T>> Read(DbDataReader r);
    }

    public interface IDataSource
    {
        Task<IEnumerable<T>> Submit<T>(
            PgresUser creds,
            IQuery<T> q);
    }
}