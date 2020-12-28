using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WangSql.Abstract.Models;

namespace WangSql.Abstract.Linq
{
    public class DefaultSqlProvider<T> : ISqlProvider<T> where T : class
    {

        #region constructor
        protected ExpressionUtil _expressionUtil { get; set; }
        protected ISqlExe _sqlMapper { get; set; }
        public void Init()
        {
            _param = new Dictionary<string, object>();
        }
        public void Init(ISqlExe sqlMapper)
        {
            _expressionUtil = new ExpressionUtil(sqlMapper.SqlFactory.DbProvider);
            _sqlMapper = sqlMapper;
            _param = new Dictionary<string, object>();
        }
        #endregion

        #region implement
        public ISqlProvider<T> With(string lockType)
        {
            _lock.Append(lockType);
            return this;
        }
        public ISqlProvider<T> With(LockType lockType)
        {
            if (lockType == LockType.FOR_UPADTE)
            {
                With("FOR UPDATE");
            }
            else if (lockType == LockType.LOCK_IN_SHARE_MODE)
            {
                With("LOCK IN SHARE MODE");
            }
            return this;
        }
        public ISqlProvider<T> Where(string expression, Action<Dictionary<string, object>> action = null)
        {
            if (_whereBuffer.Length > 0)
            {
                _whereBuffer.AppendFormat(" {0} ", Operator.GetOperator(ExpressionType.AndAlso));
            }
            action?.Invoke(_param);
            _whereBuffer.Append(expression);
            return this;
        }
        public ISqlProvider<T> Where(Expression<Func<T, bool>> expression)
        {
            Where(_expressionUtil.BuildExpression(expression, _param), null);
            return this;
        }
        public ISqlProvider<T> WhereIf(bool where, Expression<Func<T, bool>> expression)
        {
            if (where)
                Where(_expressionUtil.BuildExpression(expression, _param), null);
            return this;
        }
        public ISqlProvider<T> WhereIf(bool where, Expression<Func<T, bool>> expressionTrue, Expression<Func<T, bool>> expressionFalse)
        {
            if (where)
                Where(_expressionUtil.BuildExpression(expressionTrue, _param), null);
            else
                Where(_expressionUtil.BuildExpression(expressionFalse, _param), null);
            return this;
        }
        public ISqlProvider<T> Filter<TResult>(Expression<Func<T, TResult>> columns)
        {
            _filters.AddRange(_expressionUtil.BuildColumns(columns, _param).Select(s => s.Value));
            return this;
        }

        //Select
        public ISqlProvider<T> Select(string expression)
        {
            _commandType = CommandType.Select;
            if (expression != null)
            {
                _columnBuffer.Append(expression);
            }
            return this;
        }
        public ISqlProvider<T> Select<TResult>(Expression<Func<T, TResult>> columns)
        {
            _commandType = CommandType.Select;
            var columstr = string.Join(",",
                _expressionUtil.BuildColumns(columns, _param).Select(s => string.Format("{0} AS {1}", s.Value, s.Key)));
            if (columstr != null)
            {
                _columnBuffer.Append(columns);
            }
            return this;
        }
        public ISqlProvider<T> Distinct()
        {
            _distinctBuffer.Append("DISTINCT");
            return this;
        }
        public ISqlProvider<T> GroupBy(string expression)
        {
            if (_groupBuffer.Length > 0)
            {
                _groupBuffer.Append(",");
            }
            _groupBuffer.Append(expression);
            return this;
        }
        public ISqlProvider<T> GroupBy<TResult>(Expression<Func<T, TResult>> expression)
        {
            GroupBy(string.Join(",", _expressionUtil.BuildColumns(expression, _param).Select(s => s.Value)));
            return this;
        }
        public ISqlProvider<T> Having(string expression)
        {
            _havingBuffer.Append(expression);
            return this;
        }
        public ISqlProvider<T> Having(Expression<Func<T, bool>> expression)
        {
            Having(string.Join(",", _expressionUtil.BuildColumns(expression, _param).Select(s => s.Value)));
            return this;
        }
        public ISqlProvider<T> OrderBy(string orderBy)
        {
            if (_orderBuffer.Length > 0)
            {
                _orderBuffer.Append(",");
            }
            _orderBuffer.Append(orderBy);
            return this;
        }
        public ISqlProvider<T> OrderBy<TResult>(Expression<Func<T, TResult>> expression)
        {
            OrderBy(string.Join(",", _expressionUtil.BuildColumns(expression, _param).Select(s => string.Format("{0} ASC", s.Value))));
            return this;
        }
        public ISqlProvider<T> OrderByDescending<TResult>(Expression<Func<T, TResult>> expression)
        {
            OrderBy(string.Join(",", _expressionUtil.BuildColumns(expression, _param).Select(s => string.Format("{0} DESC", s.Value))));
            return this;
        }

