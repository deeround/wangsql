using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

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
        Task<int> ExecuteAsync(string sql, object param);

        /// <summary>
        ///      SQL查询单条
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        T QueryFirstOrDefault<T>(string sql, object param);
        Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param);

        /// <summary>
        ///     SQL查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="param">参数（Dictionary、Simple、Class）（注意：枚举一律会转换成字符串处理）</param>
        /// <returns></returns>
        IEnumerable<T> Query<T>(string sql, object param);
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object param);

        /// <summary>
        ///     返回值为结果集中第一行的第一列或空引用（如果结果集为空）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="param">参数（Dictionary、Simple、Class）（注意：枚举一律会转换成字符串处理）</param>
        /// <returns></returns>
        T Scalar<T>(string sql, object param);
        Task<T> ScalarAsync<T>(string sql, object param);

        /// <summary>
        ///     SQL查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="param">参数（Dictionary、Simple、Class）（注意：枚举一律会转换成字符串处理）</param>
        /// <returns></returns>
        DataTable QueryTable(string sql, object param, string tableName = "p_Out");
        Task<DataTable> QueryTableAsync(string sql, object param, string tableName = "p_Out");

    }

    public interface ISqlMapper : ISqlExe
    {
        /// <summary>
        /// 手动维护事务
        /// </summary>
        /// <returns></returns>
        ISqlTrans BeginTransaction();
    }

    public interface ISqlTrans : ISqlExe, IDisposable
    {
        void Commit();

        void Rollback();
    }
}
