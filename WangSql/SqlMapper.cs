using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using WangSql.BuildProviders.Formula;
using WangSql.BuildProviders.Migrate;

namespace WangSql
{
    public interface ISqlExe
    {
        SqlFactory SqlFactory { get; }

        /// <summary>
        /// 方便调用
        /// </summary>
        IFormulaProvider Formula { get; }

        /// <summary>
        ///     SQL执行
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="param">参数（Dictionary、Simple、Class）</param>
        /// <returns></returns>
        int Execute(string sql, object param);

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="sql"></param>
        ///// <returns></returns>
        //int Execute(SqlInfomation sql);
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="sql"></param>
        ///// <returns></returns>
        //int Execute(IEnumerable<SqlInfomation> sql);

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
        ///     SQL查询（分页不返回总数）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IEnumerable<T> Query<T>(string sql, object param, int pageIndex, int pageSize);

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
        DataTable QueryTable(string sql, object param, string tableName = "p_Out");

    }

    public interface ISqlMapper : ISqlExe
    {
        /// <summary>
        /// 数据迁移
        /// </summary>
        IMigrateProvider Migrate { get; }

        /// <summary>
        /// 手动维护事务
        /// </summary>
        /// <returns></returns>
        ISqlTrans BeginTransaction();

        string GetSqlTag(string sql, object param);

        //
        // 直接内置工作单元，自动维护事务（同一个进程内）
        //

        void BeginUnitWork();
        void CommitUnitWork();
        void RollbackUnitWork();
    }

    public interface ISqlTrans : ISqlExe, IDisposable
    {
        void Commit();

        void Rollback();
    }

    public interface ISqlMapperManual : ISqlExe
    {
        void OpenConn();
        void CloseConn();
        ISqlTrans BeginTransaction();
    }

    public class SqlFactory
    {
        private readonly string _name;

        public DbProvider DbProvider { get { return DbProviderManager.Get(_name); } }

        public ParamMap ParamMap { get; }

        public ResultMap ResultMap { get; }

        public SqlFactory()
        {
            ParamMap = new ParamMap();
            ResultMap = new ResultMap();
        }

        public SqlFactory(string name)
        {
            _name = name;
            ParamMap = new ParamMap();
            ResultMap = new ResultMap();
        }

        public IDbCommand CreateCommand(IDbConnection conn, string sql, object param, CommandType commandType, int? timeout = null)
        {
            //调试日志
            if (DbProvider.Debug)
            {
                Debug.WriteLine("处理前SQL:" + sql);
            }
            var cmd = conn.CreateCommand();
            if (timeout != null) cmd.CommandTimeout = (int)timeout;
            ParamMap.GetCacheMap(DbProvider, sql).Prepare(cmd, param, commandType);
            //调试日志
            if (DbProvider.Debug)
            {
                Debug.WriteLine("处理后SQL:" + cmd.CommandText);
                StringBuilder sb = new StringBuilder();
                var ps = cmd.Parameters;
                if (ps != null && ps.Count > 0)
                {
                    foreach (IDbDataParameter item in ps)
                    {
                        sb.Append($"\"{item.ParameterName}\":\"{item.Value?.ToString()}\",");
                    }
                }
                Debug.WriteLine("处理后参数:" + "{" + sb.ToString().TrimEnd(',') + "}");
            }
            return cmd;
        }

        public IDbConnection CreateConnection(bool isReadDb)
        {
            var conn = DbProvider.CreateConnection(isReadDb);
            return conn;
        }
    }


    /// <summary>
    /// 自动管理数据库连接
    /// </summary>
    public class SqlMapper : ISqlMapper
    {
        private IDbConnection _conn;
        private IDbTransaction _trans;
        private bool _isUnitWork;
        private bool _disposed;