        //Update(Filter和Select将失效)
        public ISqlProvider<T> Set(string expression, Action<Dictionary<string, object>> action = null)
        {
            if (_setBuffer.Length > 0)
            {
                _setBuffer.Append(",");
            }
            action?.Invoke(_param);
            _setBuffer.AppendFormat(expression);
            return this;
        }
        public ISqlProvider<T> Set<TResult>(Expression<Func<T, TResult>> column, TResult value)
        {
            if (_setBuffer.Length > 0)
            {
                _setBuffer.Append(",");
            }
            var columns = _expressionUtil.BuildColumn(column, _param).First();
            var key = string.Format("{0}{1}", columns.Key, _param.Count);
            _param.Add(key, value);
            _setBuffer.AppendFormat("{0} = #{1}#", columns.Value, key);
            return this;
        }
        public ISqlProvider<T> Set<TResult>(Expression<Func<T, TResult>> column, Expression<Func<T, TResult>> value)
        {
            if (_setBuffer.Length > 0)
            {
                _setBuffer.Append(",");
            }
            var columnName = _expressionUtil.BuildColumn(column, _param).First().Value;
            var expression = _expressionUtil.BuildExpression(value, _param);
            _setBuffer.AppendFormat("{0} = {1}", columnName, expression);
            return this;
        }

        public ISqlProvider<T> Update(T entity)
        {
            _commandType = CommandType.Update;
            if (entity != null)
                entity.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
                    .ToList().ForEach(op => _param.Add(op.Name, op.GetValue(entity, null)));
            return this;
        }
        public ISqlProvider<T> Update(Dictionary<string, object> entity)
        {
            _commandType = CommandType.Update;
            if (entity != null)
                _param = entity;
            return this;
        }
        public ISqlProvider<T> Insert(T entity)
        {
            _commandType = CommandType.Insert;
            if (entity != null)
                entity.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
                    .ToList().ForEach(op => _param.Add(op.Name, op.GetValue(entity, null)));
            return this;
        }
        public ISqlProvider<T> Insert(Dictionary<string, object> entity)
        {
            _commandType = CommandType.Insert;
            if (entity != null)
                _param = entity;
            return this;
        }
        public ISqlProvider<T> Delete(T entity)
        {
            _commandType = CommandType.Delete;
            if (entity != null)
                entity.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
                    .ToList().ForEach(op => _param.Add(op.Name, op.GetValue(entity, null)));
            return this;
        }
        public ISqlProvider<T> Delete(Dictionary<string, object> entity)
        {
            _commandType = CommandType.Delete;
            if (entity != null)
                _param = entity;
            return this;
        }

        public SqlBuilder ToSql()
        {
            var sqlBuilder = new SqlBuilder()
            {
                Parameter = _param
            };
            switch (_commandType)
            {
                case CommandType.Select:
                    sqlBuilder.Sql = BuildSelect();
                    break;
                case CommandType.Insert:
                    sqlBuilder.Sql = BuildInsert();
                    break;
                case CommandType.Update:
                    sqlBuilder.Sql = BuildUpdate();
                    break;
                case CommandType.Delete:
                    sqlBuilder.Sql = BuildDelete();
                    break;
            }

            Console.WriteLine(sqlBuilder.Sql);

            return sqlBuilder;
        }
        public int SaveChanges(int? timeout = null)
        {
            if (_sqlMapper != null)
            {
                var sql = ToSql();
                return _sqlMapper.Execute(sql.Sql, sql.Parameter, timeout);
            }
            return 0;
        }
        public async Task<int> SaveChangesAsync(int? timeout = null)
        {
            if (_sqlMapper != null)
            {
                var sql = ToSql();
                return await _sqlMapper.ExecuteAsync(sql.Sql, sql.Parameter, timeout);
            }
            return 0;
        }

