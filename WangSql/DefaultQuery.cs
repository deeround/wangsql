using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WangSql
{
    public abstract class BaseSqlPart
    {
        public string Sql { get; set; }
    }

    public class WhereSqlPart : BaseSqlPart
    {
        public List<DbParamter> Paramters { get; set; }

        public WhereSqlPart()
        {
            Paramters = new List<DbParamter>();
        }
    }

    public class OrderBySqlPart : BaseSqlPart
    {
    }

    public class SelectSqlPart : BaseSqlPart
    {
    }

    public class GroupBySqlPart : BaseSqlPart
    {
    }

    public class HavingSqlPart : BaseSqlPart
    {
        public List<DbParamter> Paramters { get; set; }

        public HavingSqlPart()
        {
            Paramters = new List<DbParamter>();
        }
    }

    public class DistinctSqlPart : BaseSqlPart
    {
    }

    public class SetSqlPart : BaseSqlPart
    {
        public bool IsModify { get; set; }
    }

    public class DefaultQuery<T> where T : class
    {
        private readonly ISqlExe _sqlExe;

        private readonly List<WhereSqlPart> whereSqlParts = new List<WhereSqlPart>();
        private readonly List<OrderBySqlPart> orderBySqlParts = new List<OrderBySqlPart>();
        private readonly List<SelectSqlPart> selectSqlParts = new List<SelectSqlPart>();
        private readonly List<GroupBySqlPart> groupBySqlParts = new List<GroupBySqlPart>();
        private readonly List<HavingSqlPart> havingSqlParts = new List<HavingSqlPart>();
        private readonly List<DistinctSqlPart> distinctSqlParts = new List<DistinctSqlPart>();
        private readonly List<SetSqlPart> setSqlParts = new List<SetSqlPart>();

        public DefaultQuery(ISqlExe sqlExe)
        {
            _sqlExe = sqlExe;
        }

        public static ExpressionHelper<T> ExpressionHelperBuild(DbProvider dbProvider, List<DbParamter> dbParamters = null, int dbParamterStartIndex = 0)
        {
            return new ExpressionHelper<T>(dbProvider, dbParamters, dbParamterStartIndex);
        }

        public DefaultQuery<T> Where(Expression<Func<T, bool>> expression)
        {
            WhereSqlPart whereSqlPart = new WhereSqlPart();
            whereSqlPart.Sql = ExpressionHelperBuild(_sqlExe.SqlFactory.DbProvider, whereSqlPart.Paramters, whereSqlParts.Sum(op => op.Paramters.Count) + havingSqlParts.Sum(op => op.Paramters.Count)).GetWhereSql(expression);
            whereSqlParts.Add(whereSqlPart);
            return this;
        }

        public DefaultQuery<T> Where(string expression)
        {
            WhereSqlPart whereSqlPart = new WhereSqlPart
            {
                Sql = $"({expression})"
            };
            whereSqlParts.Add(whereSqlPart);
            return this;
        }

        public DefaultQuery<T> OrderBy(Expression<Func<T, object>> expression)
        {
            OrderBySqlPart orderBySqlPart = new OrderBySqlPart
            {
                Sql = ExpressionHelperBuild(_sqlExe.SqlFactory.DbProvider).GetOrderBySql(expression) + " asc"
            };
            orderBySqlParts.Add(orderBySqlPart);
            return this;
        }

        public DefaultQuery<T> OrderBy(string sortName)
        {
            OrderBySqlPart orderBySqlPart = new OrderBySqlPart
            {
                Sql = _sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(sortName) + " asc"
            };
            orderBySqlParts.Add(orderBySqlPart);
            return this;
        }

        public DefaultQuery<T> OrderByDescending(Expression<Func<T, object>> expression)
        {
            OrderBySqlPart orderSqlPart = new OrderBySqlPart
            {
                Sql = ExpressionHelperBuild(_sqlExe.SqlFactory.DbProvider).GetOrderBySql(expression) + " desc"
            };
            orderBySqlParts.Add(orderSqlPart);
            return this;
        }

        public DefaultQuery<T> OrderByDescending(string sortName)
        {
            OrderBySqlPart orderSqlPart = new OrderBySqlPart
            {
                Sql = _sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(sortName) + " desc"
            };
            orderBySqlParts.Add(orderSqlPart);
            return this;
        }

        public DefaultQuery<T> Select(Expression<Func<T, object>> expression)
        {
            SelectSqlPart selectSqlPart = new SelectSqlPart
            {
                Sql = ExpressionHelperBuild(_sqlExe.SqlFactory.DbProvider).GetSelectSql(expression)
            };
            selectSqlParts.Add(selectSqlPart);
            return this;
        }

        public DefaultQuery<T> GroupBy(Expression<Func<T, object>> expression)
        {
            GroupBySqlPart groupBySqlPart = new GroupBySqlPart
            {
                Sql = ExpressionHelperBuild(_sqlExe.SqlFactory.DbProvider).GetGroupBySql(expression)
            };
            groupBySqlParts.Add(groupBySqlPart);
            return this;
        }

        public DefaultQuery<T> Having(Expression<Func<T, bool>> expression)
        {
            HavingSqlPart havingSqlPart = new HavingSqlPart();
            havingSqlPart.Sql = ExpressionHelperBuild(_sqlExe.SqlFactory.DbProvider, havingSqlPart.Paramters, havingSqlParts.Sum(op => op.Paramters.Count) + whereSqlParts.Sum(op => op.Paramters.Count)).GetHavingSql(expression);
            havingSqlParts.Add(havingSqlPart);
            return this;
        }

        public DefaultQuery<T> Distinct()
        {
            DistinctSqlPart distinctSqlPart = new DistinctSqlPart
            {
                Sql = "distinct"
            };
            distinctSqlParts.Add(distinctSqlPart);
            return this;
        }

        public IList<T> ToList()
        {
            return _sqlExe.Query<T>(ResolveSqlPart(), ResolveParamter()).ToList();
        }

        public IList<T> ToPaged(int pageIndex, int pageSize)
        {
            return _sqlExe.Query<T>(ResolveSqlPart(), ResolveParamter(), pageIndex, pageSize).ToList();
        }

        public IList<T> ToPaged(int pageIndex, int pageSize, out int total)
        {
            return _sqlExe.Query<T>(ResolveSqlPart(), ResolveParamter(), pageIndex, pageSize, out total).ToList();
        }

        public T FirstOrDefault()
        {
            return _sqlExe.QueryFirstOrDefault<T>(ResolveSqlPart(), ResolveParamter());
        }

        public string ResolveSqlPart()
        {
            StringBuilder sbTemp = new StringBuilder();
            sbTemp.Append("select");
            if (distinctSqlParts.Any()) sbTemp.Append(" " + distinctSqlParts.First().Sql);
            if (selectSqlParts.Any()) sbTemp.Append(" " + string.Join(",", selectSqlParts.Select(op => op.Sql)));
            else sbTemp.Append(" " + string.Join(",", GetColumnsForSelect()));
            sbTemp.Append(" from");
            sbTemp.Append(" " + GetTable());
            if (whereSqlParts.Any())
            {
                sbTemp.Append(" where " + string.Join(" and ", whereSqlParts.Select(op => op.Sql)));
            }
            if (groupBySqlParts.Any()) sbTemp.Append(" group by " + string.Join(",", groupBySqlParts.Select(op => op.Sql)));
            if (havingSqlParts.Any())
            {
                sbTemp.Append(" having " + string.Join(" and ", havingSqlParts.Select(op => op.Sql)));
            }
            if (orderBySqlParts.Any()) sbTemp.Append(" order by " + string.Join(",", orderBySqlParts.Select(op => op.Sql)));
            return sbTemp.ToString();
        }

        public Dictionary<string, object> ResolveParamter()
        {
            List<DbParamter> paramters = new List<DbParamter>();
            if (whereSqlParts.Any())
            {
                whereSqlParts.ForEach(op => { paramters.AddRange(op.Paramters); });
            }
            if (havingSqlParts.Any())
            {
                havingSqlParts.ForEach(op => { paramters.AddRange(op.Paramters); });
            }

            var result = new Dictionary<string, object>();
            paramters.ForEach(op => { result.Add(op.Name, op.Value); });
            return result;
        }

        public DefaultQuery<T> Set(Expression<Func<T, object>> expression, bool isModify = true)
        {
            SetSqlPart setSqlPart = new SetSqlPart();
            setSqlPart.Sql = ExpressionHelperBuild(_sqlExe.SqlFactory.DbProvider).GetSetSql(expression);
            setSqlPart.IsModify = isModify;
            setSqlParts.Add(setSqlPart);
            return this;
        }

        public int Insert(T entity)
        {
            var cols = GetColumns();
            if (setSqlParts.Any())
            {
                if (setSqlParts.Any(op => op.IsModify))
                {
                    var sets = new List<string>();
                    setSqlParts.Where(o => o.IsModify).ToList().ForEach(op => { sets.AddRange(op.Sql.Split(',')); });
                    cols = cols.Where(op => sets.Any(x => x == op)).ToList();
                }
                else if (setSqlParts.Any(op => !op.IsModify))
                {
                    var sets = new List<string>();
                    setSqlParts.Where(o => !o.IsModify).ToList().ForEach(op => { sets.AddRange(op.Sql.Split(',')); });
                    cols = cols.Where(op => !sets.Any(x => x == op)).ToList();
                }
            }
            string sql = $"insert into {GetTable()}({string.Join(",", cols)}) values(#{string.Join("#,#", cols.Select(op => op.Replace("\"", "")))}#)";
            var sod = EntityToDict(entity);
            return _sqlExe.Execute(sql, sod);
        }

        public int Update(T entity)
        {
            var cols = GetColumns();
            var keys = GetPrimaryKeys();
            cols = cols.Where(op => !keys.Any(x => x == op)).ToList();
            if (setSqlParts.Any())
            {
                if (setSqlParts.Any(op => op.IsModify))
                {
                    var sets = new List<string>();
                    setSqlParts.Where(o => o.IsModify).ToList().ForEach(op => { sets.AddRange(op.Sql.Split(',')); });
                    cols = cols.Where(op => sets.Any(x => x == op)).ToList();
                }
                else if (setSqlParts.Any(op => !op.IsModify))
                {
                    var sets = new List<string>();
                    setSqlParts.Where(o => !o.IsModify).ToList().ForEach(op => { sets.AddRange(op.Sql.Split(',')); });
                    cols = cols.Where(op => !sets.Any(x => x == op)).ToList();
                }
            }
            var sql = $"update {GetTable()} set {string.Join(",", cols.Select(op => op + "=#" + op.Replace("\"", "") + "#"))}";
            var sod = EntityToDict(entity);
            if (whereSqlParts.Any())
            {
                sql += " where " + string.Join(" and ", whereSqlParts.Select(op => op.Sql));
                whereSqlParts.ForEach(op =>
                {
                    op.Paramters.ForEach(o => sod.Add(o.Name, o.Value));
                });
            }
            else
            {
                sql += " where " + string.Join(" and ", keys.Select(op => op + "=#" + op.Replace("\"", "") + "#"));
            }
            return _sqlExe.Execute(sql, sod);
        }

        public int Delete(string id)
        {
            var keys = GetPrimaryKeys();
            string sql = $"delete from {GetTable()} where {string.Join(" and ", keys.Select(op => op + "=#" + op.Replace("\"", "") + "#"))}";
            return _sqlExe.Execute(sql, id);
        }

        public int Delete()
        {
            string sql = $"delete from {GetTable()}";
            var sod = new Dictionary<string, object>();
            if (whereSqlParts.Any())
            {
                sql += " where " + string.Join(" and ", whereSqlParts.Select(op => op.Sql));
                whereSqlParts.ForEach(op =>
                {
                    op.Paramters.ForEach(o => sod.Add(o.Name, o.Value));
                });
            }
            return _sqlExe.Execute(sql, sod);
        }

        private string GetTable()
        {
            return _sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(TableMap.GetMap<T>().Name);
        }

        private IList<string> GetColumns()
        {
            IList<string> columns = new List<string>();
            var map = TableMap.GetMap<T>();
            map.Columns.ToList().ForEach(op =>
            {
                columns.Add(_sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(op.Name));
            });
            return columns;
        }

        private IList<string> GetColumnsForSelect()
        {
            var map = TableMap.GetMap<T>();
            IList<string> columns = new List<string>();
            map.Columns.ToList().ForEach(op =>
            {
                columns.Add($"{_sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(op.Name)} as {_sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(op.PropertyName)}");
            });
            return columns;
        }

        private IList<string> GetPrimaryKeys()
        {
            IList<string> columns = new List<string>();
            var map = TableMap.GetMap<T>();
            map.Columns.Where(op => op.IsPrimaryKey).ToList().ForEach(op =>
            {
                columns.Add(_sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(op.Name));
            });
            return columns;
        }

        private Dictionary<string, object> EntityToDict(T entity)
        {
            if (entity == null) return new Dictionary<string, object>();
            var sod = new Dictionary<string, object>();
            entity.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
                .ToList().ForEach(op => sod.Add(op.Name, op.GetValue(entity, null)));
            var map = TableMap.GetMap<T>();
            var sod1 = new Dictionary<string, object>();
            map.Columns.ToList().ForEach(op =>
            {
                sod1.Add(op.Name, sod[op.PropertyName]);
            });
            return sod1;
        }
    }

    //public class DefaultQuery<T, R> where T : class where R : class
    //{
    //    private readonly ISqlExe _sqlExe;

    //    private List<WhereSqlPart> whereSqlParts = new List<WhereSqlPart>();
    //    private List<OrderBySqlPart> orderBySqlParts = new List<OrderBySqlPart>();
    //    private List<SelectSqlPart> selectSqlParts = new List<SelectSqlPart>();
    //    private List<GroupBySqlPart> groupBySqlParts = new List<GroupBySqlPart>();
    //    private List<HavingSqlPart> havingSqlParts = new List<HavingSqlPart>();
    //    private List<DistinctSqlPart> distinctSqlParts = new List<DistinctSqlPart>();
    //    private List<SetSqlPart> setSqlParts = new List<SetSqlPart>();

    //    public DefaultQuery(ISqlExe sqlExe)
    //    {
    //        _sqlExe = sqlExe;
    //    }

    //    public static ExpressionHelper<T> ExpressionHelperBuild(DbProvider dbProvider, List<DbParamter> dbParamters = null, int dbParamterStartIndex = 0)
    //    {
    //        return new ExpressionHelper<T>(dbProvider, dbParamters, dbParamterStartIndex);
    //    }

    //    public DefaultQuery<T, R> Where(Expression<Func<T, bool>> expression)
    //    {
    //        WhereSqlPart whereSqlPart = new WhereSqlPart();
    //        whereSqlPart.Sql = ExpressionHelperBuild(_sqlExe.SqlFactory.DbProvider, whereSqlPart.Paramters, whereSqlParts.Sum(op => op.Paramters.Count) + havingSqlParts.Sum(op => op.Paramters.Count)).GetWhereSql(expression);
    //        whereSqlParts.Add(whereSqlPart);
    //        return this;
    //    }

    //    public DefaultQuery<T, R> OrderBy(Expression<Func<T, object>> expression)
    //    {
    //        OrderBySqlPart orderBySqlPart = new OrderBySqlPart();
    //        orderBySqlPart.Sql = ExpressionHelperBuild(_sqlExe.SqlFactory.DbProvider).GetOrderBySql(expression) + " asc";
    //        orderBySqlParts.Add(orderBySqlPart);
    //        return this;
    //    }

    //    public DefaultQuery<T, R> OrderBy(string sortName)
    //    {
    //        OrderBySqlPart orderBySqlPart = new OrderBySqlPart();
    //        orderBySqlPart.Sql = _sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(sortName) + " asc";
    //        orderBySqlParts.Add(orderBySqlPart);
    //        return this;
    //    }

    //    public DefaultQuery<T, R> OrderByDescending(Expression<Func<T, object>> expression)
    //    {
    //        OrderBySqlPart orderSqlPart = new OrderBySqlPart();
    //        orderSqlPart.Sql = ExpressionHelperBuild(_sqlExe.SqlFactory.DbProvider).GetOrderBySql(expression) + " desc";
    //        orderBySqlParts.Add(orderSqlPart);
    //        return this;
    //    }

    //    public DefaultQuery<T, R> OrderByDescending(string sortName)
    //    {
    //        OrderBySqlPart orderSqlPart = new OrderBySqlPart();
    //        orderSqlPart.Sql = _sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(sortName) + " desc";
    //        orderBySqlParts.Add(orderSqlPart);
    //        return this;
    //    }

    //    public DefaultQuery<T, R> Select(Expression<Func<T, object>> expression)
    //    {
    //        SelectSqlPart selectSqlPart = new SelectSqlPart();
    //        selectSqlPart.Sql = ExpressionHelperBuild(_sqlExe.SqlFactory.DbProvider).GetSelectSql(expression);
    //        selectSqlParts.Add(selectSqlPart);
    //        return this;
    //    }

    //    public DefaultQuery<T, R> GroupBy(Expression<Func<T, object>> expression)
    //    {
    //        GroupBySqlPart groupBySqlPart = new GroupBySqlPart();
    //        groupBySqlPart.Sql = ExpressionHelperBuild(_sqlExe.SqlFactory.DbProvider).GetGroupBySql(expression);
    //        groupBySqlParts.Add(groupBySqlPart);
    //        return this;
    //    }

    //    public DefaultQuery<T, R> Having(Expression<Func<T, bool>> expression)
    //    {
    //        HavingSqlPart havingSqlPart = new HavingSqlPart();
    //        havingSqlPart.Sql = ExpressionHelperBuild(_sqlExe.SqlFactory.DbProvider, havingSqlPart.Paramters, havingSqlParts.Sum(op => op.Paramters.Count) + whereSqlParts.Sum(op => op.Paramters.Count)).GetHavingSql(expression);
    //        havingSqlParts.Add(havingSqlPart);
    //        return this;
    //    }

    //    public DefaultQuery<T, R> Distinct()
    //    {
    //        DistinctSqlPart distinctSqlPart = new DistinctSqlPart();
    //        distinctSqlPart.Sql = "distinct";
    //        distinctSqlParts.Add(distinctSqlPart);
    //        return this;
    //    }

    //    public IList<R> ToList()
    //    {
    //        return _sqlExe.Query<R>(ResolveSqlPart(), ResolveParamter()).ToList();
    //    }

    //    public IList<R> ToPaged(int pageIndex, int pageSize)
    //    {
    //        return _sqlExe.Query<R>(ResolveSqlPart(), ResolveParamter(), pageIndex, pageSize).ToList();
    //    }

    //    public IList<R> ToPaged(int pageIndex, int pageSize, out int total)
    //    {
    //        return _sqlExe.Query<R>(ResolveSqlPart(), ResolveParamter(), pageIndex, pageSize, out total).ToList();
    //    }

    //    public R FirstOrDefault()
    //    {
    //        return _sqlExe.QueryFirstOrDefault<R>(ResolveSqlPart(), ResolveParamter());
    //    }

    //    public string ResolveSqlPart()
    //    {
    //        StringBuilder sbTemp = new StringBuilder();
    //        sbTemp.Append("select");
    //        if (distinctSqlParts.Any()) sbTemp.Append(" " + distinctSqlParts.First().Sql);
    //        if (selectSqlParts.Any()) sbTemp.Append(" " + string.Join(",", selectSqlParts.Select(op => op.Sql)));
    //        else sbTemp.Append(" " + string.Join(",", GetColumnsForSelect()));
    //        sbTemp.Append(" from");
    //        sbTemp.Append(" " + GetTable());
    //        if (whereSqlParts.Any())
    //        {
    //            sbTemp.Append(" where " + string.Join(" and ", whereSqlParts.Select(op => op.Sql)));
    //        }
    //        if (groupBySqlParts.Any()) sbTemp.Append(" group by " + string.Join(",", groupBySqlParts.Select(op => op.Sql)));
    //        if (havingSqlParts.Any())
    //        {
    //            sbTemp.Append(" having " + string.Join(" and ", havingSqlParts.Select(op => op.Sql)));
    //        }
    //        if (orderBySqlParts.Any()) sbTemp.Append(" order by " + string.Join(",", orderBySqlParts.Select(op => op.Sql)));
    //        return sbTemp.ToString();
    //    }

    //    public Dictionary<string, object> ResolveParamter()
    //    {
    //        List<DbParamter> paramters = new List<DbParamter>();
    //        if (whereSqlParts.Any())
    //        {
    //            whereSqlParts.ForEach(op => { paramters.AddRange(op.Paramters); });
    //        }
    //        if (havingSqlParts.Any())
    //        {
    //            havingSqlParts.ForEach(op => { paramters.AddRange(op.Paramters); });
    //        }

    //        var result = new Dictionary<string, object>();
    //        paramters.ForEach(op => { result.Add(op.Name, op.Value); });
    //        return result;
    //    }

    //    public DefaultQuery<T, R> Set(Expression<Func<T, object>> expression, bool isModify = true)
    //    {
    //        SetSqlPart setSqlPart = new SetSqlPart();
    //        setSqlPart.Sql = ExpressionHelperBuild(_sqlExe.SqlFactory.DbProvider).GetSetSql(expression);
    //        setSqlPart.IsModify = isModify;
    //        setSqlParts.Add(setSqlPart);
    //        return this;
    //    }

    //    public int Insert(T entity)
    //    {
    //        var cols = GetColumns();
    //        if (setSqlParts.Any())
    //        {
    //            if (setSqlParts.Any(op => op.IsModify))
    //            {
    //                var sets = new List<string>();
    //                setSqlParts.Where(o => o.IsModify).ToList().ForEach(op => { sets.AddRange(op.Sql.Split(',')); });
    //                cols = cols.Where(op => sets.Any(x => x == op)).ToList();
    //            }
    //            else if (setSqlParts.Any(op => !op.IsModify))
    //            {
    //                var sets = new List<string>();
    //                setSqlParts.Where(o => !o.IsModify).ToList().ForEach(op => { sets.AddRange(op.Sql.Split(',')); });
    //                cols = cols.Where(op => !sets.Any(x => x == op)).ToList();
    //            }
    //        }
    //        string sql = $"insert into {GetTable()}({string.Join(",", cols)}) values(#{string.Join("#,#", cols.Select(op => op.Replace("\"", "")))}#)";
    //        var sod = EntityToDict(entity);
    //        return _sqlExe.Execute(sql, sod);
    //    }

    //    public int Update(T entity)
    //    {
    //        var cols = GetColumns();
    //        var keys = GetPrimaryKeys();
    //        cols = cols.Where(op => !keys.Any(x => x == op)).ToList();
    //        if (setSqlParts.Any())
    //        {
    //            if (setSqlParts.Any(op => op.IsModify))
    //            {
    //                var sets = new List<string>();
    //                setSqlParts.Where(o => o.IsModify).ToList().ForEach(op => { sets.AddRange(op.Sql.Split(',')); });
    //                cols = cols.Where(op => sets.Any(x => x == op)).ToList();
    //            }
    //            else if (setSqlParts.Any(op => !op.IsModify))
    //            {
    //                var sets = new List<string>();
    //                setSqlParts.Where(o => !o.IsModify).ToList().ForEach(op => { sets.AddRange(op.Sql.Split(',')); });
    //                cols = cols.Where(op => !sets.Any(x => x == op)).ToList();
    //            }
    //        }
    //        var sql = $"update {GetTable()} set {string.Join(",", cols.Select(op => op + "=#" + op.Replace("\"", "") + "#"))}";
    //        var sod = EntityToDict(entity);
    //        if (whereSqlParts.Any())
    //        {
    //            sql += " where " + string.Join(" and ", whereSqlParts.Select(op => op.Sql));
    //            whereSqlParts.ForEach(op =>
    //            {
    //                op.Paramters.ForEach(o => sod.Add(o.Name, o.Value));
    //            });
    //        }
    //        else
    //        {
    //            sql += " where " + string.Join(" and ", keys.Select(op => op + "=#" + op.Replace("\"", "") + "#"));
    //        }
    //        return _sqlExe.Execute(sql, sod);
    //    }

    //    public int Delete(string id)
    //    {
    //        var keys = GetPrimaryKeys();
    //        string sql = $"delete from {GetTable()} where {string.Join(" and ", keys.Select(op => op + "=#" + op.Replace("\"", "") + "#"))}";
    //        return _sqlExe.Execute(sql, id);
    //    }

    //    public int Delete()
    //    {
    //        string sql = $"delete from {GetTable()}";
    //        var sod = new Dictionary<string, object>();
    //        if (whereSqlParts.Any())
    //        {
    //            sql += " where " + string.Join(" and ", whereSqlParts.Select(op => op.Sql));
    //            whereSqlParts.ForEach(op =>
    //            {
    //                op.Paramters.ForEach(o => sod.Add(o.Name, o.Value));
    //            });
    //        }
    //        return _sqlExe.Execute(sql, sod);
    //    }

    //    private string GetTable()
    //    {
    //        return _sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(TableMap.GetMap<T>().Name);
    //    }

    //    private IList<string> GetColumns()
    //    {
    //        IList<string> columns = new List<string>();
    //        var map = TableMap.GetMap<T>();
    //        map.Columns.ToList().ForEach(op =>
    //        {
    //            columns.Add(_sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(op.Name));
    //        });
    //        return columns;
    //    }

    //    private IList<string> GetColumnsForSelect()
    //    {
    //        var map = TableMap.GetMap<T>();
    //        IList<string> columns = new List<string>();
    //        map.Columns.ToList().ForEach(op =>
    //        {
    //            columns.Add($"{_sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(op.Name)} as {_sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(op.PropertyName)}");
    //        });
    //        return columns;
    //    }

    //    private IList<string> GetPrimaryKeys()
    //    {
    //        IList<string> columns = new List<string>();
    //        var map = TableMap.GetMap<T>();
    //        map.Columns.Where(op => op.IsPrimaryKey).ToList().ForEach(op =>
    //        {
    //            columns.Add(_sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(op.Name));
    //        });
    //        return columns;
    //    }

    //    private Dictionary<string, object> EntityToDict(T entity)
    //    {
    //        if (entity == null) return new Dictionary<string, object>();
    //        var sod = new Dictionary<string, object>();
    //        entity.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
    //            .ToList().ForEach(op => sod.Add(op.Name, op.GetValue(entity, null)));
    //        var map = TableMap.GetMap<T>();
    //        var sod1 = new Dictionary<string, object>();
    //        map.Columns.ToList().ForEach(op =>
    //        {
    //            sod1.Add(op.Name, sod[op.PropertyName]);
    //        });
    //        return sod1;
    //    }
    //}

}