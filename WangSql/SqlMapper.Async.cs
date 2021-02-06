using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace WangSql
{
    /// <summary>
    /// 自动管理数据库连接
    /// </summary>
    public partial class SqlMapper : ISqlMapper
    {
        private async Task OpenConnectionAsync(DbConnection conn)
        {
            if (conn.State == ConnectionState.Closed)
                await conn.OpenAsync();
        }

        public async Task<int> ExecuteAsync(string sql, object param, int? timeout = null)
        {
            var conn = CreateConnection(false);
            try
            {
                var cmd = SqlFactory.CreateCommand(conn, sql, param, CommandType.Text);
                await OpenConnectionAsync(conn);
                return await cmd.ExecuteNonQueryAsync();
            }
            finally
            {
                CloseConnection(conn);
            }
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param, int? timeout = null)
        {
            var conn = CreateConnection(true);
            try
            {
                var cmd = SqlFactory.CreateCommand(conn, sql, param, CommandType.Text);
                using (cmd)
                {
                    await OpenConnectionAsync(conn);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        T result = default(T);
                        if (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            var next = SqlFactory.ResultMap.Deserializer<T>(reader);
                            result = (T)next;
                        }
                        while (await reader.NextResultAsync().ConfigureAwait(false)) { }
                        return result;
                    }
                }
            }
            finally
            {
                CloseConnection(conn);
            }
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param, int? timeout = null)
        {
            var conn = CreateConnection(true);
            try
            {
                var cmd = SqlFactory.CreateCommand(conn, sql, param, CommandType.Text);
                using (cmd)
                {
                    await OpenConnectionAsync(conn);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        IList<T> list = new List<T>();
                        while (await reader.ReadAsync())
                        {
                            var next = SqlFactory.ResultMap.Deserializer<T>(reader);
                            list.Add((T)next);
                        }
                        return list;
                    }
                }
            }
            finally
            {
                CloseConnection(conn);
            }
        }

        public async Task<T> ScalarAsync<T>(string sql, object param, int? timeout = null)
        {
            var conn = CreateConnection(true);
            try
            {
                var cmd = SqlFactory.CreateCommand(conn, sql, param, CommandType.Text);
                using (cmd)
                {
                    await OpenConnectionAsync(conn);
                    var obj = await cmd.ExecuteScalarAsync();
                    var obj1 = TypeMap.ConvertToType(obj, typeof(T));
                    return obj1 == null ? default(T) : (T)obj1;
                }
            }
            finally
            {
                CloseConnection(conn);
            }
        }

        public async Task<DataTable> QueryTableAsync(string sql, object param, string tableName = "p_Out", int? timeout = null)
        {
            DataTable dt = new DataTable();
            dt.TableName = tableName;
            var conn = CreateConnection(true);
            try
            {
                var cmd = SqlFactory.CreateCommand(conn, sql, param, CommandType.Text);
                using (cmd)
                {
                    await OpenConnectionAsync(conn);
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        dt.Load(dr);
                    }
                }
            }
            finally
            {
                CloseConnection(conn);
            }
            return dt;
        }
    }

    /// <summary>
    /// 事务对象
    /// </summary>
    public partial class SqlTrans : ISqlTrans
    {
        public async Task<int> ExecuteAsync(string sql, object param, int? timeout = null)
        {
            var cmd = SqlFactory.CreateCommand(_conn, sql, param, CommandType.Text);
            cmd.Transaction = _trans;
            using (cmd)
            {
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param, int? timeout = null)
        {
            var cmd = SqlFactory.CreateCommand(_conn, sql, param, CommandType.Text);
            using (cmd)
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    T result = default(T);
                    if (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var next = SqlFactory.ResultMap.Deserializer<T>(reader);
                        result = (T)next;
                    }
                    while (await reader.NextResultAsync().ConfigureAwait(false)) { }
                    return result;
                }
            }
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param, int? timeout = null)
        {
            var cmd = SqlFactory.CreateCommand(_conn, sql, param, CommandType.Text);
            cmd.Transaction = _trans;
            using (cmd)
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    IList<T> list = new List<T>();
                    while (await reader.ReadAsync())
                    {
                        var next = SqlFactory.ResultMap.Deserializer<T>(reader);
                        list.Add((T)next);
                    }
                    return list;
                }
            }
        }

        public async Task<T> ScalarAsync<T>(string sql, object param, int? timeout = null)
        {
            var cmd = SqlFactory.CreateCommand(_conn, sql, param, CommandType.Text);
            cmd.Transaction = _trans;
            using (cmd)
            {
                var obj = await cmd.ExecuteScalarAsync();
                var obj1 = TypeMap.ConvertToType(obj, typeof(T));
                return obj1 == null ? default(T) : (T)obj1;
            }
        }

        public async Task<DataTable> QueryTableAsync(string sql, object param, string tableName = "p_Out", int? timeout = null)
        {
            DataTable dt = new DataTable();
            dt.TableName = tableName;
            var cmd = SqlFactory.CreateCommand(_conn, sql, param, CommandType.Text);
            cmd.Transaction = _trans;
            using (cmd)
            {
                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    dt.Load(dr);
                }
            }
            return dt;
        }
    }
}