        #region Select
        public T Single(int? timeout = null)
        {
            if (_sqlMapper != null)
            {
                var sql = ToSql();
                return _sqlMapper.QueryFirstOrDefault<T>(sql.Sql, sql.Parameter, timeout);
            }
            return default(T);
        }
        public async Task<T> SingleAsync(int? timeout = null)
        {
            if (_sqlMapper != null)
            {
                var sql = ToSql();
                return await _sqlMapper.QueryFirstOrDefaultAsync<T>(sql.Sql, sql.Parameter, timeout);
            }
            return default(T);
        }
        public TResult Single<TResult>(int? timeout = null)
        {
            if (_sqlMapper != null)
            {
                var sql = ToSql();
                return _sqlMapper.QueryFirstOrDefault<TResult>(sql.Sql, sql.Parameter, timeout);
            }
            return default(TResult);
        }
        public async Task<TResult> SingleAsync<TResult>(int? timeout = null)
        {
            if (_sqlMapper != null)
            {
                var sql = ToSql();
                return await _sqlMapper.QueryFirstOrDefaultAsync<TResult>(sql.Sql, sql.Parameter, timeout);
            }
            return default(TResult);
        }

        public IEnumerable<T> ToList(bool buffered = true, int? timeout = null)
        {
            if (_sqlMapper != null)
            {
                var sql = ToSql();
                return _sqlMapper.Query<T>(sql.Sql, sql.Parameter, buffered, timeout);
            }
            return new List<T>();
        }
        public async Task<IEnumerable<T>> ToListAsync(bool buffered = true, int? timeout = null)
        {
            if (_sqlMapper != null)
            {
                var sql = ToSql();
                return await _sqlMapper.QueryAsync<T>(sql.Sql, sql.Parameter, buffered, timeout);
            }
            return new List<T>();
        }
        public IEnumerable<TResult> ToList<TResult>(bool buffered = true, int? timeout = null)
        {
            if (_sqlMapper != null)
            {
                var sql = ToSql();
                return _sqlMapper.Query<TResult>(sql.Sql, sql.Parameter, buffered, timeout);
            }
            return new List<TResult>();
        }
        public async Task<IEnumerable<TResult>> ToListAsync<TResult>(bool buffered = true, int? timeout = null)
        {
            if (_sqlMapper != null)
            {
                var sql = ToSql();
                return await _sqlMapper.QueryAsync<TResult>(sql.Sql, sql.Parameter, buffered, timeout);
            }
            return new List<TResult>();
        }
        #endregion

        #endregion

        #region property
        public CommandType _commandType = CommandType.Select;
        public Dictionary<string, object> _param { get; set; }
        public StringBuilder _columnBuffer = new StringBuilder();
        public List<string> _filters = new List<string>();
        public StringBuilder _setBuffer = new StringBuilder();
        public StringBuilder _havingBuffer = new StringBuilder();
        public StringBuilder _whereBuffer = new StringBuilder();
        public StringBuilder _groupBuffer = new StringBuilder();
        public StringBuilder _orderBuffer = new StringBuilder();
        public StringBuilder _distinctBuffer = new StringBuilder();
        public StringBuilder _countBuffer = new StringBuilder();
        public StringBuilder _sumBuffer = new StringBuilder();
        public StringBuilder _lock = new StringBuilder();
        public TableInfo _table = EntityUtil.GetMap<T>();
        #endregion