        private IDbConnection CreateConnection(bool isReadDb)
        {
            return _isUnitWork ? _conn : SqlFactory.CreateConnection(isReadDb);
        }
        private void OpenConnection(IDbConnection conn)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();
        }
        private void CloseConnection(IDbConnection conn)
        {
            if (conn.State != ConnectionState.Closed)
                conn.Close();
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

        public IFormulaProvider Formula { get { return SqlFactory.DbProvider.FormulaProvider; } }

        public IMigrateProvider Migrate { get { return SqlFactory.DbProvider.MigrateProvider.Instance(this); } }

        public ISqlTrans BeginTransaction()
        {
            return new SqlTrans(SqlFactory);
        }

        public int Execute(string sql, object param)
        {
            var conn = CreateConnection(false);
            try
            {
                var cmd = SqlFactory.CreateCommand(conn, sql, param, CommandType.Text);
                if (_isUnitWork)
                {
                    cmd.Transaction = _trans;
                }
                using (cmd)
                {
                    OpenConnection(conn);
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (!_isUnitWork)
                {
                    Dispose();
                }
            }
        }

        [Obsolete("仅开发调试使用，正式环境禁用")]
        public string GetSqlTag(string sql, object param)
        {
            var conn = SqlFactory.CreateConnection(true);
            var cmd = SqlFactory.CreateCommand(conn, sql, param, CommandType.Text);
            SqlFactory.ParamMap.GetCacheMap(SqlFactory.DbProvider, sql).Prepare(cmd, param, CommandType.Text);
            return cmd.CommandText;
        }

        public T QueryFirstOrDefault<T>(string sql, object param)
        {
            return SqlFactory.DbProvider.PageProvider.Instance(this).BuildPageSql<T>(sql, param, 1, 1).FirstOrDefault();
        }

        public IEnumerable<T> Query<T>(string sql, object param)
        {
            var conn = CreateConnection(true);
            try
            {
                var cmd = SqlFactory.CreateCommand(conn, sql, param, CommandType.Text);
                if (_isUnitWork)
                {
                    cmd.Transaction = _trans;
                }
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
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (!_isUnitWork)
                {
                    Dispose();
                }
            }
        }

        public IEnumerable<T> Query<T>(string sql, object param, int pageIndex, int pageSize)
        {
            var rr = SqlFactory.DbProvider.PageProvider.Instance(this).BuildPageSql<T>(sql, param, pageIndex, pageSize);
            return rr;
        }

        public IEnumerable<T> Query<T>(string sql, object param, int pageIndex, int pageSize, out int total)
        {
            total = SqlFactory.DbProvider.PageProvider.Instance(this).BuildPageCountSql<T>(sql, param);
            var rr = SqlFactory.DbProvider.PageProvider.Instance(this).BuildPageSql<T>(sql, param, pageIndex, pageSize);
            return rr;
        }

        public T Scalar<T>(string sql, object param)
        {
            var conn = CreateConnection(true);
            try
            {
                var cmd = SqlFactory.CreateCommand(conn, sql, param, CommandType.Text);
                if (_isUnitWork)
                {
                    cmd.Transaction = _trans;
                }
                using (cmd)
                {
                    OpenConnection(conn);
                    var obj = cmd.ExecuteScalar();
                    var obj1 = TypeMap.ConvertToType(obj, typeof(T));
                    return obj1 == null ? default(T) : (T)obj1;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (!_isUnitWork)
                {
                    Dispose();
                }
            }
        }

        public DataTable QueryTable(string sql, object param, string tableName = "p_Out")
        {
            DataTable dt = new DataTable();
            dt.TableName = tableName;
            var conn = CreateConnection(true);
            try
            {
                var cmd = SqlFactory.CreateCommand(conn, sql, param, CommandType.Text);
                if (_isUnitWork)
                {
                    cmd.Transaction = _trans;
                }
                using (cmd)
                {
                    OpenConnection(conn);
                    using (var dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (!_isUnitWork)
                {
                    Dispose();
                }
            }
            return dt;
        }

        public void BeginUnitWork()
        {
            _conn = SqlFactory.CreateConnection(false);
            _conn.Open();
            _trans = _conn.BeginTransaction();
            _isUnitWork = true;
        }

        public void CommitUnitWork()
        {
            if (_isUnitWork)
            {
                _isUnitWork = false;
                _trans.Commit();
                Dispose();
            }
        }

        public void RollbackUnitWork()
        {
            if (_isUnitWork)
            {
                _isUnitWork = false;
                _trans.Rollback();
                Dispose();
            }
        }

        private void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                if (_trans != null)
                {
                    _trans.Dispose();
                }
                if (_conn != null)
                {
                    if (_conn.State != ConnectionState.Closed)
                        _conn.Close();
                    _conn.Dispose();
                }
            }
        }
    }

    /// <summary>
    /// 事务对象
    /// </summary>
    public class SqlTrans : ISqlTrans
    {
        private readonly IDbConnection _conn;
        private readonly IDbTransaction _trans;
        private bool _disposed;

        public SqlTrans(SqlFactory sqlFactory)
        {
            SqlFactory = sqlFactory;
            _conn = SqlFactory.CreateConnection(false);
            _conn.Open();
            _trans = _conn.BeginTransaction();
        }

        public SqlFactory SqlFactory { get; }

        public IFormulaProvider Formula { get { return SqlFactory.DbProvider.FormulaProvider; } }

        public void Commit()
        {
            _trans.Commit();
            Dispose();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                if (_trans != null)
                {
                    _trans.Dispose();
                }
                if (_conn != null)
                {
                    if (_conn.State != ConnectionState.Closed)
                        _conn.Close();
                    _conn.Dispose();
                }
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
            return SqlFactory.DbProvider.PageProvider.Instance(this).BuildPageSql<T>(sql, param, 1, 1).FirstOrDefault();
        }

        public IEnumerable<T> Query<T>(string sql, object param)
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

        public IEnumerable<T> Query<T>(string sql, object param, int pageIndex, int pageSize)
        {
            var rr = SqlFactory.DbProvider.PageProvider.Instance(this).BuildPageSql<T>(sql, param, pageIndex, pageSize);
            return rr;
        }

        public IEnumerable<T> Query<T>(string sql, object param, int pageIndex, int pageSize, out int total)
        {
            total = SqlFactory.DbProvider.PageProvider.Instance(this).BuildPageCountSql<T>(sql, param);
            var rr = SqlFactory.DbProvider.PageProvider.Instance(this).BuildPageSql<T>(sql, param, pageIndex, pageSize);
            return rr;
        }

        public void Rollback()
        {
            _trans.Rollback();
            Dispose();
        }

        public T Scalar<T>(string sql, object param)
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

        public DataTable QueryTable(string sql, object param, string tableName = "p_Out")
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