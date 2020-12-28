using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace WangSql.Abstract.Linq
{
    public class SqlBuilder
    {
        public string Sql { get; set; }
        public Dictionary<string, object> Parameter { get; set; }
    }
    public interface ISqlProvider<T> : IProvider
    {
        #region interface
        ISqlProvider<T> With(string lockType);
        ISqlProvider<T> With(LockType lockType);
        ISqlProvider<T> Where(string expression, Action<Dictionary<string, object>> action = null);
        ISqlProvider<T> Where(Expression<Func<T, bool>> expression);
        ISqlProvider<T> WhereIf(bool where, Expression<Func<T, bool>> expression);
        ISqlProvider<T> WhereIf(bool where, Expression<Func<T, bool>> expressionTrue, Expression<Func<T, bool>> expressionFalse);
        ISqlProvider<T> Filter<TResult>(Expression<Func<T, TResult>> columns);


        ISqlProvider<T> Select(string expression);
        ISqlProvider<T> Select<TResult>(Expression<Func<T, TResult>> columns);
        ISqlProvider<T> Distinct();
        ISqlProvider<T> GroupBy(string expression);
        ISqlProvider<T> GroupBy<TResult>(Expression<Func<T, TResult>> expression);
        ISqlProvider<T> Having(string expression);
        ISqlProvider<T> Having(Expression<Func<T, bool>> expression);
        ISqlProvider<T> OrderBy(string orderBy);
        ISqlProvider<T> OrderBy<TResult>(Expression<Func<T, TResult>> expression);
        ISqlProvider<T> OrderByDescending<TResult>(Expression<Func<T, TResult>> expression);

        ISqlProvider<T> Set(string expression, Action<Dictionary<string, object>> action = null);
        ISqlProvider<T> Set<TResult>(Expression<Func<T, TResult>> column, TResult value);
        ISqlProvider<T> Set<TResult>(Expression<Func<T, TResult>> column, Expression<Func<T, TResult>> value);

        ISqlProvider<T> Update(T entity);
        ISqlProvider<T> Insert(T entity);
        ISqlProvider<T> Delete(T entity);

        SqlBuilder ToSql();
        int SaveChanges(int? timeout = null);
        Task<int> SaveChangesAsync(int? timeout = null);

        #region Select
        T Single(int? timeout = null);
        Task<T> SingleAsync(int? timeout = null);
        TResult Single<TResult>(int? timeout = null);
        Task<TResult> SingleAsync<TResult>(int? timeout = null);

        IEnumerable<T> ToList(bool buffered = true, int? timeout = null);
        Task<IEnumerable<T>> ToListAsync(bool buffered = true, int? timeout = null);
        IEnumerable<TResult> ToList<TResult>(bool buffered = true, int? timeout = null);
        Task<IEnumerable<TResult>> ToListAsync<TResult>(bool buffered = true, int? timeout = null);
        #endregion

        #endregion
    }







    public interface ISqlProvider<T1, T2> : IProvider
    {
        ISqlProvider<T1, T2> Join(string expression);
        ISqlProvider<T1, T2> Join(Expression<Func<T1, T2, bool>> expression, JoinType join = JoinType.Inner);
        ISqlProvider<T1, T2> GroupBy(string expression);
        ISqlProvider<T1, T2> GroupBy<TResult>(Expression<Func<T1, T2, TResult>> expression);
        ISqlProvider<T1, T2> Where(string expression, Action<Dictionary<string, object>> action = null);
        ISqlProvider<T1, T2> Where(Expression<Func<T1, T2, bool>> expression);
        ISqlProvider<T1, T2> OrderBy(string orderBy);
        ISqlProvider<T1, T2> OrderBy<TResult>(Expression<Func<T1, T2, TResult>> expression);
        ISqlProvider<T1, T2> OrderByDescending<TResult>(Expression<Func<T1, T2, TResult>> expression);
        ISqlProvider<T1, T2> Having(string expression);
        ISqlProvider<T1, T2> Having(Expression<Func<T1, T2, bool>> expression);
        ISqlProvider<T1, T2> Distinct();
        ISqlProvider<T1, T2> Select(string expression);
        ISqlProvider<T1, T2> Select<TResult>(Expression<Func<T1, T2, TResult>> expression);
        SqlBuilder ToSql();

        IEnumerable<TResult> ToList<TResult>(bool buffered = true, int? timeout = null);
        Task<IEnumerable<TResult>> ToListAsync<TResult>(bool buffered = true, int? timeout = null);
    }
    public interface ISqlProvider<T1, T2, T3> : IProvider
    {
        ISqlProvider<T1, T2, T3> Join(string expression);
        ISqlProvider<T1, T2, T3> Join<E1, E2>(Expression<Func<E1, E2, bool>> expression, JoinType join = JoinType.Inner) where E1 : class where E2 : class;
        ISqlProvider<T1, T2, T3> GroupBy(string expression);
        ISqlProvider<T1, T2, T3> GroupBy<TResult>(Expression<Func<T1, T2, T3, TResult>> expression);
        ISqlProvider<T1, T2, T3> Where(string expression, Action<Dictionary<string, object>> action = null);
        ISqlProvider<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> expression);
        ISqlProvider<T1, T2, T3> OrderBy(string orderBy);
        ISqlProvider<T1, T2, T3> OrderBy<TResult>(Expression<Func<T1, T2, T3, TResult>> expression);
        ISqlProvider<T1, T2, T3> OrderByDescending<TResult>(Expression<Func<T1, T2, T3, TResult>> expression);
        ISqlProvider<T1, T2, T3> Having(string expression);
        ISqlProvider<T1, T2, T3> Having(Expression<Func<T1, T2, T3, bool>> expression);
        ISqlProvider<T1, T2, T3> Distinct();
        ISqlProvider<T1, T2, T3> Select(string expression);
        ISqlProvider<T1, T2, T3> Select<TResult>(Expression<Func<T1, T2, T3, TResult>> expression);
        SqlBuilder ToSql();

        IEnumerable<TResult> ToList<TResult>(bool buffered = true, int? timeout = null);
        Task<IEnumerable<TResult>> ToListAsync<TResult>(bool buffered = true, int? timeout = null);
    }

    public enum LockType
    {
        FOR_UPADTE,
        LOCK_IN_SHARE_MODE,
        UPDLOCK,
        NOLOCK
    }
    public enum JoinType
    {
        Inner,
        Left,
        Right
    }
    public enum CommandType
    {
        Select,
        Insert,
        Update,
        Delete
    }
}
