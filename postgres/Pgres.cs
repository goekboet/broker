﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;

namespace postgres
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

    public abstract class SqlResult<T> { }

    public sealed class Success<T> : SqlResult<T>
    {
        public Success(T r)
        {
            Result = r;
        }
        public T Result { get; }
    }

    public sealed class Fail<T> : SqlResult<T>
    {
        public Fail(PostgresException e)
        {
            Exception = e;
        }
        public PostgresException Exception { get; }
    }

    public static class PGres
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

        public static async Task<T> SubmitSingleQuery<T>(
            this NpgsqlCommand cmd,
            Func<IDataRecord, T> selector)
        {
            var r = default(T);
            using (cmd)
            {
                await cmd.Connection.OpenAsync();
                using (var rd = await cmd.ExecuteReaderAsync())
                {
                    if (await rd.ReadAsync())
                    {
                        r = selector(rd);
                    }
                }
            }

            return r;
        }

        public static async Task<SqlResult<T>> SubmitCommandReturning<T>(
            this NpgsqlCommand cmd,
            Func<IDataRecord, T> selector)
        {
            var r = default(T);
            try
            {
                using (cmd)
                {
                    await cmd.Connection.OpenAsync();
                    using (var rd = await cmd.ExecuteReaderAsync())
                    {
                        if (await rd.ReadAsync())
                        {
                            r = selector(rd);
                        }
                    }
                }
            }
            catch (PostgresException e)
            {
                return new Fail<T>(e);
            }

            return new Success<T>(r);
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
