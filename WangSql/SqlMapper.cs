using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace WangSql
{
    /// <summary>
    /// 自动管理数据库连接
    /// </summary>
    public partial class SqlMapper : ISqlMapper
    {
        private DbConnection CreateConnection(bool isReadDb)
        {
            return SqlFactory.CreateConnection(isReadDb);
        }
        private void OpenConnection(DbConnection conn)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();
        }
        private void CloseConnection(DbConnection conn)
        {
            if (conn != null)
            {
                try
                {
                    if (conn.State != ConnectionState.Closed)
                        conn.Close();
                }
                catch
                {
                }
                finally
                {
                    conn.Dispose();
                }
            }
        }

        public SqlMapper()
        {
            SqlFactory = new SqlFactory();
        }

        public SqlMapper(string name)
        {
            SqlFactory = new SqlFactory(name);
        }

        public SqlFactory SqlFactory { get; }

        public ISqlTrans BeginTransaction()
        {
            return new SqlTrans(SqlFactory);
        }

        public int Execute(string sql, object param, int? timeout = null)
        {
            var conn = CreateConnection(false);
            try
            {
                var cmd = SqlFactory.CreateCommand(conn, sql, param, CommandType.Text);
                OpenConnection(conn);
                return cmd.ExecuteNonQuery();
            }
            finally
            {
                CloseConnection(conn);
            }
        }

        public T QueryFirstOrDefault<T>(string sql, object param, int? timeout = null)
        {
            var conn = CreateConnection(true);
            try
            {
                var cmd = SqlFactory.CreateCommand(conn, sql, param, CommandType.Text);
                using (cmd)
                {
                    OpenConnection(conn);
                    using (var reader = cmd.ExecuteReader())
                    {
                        T result = default(T);
                        if (reader.Read())
                        {
                            var next = SqlFactory.ResultMap.Deserializer<T>(reader);
                            result = (T)next;
                        }
                        while (reader.NextResult()) { }
                        return result;
                    }
                }
            }
            finally
            {
                CloseConnection(conn);
            }
        }

        public IEnumerable<T> Query<T>(string sql, object param, int? timeout = null)
        {
            var conn = CreateConnection(true);
            try
            {
                var cmd = SqlFactory.CreateCommand(conn, sql, param, CommandType.Text);
                using (cmd)
                {
                    OpenConnection(conn);
                    using (var reader = cmd.ExecuteReader())
                    {
                        IList<T> list = new List<T>();
                        while (reader.Read())
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

        public T Scalar<T>(string sql, object param, int? timeout = null)
        {
            var conn = CreateConnection(true);
            try
            {
                var cmd = SqlFactory.CreateCommand(conn, sql, param, CommandType.Text);
                using (cmd)
                {
                    OpenConnection(conn);
                    var obj = cmd.ExecuteScalar();
                    var obj1 = TypeMap.ConvertToType(obj, typeof(T));
                    return obj1 == null ? default(T) : (T)obj1;
                }
            }
            finally
            {
                CloseConnection(conn);
            }
        }

        public DataTable QueryTable(string sql, object param, string tableName = "p_Out", int? timeout = null)
        {
            DataTable dt = new DataTable();
            dt.TableName = tableName;
            var conn = CreateConnection(true);
            try
            {
                var cmd = SqlFactory.CreateCommand(conn, sql, param, CommandType.Text);
                using (cmd)
                {
                    OpenConnection(conn);
                    using (var dr = cmd.ExecuteReader())
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
        private readonly DbConnection _conn;
        private readonly DbTransaction _trans;

        public SqlTrans(SqlFactory sqlFactory)
        {
            SqlFactory = sqlFactory;
            _conn = SqlFactory.CreateConnection(false);
            _conn.Open();
            _trans = _conn.BeginTransaction();
        }

        public SqlFactory SqlFactory { get; }

        public void Commit()
        {
            _trans.Commit();
            Dispose();
        }

        public void Rollback()
        {
            _trans.Rollback();
            Dispose();
        }

        public void Dispose()
        {
            if (_trans != null)
            {
                _trans.Dispose();
            }
            if (_conn != null)
            {
                try
                {
                    if (_conn.State != ConnectionState.Closed)
                        _conn.Close();
                }
                catch
                {
                }
                finally
                {
                    _conn.Dispose();
                }
            }
        }

        public int Execute(string sql, object param, int? timeout = null)
        {
            var cmd = SqlFactory.CreateCommand(_conn, sql, param, CommandType.Text);
            cmd.Transaction = _trans;
            using (cmd)
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public T QueryFirstOrDefault<T>(string sql, object param, int? timeout = null)
        {
            var cmd = SqlFactory.CreateCommand(_conn, sql, param, CommandType.Text);
            using (cmd)
            {
                using (var reader = cmd.ExecuteReader())
                {
                    T result = default(T);
                    if (reader.Read())
                    {
                        var next = SqlFactory.ResultMap.Deserializer<T>(reader);
                        result = (T)next;
                    }
                    while (reader.NextResult()) { }
                    return result;
                }
            }
        }

        public IEnumerable<T> Query<T>(string sql, object param, int? timeout = null)
        {
            var cmd = SqlFactory.CreateCommand(_conn, sql, param, CommandType.Text);
            cmd.Transaction = _trans;
            using (cmd)
            {
                using (var reader = cmd.ExecuteReader())
                {
                    IList<T> list = new List<T>();
                    while (reader.Read())
                    {
                        var next = SqlFactory.ResultMap.Deserializer<T>(reader);
                        list.Add((T)next);
                    }
                    return list;
                }
            }
        }

        public T Scalar<T>(string sql, object param, int? timeout = null)
        {
            var cmd = SqlFactory.CreateCommand(_conn, sql, param, CommandType.Text);
            cmd.Transaction = _trans;
            using (cmd)
            {
                var obj = cmd.ExecuteScalar();
                var obj1 = TypeMap.ConvertToType(obj, typeof(T));
                return obj1 == null ? default(T) : (T)obj1;
            }
        }

        public DataTable QueryTable(string sql, object param, string tableName = "p_Out", int? timeout = null)
        {
            DataTable dt = new DataTable();
            dt.TableName = tableName;
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

}