        #region build
        public string BuildInsert()
        {
            var sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                _table.TableName,
                string.Join(",", _table.Columns.FindAll(f => !_filters.Exists(e => e == f.ColumnName)).Select(s => s.ColumnName))
                , string.Join(",", _table.Columns.FindAll(f => !_filters.Exists(e => e == f.ColumnName)).Select(s => string.Format("#{0}#", s.PropertyName))));
            return sql;
        }
        public string BuildUpdate()
        {
            bool allColumn = _setBuffer.Length == 0;
            if (allColumn)
            {
                var keyColumn = _table.Columns.Find(f => f.IsPrimaryKey);
                var columns = _table.Columns.FindAll(f => !f.IsPrimaryKey && !_filters.Exists(e => e == f.ColumnName));
                var sql = string.Format("UPDATE {0} SET {1} WHERE {2}",
                    _table.TableName,
                    string.Join(",", columns.Select(s => string.Format("{0} = #{1}#", s.ColumnName, s.PropertyName))),
                    string.Format("{0} = #{1}#", keyColumn.ColumnName, keyColumn.PropertyName)
                    );
                return sql;
            }
            else
            {
                var sql = string.Format("UPDATE {0} SET {1}{2}",
                    _table.TableName,
                    _setBuffer,
                    _whereBuffer.Length > 0 ? string.Format(" WHERE {0}", _whereBuffer) : "");
                return sql;
            }

        }
        public string BuildDelete()
        {
            var sql = string.Format("DELETE FROM {0}{1}",
                _table.TableName,
                _whereBuffer.Length > 0 ? string.Format(" WHERE {0}", _whereBuffer) : "");
            return sql;
        }
        public string BuildSelect()
        {
            var sqlBuffer = new StringBuilder("SELECT");
            if (_distinctBuffer.Length > 0)
            {
                sqlBuffer.AppendFormat(" {0}", _distinctBuffer);
            }
            if (_columnBuffer.Length > 0)
            {
                sqlBuffer.AppendFormat(" {0}", _columnBuffer);
            }
            else
            {
                sqlBuffer.AppendFormat(" {0}", string.Join(",", _table.Columns.FindAll(f => !_filters.Exists(e => e == f.ColumnName)).Select(s => string.Format("{0} AS {1}", s.ColumnName, s.PropertyName))));
            }
            sqlBuffer.AppendFormat(" FROM {0}", _table.TableName);
            if (_whereBuffer.Length > 0)
            {
                sqlBuffer.AppendFormat(" WHERE {0}", _whereBuffer);
            }
            if (_groupBuffer.Length > 0)
            {
                sqlBuffer.AppendFormat(" GROUP BY {0}", _groupBuffer);
            }
            if (_havingBuffer.Length > 0)
            {
                sqlBuffer.AppendFormat(" HAVING {0}", _havingBuffer);
            }
            if (_orderBuffer.Length > 0)
            {
                sqlBuffer.AppendFormat(" ORDER BY {0}", _orderBuffer);
            }
            //if (pageIndex != null && pageCount != null)
            //{
            //    sqlBuffer.AppendFormat(" LIMIT {0},{1}", pageIndex, pageCount);
            //}
            if (_lock.Length > 0)
            {
                sqlBuffer.AppendFormat(" {0}", _lock);
            }
            var sql = sqlBuffer.ToString();
            return sql;
        }
        #endregion
    }









    public class DefaultSqlProvider<T1, T2> : ISqlProvider<T1, T2> where T1 : class where T2 : class
    {
        #region constructor
        protected ExpressionUtil _expressionUtil { get; set; }
        protected ISqlExe _sqlMapper { get; set; }
        public void Init()
        {
            _expressionUtil = new ExpressionUtil();
            _param = new Dictionary<string, object>();
        }
        public void Init(ISqlExe sqlMapper)
        {
            _expressionUtil = new ExpressionUtil();
            _sqlMapper = sqlMapper;
            _param = new Dictionary<string, object>();
        }
        #endregion

        #region implement
        public ISqlProvider<T1, T2> Distinct()
        {
            _distinctBuffer.Append("DISTINCT");
            return this;
        }
        public ISqlProvider<T1, T2> GroupBy(string expression)
        {
            if (_groupBuffer.Length > 0)
            {
                _groupBuffer.Append(",");
            }
            _groupBuffer.Append(expression);
            return this;
        }
        public ISqlProvider<T1, T2> GroupBy<TResult>(Expression<Func<T1, T2, TResult>> expression)
        {
            GroupBy(string.Join(",", _expressionUtil.BuildColumns(expression, _param, false).Select(s => s.Value)));
            return this;
        }
        public ISqlProvider<T1, T2> Having(string expression)
        {
            _havingBuffer.Append(expression);
            return this;
        }
        public ISqlProvider<T1, T2> Having(Expression<Func<T1, T2, bool>> expression)
        {
            Having(string.Join(",", _expressionUtil.BuildColumns(expression, _param, false).Select(s => s.Value)));
            return this;
        }
        public ISqlProvider<T1, T2> OrderBy(string orderBy)
        {
            if (_orderBuffer.Length > 0)
            {
                _orderBuffer.Append(",");
            }
            _orderBuffer.Append(orderBy);
            return this;
        }
        public ISqlProvider<T1, T2> OrderBy<TResult>(Expression<Func<T1, T2, TResult>> expression)
        {
            OrderBy(string.Join(",", _expressionUtil.BuildColumns(expression, _param, false).Select(s => string.Format("{0} ASC", s.Value))));
            return this;
        }
        public ISqlProvider<T1, T2> OrderByDescending<TResult>(Expression<Func<T1, T2, TResult>> expression)
        {
            OrderBy(string.Join(",", _expressionUtil.BuildColumns(expression, _param, false).Select(s => string.Format("{0} DESC", s.Value))));
            return this;
        }
        public ISqlProvider<T1, T2> Where(string expression, Action<Dictionary<string, object>> action = null)
        {
            if (_whereBuffer.Length > 0)
            {
                _whereBuffer.AppendFormat(" {0} ", Operator.GetOperator(ExpressionType.AndAlso));
            }
            action?.Invoke(_param);
            _whereBuffer.Append(expression);
            return this;
        }
        public ISqlProvider<T1, T2> Where(Expression<Func<T1, T2, bool>> expression)
        {
            Where(_expressionUtil.BuildExpression(expression, _param, false), null);
            return this;
        }
        public ISqlProvider<T1, T2> Join(string expression)
        {
            if (_join.Length > 0)
            {
                _join.Append(" ");
            }
            _join.Append(expression);
            return this;
        }
        public ISqlProvider<T1, T2> Join(Expression<Func<T1, T2, bool>> expression, JoinType join = JoinType.Inner)
        {
            var onExpression = _expressionUtil.BuildExpression(expression, _param, false);
            var table1Name = EntityUtil.GetMap<T1>().TableName;
            var table2Name = EntityUtil.GetMap<T2>().TableName;
            var joinType = string.Format("{0} JOIN", join.ToString().ToUpper());
            Join(string.Format("{0} {1} {2} ON {3}", table1Name, joinType, table2Name, onExpression));
            return this;
        }
        public ISqlProvider<T1, T2> Select(string expression)
        {
            if (expression != null)
            {
                _columnBuffer.Append(expression);
            }
            return this;
        }
        public ISqlProvider<T1, T2> Select<TResult>(Expression<Func<T1, T2, TResult>> expression)
        {
            var columstr = string.Join(",",
                _expressionUtil.BuildColumns(expression, _param, false).Select(s => string.Format("{0} AS {1}", s.Value, s.Key)));
            return Select(columstr);
        }
        public SqlBuilder ToSql()
        {
            var sqlBuilder = new SqlBuilder()
            {
                Sql = BuildSelect(),
                Parameter = _param
            };

            Console.WriteLine(sqlBuilder.Sql);

            return sqlBuilder;
        }

        public IEnumerable<TResult> ToList<TResult>(bool buffered = true, int? timeout = null)
        {
            if (_sqlMapper != null)
            {
                var sql = ToSql();
                return _sqlMapper.Query<TResult>(sql.Sql, sql.Parameter, buffered, timeout);
            }
            return new List<TResult>();
        }
        public async Task<IEnumerable<TResult>> ToListAsync<TResult>(bool buffered = true, int? timeout = null)
        {
            if (_sqlMapper != null)
            {
                var sql = ToSql();
                return await _sqlMapper.QueryAsync<TResult>(sql.Sql, sql.Parameter, buffered, timeout);
            }
            return new List<TResult>();
        }
        #endregion

        #region property
        public Dictionary<string, object> _param { get; set; }
        public StringBuilder _columnBuffer = new StringBuilder();
        public StringBuilder _havingBuffer = new StringBuilder();
        public StringBuilder _whereBuffer = new StringBuilder();
        public StringBuilder _groupBuffer = new StringBuilder();
        public StringBuilder _orderBuffer = new StringBuilder();
        public StringBuilder _distinctBuffer = new StringBuilder();
        public StringBuilder _countBuffer = new StringBuilder();
        public StringBuilder _join = new StringBuilder();
        #endregion

        #region build
        public string BuildSelect()
        {
            var sqlBuffer = new StringBuilder("SELECT");
            if (_distinctBuffer.Length > 0)
            {
                sqlBuffer.AppendFormat(" {0}", _distinctBuffer);
            }
            sqlBuffer.AppendFormat(" {0}", _columnBuffer);
            sqlBuffer.AppendFormat(" FROM {0}", _join);
            if (_whereBuffer.Length > 0)
            {
                sqlBuffer.AppendFormat(" WHERE {0}", _whereBuffer);
            }
            if (_groupBuffer.Length > 0)
            {
                sqlBuffer.AppendFormat(" GROUP BY {0}", _groupBuffer);
            }
            if (_havingBuffer.Length > 0)
            {
                sqlBuffer.AppendFormat(" HAVING {0}", _havingBuffer);
            }
            if (_orderBuffer.Length > 0)
            {
                sqlBuffer.AppendFormat(" ORDER BY {0}", _orderBuffer);
            }
            //if (pageIndex != null && pageCount != null)
            //{
            //    sqlBuffer.AppendFormat(" LIMIT {0},{1}", pageIndex, pageCount);
            //}
            var sql = sqlBuffer.ToString();
            return sql;
        }
        #endregion
    }
    public class DefaultSqlProvider<T1, T2, T3> : ISqlProvider<T1, T2, T3> where T1 : class where T2 : class where T3 : class
    {
        #region constructor
        protected ExpressionUtil _expressionUtil { get; set; }
        protected ISqlExe _sqlMapper { get; set; }
        public void Init()
        {
            _expressionUtil = new ExpressionUtil();
            _param = new Dictionary<string, object>();
        }
        public void Init(ISqlExe sqlMapper)
        {
            _expressionUtil = new ExpressionUtil();
            _sqlMapper = sqlMapper;
            _param = new Dictionary<string, object>();
        }
        #endregion

        #region implement
        public ISqlProvider<T1, T2, T3> Distinct()
        {
            _distinctBuffer.Append("DISTINCT");
            return this;
        }
        public ISqlProvider<T1, T2, T3> GroupBy(string expression)
        {
            if (_groupBuffer.Length > 0)
            {
                _groupBuffer.Append(",");
            }
            _groupBuffer.Append(expression);
            return this;
        }
        public ISqlProvider<T1, T2, T3> GroupBy<TResult>(Expression<Func<T1, T2, T3, TResult>> expression)
        {
            GroupBy(string.Join(",", _expressionUtil.BuildColumns(expression, _param, false).Select(s => s.Value)));
            return this;
        }
        public ISqlProvider<T1, T2, T3> Having(string expression)
        {
            _havingBuffer.Append(expression);
            return this;
        }
        public ISqlProvider<T1, T2, T3> Having(Expression<Func<T1, T2, T3, bool>> expression)
        {
            Having(string.Join(",", _expressionUtil.BuildColumns(expression, _param, false).Select(s => s.Value)));
            return this;
        }
        public ISqlProvider<T1, T2, T3> OrderBy(string orderBy)
        {
            if (_orderBuffer.Length > 0)
            {
                _orderBuffer.Append(",");
            }
            _orderBuffer.Append(orderBy);
            return this;
        }
        public ISqlProvider<T1, T2, T3> OrderBy<TResult>(Expression<Func<T1, T2, T3, TResult>> expression)
        {
            OrderBy(string.Join(",", _expressionUtil.BuildColumns(expression, _param, false).Select(s => string.Format("{0} ASC", s.Value))));
            return this;
        }
        public ISqlProvider<T1, T2, T3> OrderByDescending<TResult>(Expression<Func<T1, T2, T3, TResult>> expression)
        {
            OrderBy(string.Join(",", _expressionUtil.BuildColumns(expression, _param, false).Select(s => string.Format("{0} DESC", s.Value))));
            return this;
        }
        public ISqlProvider<T1, T2, T3> Where(string expression, Action<Dictionary<string, object>> action = null)
        {
            if (_whereBuffer.Length > 0)
            {
                _whereBuffer.AppendFormat(" {0} ", Operator.GetOperator(ExpressionType.AndAlso));
            }
            action?.Invoke(_param);
            _whereBuffer.Append(expression);
            return this;
        }
        public ISqlProvider<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> expression)
        {
            Where(_expressionUtil.BuildExpression(expression, _param, false), null);
            return this;
        }
        public ISqlProvider<T1, T2, T3> Join(string expression)
        {
            if (_join.Length > 0)
            {
                _join.Append(" ");
            }
            _join.Append(expression);
            return this;
        }
        public ISqlProvider<T1, T2, T3> Join<E1, E2>(Expression<Func<E1, E2, bool>> expression, JoinType join = JoinType.Inner) where E1 : class where E2 : class
        {
            var onExpression = _expressionUtil.BuildExpression(expression, _param, false);
            var table1Name = EntityUtil.GetMap<E1>().TableName;
            var table2Name = EntityUtil.GetMap<E2>().TableName;
            var joinType = string.Format("{0} JOIN", join.ToString().ToUpper());
            if (_tables.Count == 0)
            {
                _tables.Add(table1Name);
                _tables.Add(table2Name);
                Join(string.Format("{0} {1} {2} ON {3}", table1Name, joinType, table2Name, onExpression));
            }
            else if (_tables.Exists(a => table1Name == a))
            {
                _tables.Add(table2Name);
                Join(string.Format("{0} {1} ON {2}", joinType, table2Name, onExpression));
            }
            else
            {
                _tables.Add(table1Name);
                Join(string.Format("{0} {1} ON {2}", joinType, table1Name, onExpression));
            }
            return this;
        }
        public ISqlProvider<T1, T2, T3> Select(string expression)
        {
            if (expression != null)
            {
                _columnBuffer.Append(expression);
            }
            return this;
        }
        public ISqlProvider<T1, T2, T3> Select<TResult>(Expression<Func<T1, T2, T3, TResult>> expression)
        {
            var columstr = string.Join(",",
                _expressionUtil.BuildColumns(expression, _param, false).Select(s => string.Format("{0} AS {1}", s.Value, s.Key)));
            return Select(columstr);
        }
        public SqlBuilder ToSql()
        {
            var sqlBuilder = new SqlBuilder()
            {
                Sql = BuildSelect(),
                Parameter = _param
            };

            Console.WriteLine(sqlBuilder.Sql);

            return sqlBuilder;
        }

        public IEnumerable<TResult> ToList<TResult>(bool buffered = true, int? timeout = null)
        {
            if (_sqlMapper != null)
            {
                var sql = ToSql();
                return _sqlMapper.Query<TResult>(sql.Sql, sql.Parameter, buffered, timeout);
            }
            return new List<TResult>();
        }
        public async Task<IEnumerable<TResult>> ToListAsync<TResult>(bool buffered = true, int? timeout = null)
        {
            if (_sqlMapper != null)
            {
                var sql = ToSql();
                return await _sqlMapper.QueryAsync<TResult>(sql.Sql, sql.Parameter, timeout);
            }
            return new List<TResult>();
        }
        #endregion

        #region property
        public Dictionary<string, object> _param { get; set; }
        public StringBuilder _columnBuffer = new StringBuilder();
        public StringBuilder _havingBuffer = new StringBuilder();
        public StringBuilder _whereBuffer = new StringBuilder();
        public StringBuilder _groupBuffer = new StringBuilder();
        public StringBuilder _orderBuffer = new StringBuilder();
        public StringBuilder _distinctBuffer = new StringBuilder();
        public StringBuilder _countBuffer = new StringBuilder();
        public StringBuilder _join = new StringBuilder();
        public List<string> _tables = new List<string>();
        #endregion

        #region build
        public string BuildSelect()
        {
            var sqlBuffer = new StringBuilder("SELECT");
            if (_distinctBuffer.Length > 0)
            {
                sqlBuffer.AppendFormat(" {0}", _distinctBuffer);
            }
            sqlBuffer.AppendFormat(" {0}", _columnBuffer);
            sqlBuffer.AppendFormat(" FROM {0}", _join);
            if (_whereBuffer.Length > 0)
            {
                sqlBuffer.AppendFormat(" WHERE {0}", _whereBuffer);
            }
            if (_groupBuffer.Length > 0)
            {
                sqlBuffer.AppendFormat(" GROUP BY {0}", _groupBuffer);
            }
            if (_havingBuffer.Length > 0)
            {
                sqlBuffer.AppendFormat(" HAVING {0}", _havingBuffer);
            }
            if (_orderBuffer.Length > 0)
            {
                sqlBuffer.AppendFormat(" ORDER BY {0}", _orderBuffer);
            }
            //if (pageIndex != null && pageCount != null)
            //{
            //    sqlBuffer.AppendFormat(" LIMIT {0},{1}", pageIndex, pageCount);
            //}
            var sql = sqlBuffer.ToString();
            return sql;
        }
        #endregion
    }
}
