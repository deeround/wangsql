using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace WangSql
{
    public interface ISqlExe
    {
        SqlFactory SqlFactory { get; }

        /// <summary>
        ///     SQL执行
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="param">参数（Dictionary、Simple、Class）</param>
        /// <returns></returns>
        int Execute(string sql, object param);

        /// <summary>
        ///      SQL查询单条
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        T QueryFirstOrDefault<T>(string sql, object param);

        /// <summary>
        ///     SQL查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="param">参数（Dictionary、Simple、Class）（注意：枚举一律会转换成字符串处理）</param>
        /// <returns></returns>
        IEnumerable<T> Query<T>(string sql, object param);

        /// <summary>
        ///     SQL查询（分页）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        IEnumerable<T> Query<T>(string sql, object param, int pageIndex, int pageSize, out int total);

        /// <summary>
        ///     返回值为结果集中第一行的第一列或空引用（如果结果集为空）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="param">参数（Dictionary、Simple、Class）（注意：枚举一律会转换成字符串处理）</param>
        /// <returns></returns>
        T Scalar<T>(string sql, object param);

        /// <summary>
        ///     SQL查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="param">参数（Dictionary、Simple、Class）（注意：枚举一律会转换成字符串处理）</param>
        /// <returns></returns>
        DataTable QueryTable(string sql, object param);
    }

    public interface ISqlMapper : ISqlExe
    {
        ISqlTrans BeginTransaction();

        string GetSqlTag(string sql, object param);
    }

    public interface ISqlTrans : ISqlExe, IDisposable
    {
        void Commit();

        void Rollback();
    }

    public interface ISqlMapperManual : ISqlMapper
    {
        void OpenConn();
        void CloseConn();
    }

    public class SqlFactory
    {
        public DbProvider DbProvider;
        public ParamMap ParamMap;
        public ResultMap ResultMap;

        public SqlFactory(DbProvider dbProvider, ResultMap resultMap, ParamMap paramMap)
        {
            DbProvider = dbProvider;
            ResultMap = resultMap;
            ParamMap = paramMap;
        }

        public IDbCommand CreateCommand(IDbConnection conn, string sql, object param, CommandType commandType, int? timeout = null)
        {
            //调试日志
            if (DbProvider.Debug)
            {
                Console.WriteLine("处理前SQL:" + sql);
            }
            var cmd = conn.CreateCommand();
            if (timeout != null) cmd.CommandTimeout = (int)timeout;
            ParamMap.GetCacheMap(DbProvider, sql).Prepare(cmd, param, commandType);
            //调试日志
            if (DbProvider.Debug)
            {
                Console.WriteLine("处理后SQL:" + cmd.CommandText);
                StringBuilder sb = new StringBuilder();
                var ps = cmd.Parameters;
                if (ps != null && ps.Count > 0)
                {
                    foreach (IDbDataParameter item in ps)
                    {
                        sb.Append($"\"{item.ParameterName}\":\"{item.Value?.ToString()}\",");
                    }
                }
                Console.WriteLine("处理后参数:" + "{" + sb.ToString().TrimEnd(',') + "}");
            }
            return cmd;
        }

        public IDbConnection CreateConnection()
        {
            var conn = DbProvider.CreateConnection();
            return conn;
        }
    }

    public class SqlMapper : ISqlMapper
    {
        public SqlMapper(string name, string connectionString, string connectionType, bool useParameterPrefixInSql, bool useParameterPrefixInParameter, string parameterPrefix, bool useQuotationInSql = false, bool debug = false)
        {
            var dbProvider = new DbProvider(name, connectionString, connectionType, useParameterPrefixInSql, useParameterPrefixInParameter, parameterPrefix, useQuotationInSql, debug);
            var resultMap = new ResultMap();
            var paramMap = new ParamMap();
            SqlFactory = new SqlFactory(dbProvider, resultMap, paramMap);
        }

        public SqlFactory SqlFactory { get; private set; }

        public ISqlTrans BeginTransaction()
        {
            return new SqlTrans(SqlFactory);
        }

        public int Execute(string sql, object param)
        {
            var conn = SqlFactory.CreateConnection();
            using (conn)
            {
                var cmd = SqlFactory.CreateCommand(conn, sql, param, CommandType.Text);
                using (cmd)
                {
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        [Obsolete("仅开发调试使用")]
        public string GetSqlTag(string sql, object param)
        {
            var conn = SqlFactory.CreateConnection();
            var cmd = SqlFactory.CreateCommand(conn, sql, param, CommandType.Text);
            SqlFactory.ParamMap.GetCacheMap(SqlFactory.DbProvider, sql).Prepare(cmd, param, CommandType.Text);
            return cmd.CommandText;
        }

        public T QueryFirstOrDefault<T>(string sql, object param)
        {
            var conn = SqlFactory.CreateConnection();
            using (conn)
            {
                var cmd = SqlFactory.CreateCommand(conn, sql, param, CommandType.Text);
                using (cmd)
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var next = SqlFactory.ResultMap.Deserializer<T>(reader);
                            return (T)next;
                        }
                    }
                }
            }
            return default(T);
        }

        public IEnumerable<T> Query<T>(string sql, object param)
        {
            var conn = SqlFactory.CreateConnection();
            using (conn)
            {
                var cmd = SqlFactory.CreateCommand(conn, sql, param, CommandType.Text);
                using (cmd)
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var next = SqlFactory.ResultMap.Deserializer<T>(reader);
                            yield return (T)next;
                        }
                    }
                }
            }
        }

        public IEnumerable<T> Query<T>(string sql, object param, int pageIndex, int pageSize, out int total)
        {
            total = SqlFactory.DbProvider.BuildProvider.BuildPageCountSql<T>(this, sql, param);
            var rr = SqlFactory.DbProvider.BuildProvider.BuildPageSql<T>(this, sql, param, pageIndex, pageSize);
            return rr;
        }

        public T Scalar<T>(string sql, object param)
        {
            var conn = SqlFactory.CreateConnection();
            using (conn)
            {
                var cmd = SqlFactory.CreateCommand(conn, sql, param, CommandType.Text);
                using (cmd)
                {
                    conn.Open();
                    var obj = cmd.ExecuteScalar();
                    var obj1 = TypeMap.ConvertToType(obj, typeof(T));
                    return (T)obj1;
                }
            }
        }

        public DataTable QueryTable(string sql, object param)
        {
            DataTable dt = new DataTable();
            var conn = SqlFactory.CreateConnection();
            using (conn)
            {
                var cmd = SqlFactory.CreateCommand(conn, sql, param, CommandType.Text);
                using (cmd)
                {
                    conn.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }
    }

    public class SqlTrans : ISqlTrans
    {
        private readonly IDbConnection _conn;
        private readonly IDbTransaction _trans;

        public SqlTrans(SqlFactory sqlFactory)
        {
            SqlFactory = sqlFactory;
            _conn = SqlFactory.CreateConnection();
            _conn.Open();
            _trans = _conn.BeginTransaction();
        }

        public SqlFactory SqlFactory { get; private set; }

        public void Commit()
        {
            _trans?.Commit();
        }

        public void Dispose()
        {
            _trans?.Dispose();
            if (_conn != null)
            {
                if (_conn.State != ConnectionState.Closed)
                    _conn.Close();
                _conn.Dispose();
            }
        }

        public int Execute(string sql, object param)
        {
            var cmd = SqlFactory.CreateCommand(_conn, sql, param, CommandType.Text);
            cmd.Transaction = _trans;
            using (cmd)
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public T QueryFirstOrDefault<T>(string sql, object param)
        {
            var cmd = SqlFactory.CreateCommand(_conn, sql, param, CommandType.Text);
            cmd.Transaction = _trans;
            using (cmd)
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var next = SqlFactory.ResultMap.Deserializer<T>(reader);
                        return (T)next;
                    }
                }
            }
            return default(T);
        }

        public IEnumerable<T> Query<T>(string sql, object param)
        {
            var cmd = SqlFactory.CreateCommand(_conn, sql, param, CommandType.Text);
            cmd.Transaction = _trans;
            using (cmd)
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var next = SqlFactory.ResultMap.Deserializer<T>(reader);
                        yield return (T)next;
                    }
                }
            }
        }

        public IEnumerable<T> Query<T>(string sql, object param, int pageIndex, int pageSize, out int total)
        {
            total = SqlFactory.DbProvider.BuildProvider.BuildPageCountSql<T>(this, sql, param);
            var rr = SqlFactory.DbProvider.BuildProvider.BuildPageSql<T>(this, sql, param, pageIndex, pageSize);
            return rr;
        }

        public void Rollback()
        {
            _trans?.Rollback();
        }

        public T Scalar<T>(string sql, object param)
        {
            var cmd = SqlFactory.CreateCommand(_conn, sql, param, CommandType.Text);
            cmd.Transaction = _trans;
            using (cmd)
            {
                var obj = cmd.ExecuteScalar();
                var obj1 = TypeMap.ConvertToType(obj, typeof(T));
                return (T)obj1;
            }
        }

        public DataTable QueryTable(string sql, object param)
        {
            DataTable dt = new DataTable();
            var cmd = SqlFactory.CreateCommand(_conn, sql, param, CommandType.Text);
            cmd.Transaction = _trans;
            using (cmd)
            {
                using (var dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }
            }
            return dt;
        }
    }

    public class SqlMapperManual : ISqlMapperManual
    {
        private IDbConnection _conn = null;
        private int _commandTimeout = 60;//秒(默认是30秒)
        private int _connTimeout = 60;//10分钟
        private DateTime _connOpenTime = DateTime.Now;

        private void CheckConnTime()
        {
            var ts = (DateTime.Now - _connOpenTime).TotalSeconds;
            if (ts > _connTimeout)//数据库连接超时，重新打开
            {
                OpenConn();
            }
        }

        public SqlMapperManual(string name, string connectionString, string connectionType, bool useParameterPrefixInSql, bool useParameterPrefixInParameter, string parameterPrefix, bool useQuotationInSql = false, bool debug = false)
        {
            var dbProvider = new DbProvider(name, connectionString, connectionType, useParameterPrefixInSql, useParameterPrefixInParameter, parameterPrefix, useQuotationInSql, debug);
            var resultMap = new ResultMap();
            var paramMap = new ParamMap();
            SqlFactory = new SqlFactory(dbProvider, resultMap, paramMap);
        }

        public SqlFactory SqlFactory { get; private set; }

        public ISqlTrans BeginTransaction()
        {
            return new SqlTrans(SqlFactory);
        }

        public void OpenConn()
        {
            CloseConn();

            if (_conn == null)
                _conn = SqlFactory.CreateConnection();
            if (_conn.State != ConnectionState.Open)
            {
                _conn.Open();
                _connOpenTime = DateTime.Now;
            }
        }

        public void CloseConn()
        {
            try
            {
                if (_conn != null && _conn.State != ConnectionState.Closed)
                {
                    _conn.Close();
                }
                if (_conn != null)
                {
                    _conn.Dispose();
                }
            }
            catch
            {

            }
            finally
            {
                _conn = null;
            }
        }

        public int Execute(string sql, object param)
        {
            CheckConnTime();
            var cmd = SqlFactory.CreateCommand(_conn, sql, param, CommandType.Text, _commandTimeout);
            using (cmd)
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public T QueryFirstOrDefault<T>(string sql, object param)
        {
            CheckConnTime();
            var cmd = SqlFactory.CreateCommand(_conn, sql, param, CommandType.Text, _commandTimeout);
            using (cmd)
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var next = SqlFactory.ResultMap.Deserializer<T>(reader);
                        return (T)next;
                    }
                }
            }
            return default(T);
        }

        [Obsolete("仅开发调试使用")]
        public string GetSqlTag(string sql, object param)
        {
            var conn = SqlFactory.CreateConnection();
            var cmd = SqlFactory.CreateCommand(conn, sql, param, CommandType.Text);
            SqlFactory.ParamMap.GetCacheMap(SqlFactory.DbProvider, sql).Prepare(cmd, param, CommandType.Text);
            return cmd.CommandText;
        }

        public IEnumerable<T> Query<T>(string sql, object param)
        {
            CheckConnTime();
            var cmd = SqlFactory.CreateCommand(_conn, sql, param, CommandType.Text, _commandTimeout);
            using (cmd)
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var next = SqlFactory.ResultMap.Deserializer<T>(reader);
                        yield return (T)next;
                    }
                }
            }
        }

        public IEnumerable<T> Query<T>(string sql, object param, int pageIndex, int pageSize, out int total)
        {
            CheckConnTime();
            total = SqlFactory.DbProvider.BuildProvider.BuildPageCountSql<T>(this, sql, param);
            var rr = SqlFactory.DbProvider.BuildProvider.BuildPageSql<T>(this, sql, param, pageIndex, pageSize);
            return rr;
        }

        public T Scalar<T>(string sql, object param)
        {
            CheckConnTime();
            var cmd = SqlFactory.CreateCommand(_conn, sql, param, CommandType.Text, _commandTimeout);
            using (cmd)
            {
                var obj = cmd.ExecuteScalar();
                var obj1 = TypeMap.ConvertToType(obj, typeof(T));
                return (T)obj1;
            }
        }

        public DataTable QueryTable(string sql, object param)
        {
            CheckConnTime();
            DataTable dt = new DataTable();
            var cmd = SqlFactory.CreateCommand(_conn, sql, param, CommandType.Text, _commandTimeout);
            using (cmd)
            {
                using (var dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }
            }
            return dt;
        }
    }
}