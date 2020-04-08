using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using WangSql.BuildProviders.Formula;

namespace WangSql
{
    public class ExpressionHelper<T> where T : class
    {
        private DbProvider _dbProvider;
        private List<DbParamter> _dbParamters;
        private int _dbParamterStartIndex = 0;

        public ExpressionHelper(DbProvider dbProvider, List<DbParamter> dbParamters, int paramterStartIndex)
        {
            _dbProvider = dbProvider;
            _dbParamters = dbParamters ?? new List<DbParamter>();
            _dbParamterStartIndex = paramterStartIndex;
        }

        /// <summary>
        /// 路由计算
        /// </summary>
        private string ExpressionRouter(Expression exp)
        {
            var nodeType = exp.NodeType;
            if (exp is BinaryExpression)    //表示具有二进制运算符的表达式
            {
                return BinaryExpressionProvider(exp);
            }
            else if (exp is ConstantExpression) //表示具有常数值的表达式
            {
                return ConstantExpressionProvider(exp);
            }
            else if (exp is LambdaExpression)   //介绍 lambda 表达式。 它捕获一个类似于 .NET 方法主体的代码块
            {
                return LambdaExpressionProvider(exp);
            }
            else if (exp is MemberExpression)   //表示访问字段或属性
            {
                return MemberExpressionProvider(exp);
            }
            else if (exp is MethodCallExpression)   //表示对静态方法或实例方法的调用
            {
                return MethodCallExpressionProvider(exp);
            }
            else if (exp is NewArrayExpression) //表示创建一个新数组，并可能初始化该新数组的元素
            {
                return NewArrayExpressionProvider(exp);
            }
            if (exp is NewExpression) //表示一个构造函数调用
            {
                return NewExpressionProvider(exp);
            }
            else if (exp is ParameterExpression)    //表示一个命名的参数表达式。
            {
                return ParameterExpressionProvider(exp);
            }
            else if (exp is UnaryExpression)    //表示具有一元运算符的表达式
            {
                return UnaryExpressionProvider(exp);
            }
            return null;
        }

        /// <summary>
        /// 符号转换
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string ExpressionTypeCast(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return " and ";

                case ExpressionType.Equal:
                    return " = ";

                case ExpressionType.GreaterThan:
                    return " > ";

                case ExpressionType.GreaterThanOrEqual:
                    return " >= ";

                case ExpressionType.LessThan:
                    return " < ";

                case ExpressionType.LessThanOrEqual:
                    return " <= ";

                case ExpressionType.NotEqual:
                    return " <> ";

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return " or ";

                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return " + ";

                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return " - ";

                case ExpressionType.Divide:
                    return " / ";

                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return " * ";

                default:
                    return null;
            }
        }

        private string BinaryExpressionProvider(Expression exp)
        {
            BinaryExpression be = exp as BinaryExpression;
            Expression left = be.Left;
            Expression right = be.Right;
            ExpressionType type = be.NodeType;
            string sb = "(";

            //先处理左边
            sb += ExpressionRouter(left);

            //处理中间
            sb += ExpressionTypeCast(type);

            //再处理右边
            sb += ExpressionRouter(right);

            return sb += ")";
        }

        private string ConstantExpressionProvider(Expression exp)
        {
            ConstantExpression ce = exp as ConstantExpression;
            AddParamter(_dbParamters, ResolveValue(ce.Value));
            return "#para" + (_dbParamters.Count + _dbParamterStartIndex) + "#";
        }

        private string LambdaExpressionProvider(Expression exp)
        {
            LambdaExpression le = exp as LambdaExpression;
            return ExpressionRouter(le.Body);
        }

        private string MemberExpressionProvider(Expression exp)
        {
            if (!exp.ToString().StartsWith("value"))
            {
                MemberExpression me = exp as MemberExpression;
                return ResolveName(me.Member.Name);
            }
            else
            {
                var result = Expression.Lambda(exp).Compile().DynamicInvoke();
                if (result is ValueType || result is string)
                {
                    AddParamter(_dbParamters, ResolveValue(result));
                    return "#para" + (_dbParamters.Count + _dbParamterStartIndex) + "#";
                }
                else if (result is IEnumerable<object> rr)
                {
                    StringBuilder sbTmp = new StringBuilder();
                    foreach (var r in rr)
                    {
                        AddParamter(_dbParamters, r.ToString());
                        sbTmp.Append("#para" + (_dbParamters.Count + _dbParamterStartIndex) + "#,");
                    }
                    return sbTmp.ToString().Substring(0, sbTmp.ToString().Length - 1);
                }
                else
                {
                    return null;
                }
            }
        }

        private string MethodCallExpressionProvider(Expression exp)
        {
            MethodCallExpression mce = exp as MethodCallExpression;
            var reflectedType = mce.Method.ReflectedType;

            if (mce.Method.Name == "Contains")
            {
                if (mce.Object == null)
                {
                    return string.Format("{0} in ({1})", ExpressionRouter(mce.Arguments[1]), ExpressionRouter(mce.Arguments[0]));
                }
                else
                {
                    var name = ExpressionRouter(mce.Object);
                    var value = ExpressionRouter(mce.Arguments[0]);
                    _dbParamters[_dbParamters.Count - 1].Value = string.Format("%{0}%", _dbParamters[_dbParamters.Count - 1].Value);
                    return string.Format("{0} like {1}", name, value);
                }
            }
            else if (mce.Method.Name == "StartsWith")
            {
                var name = ExpressionRouter(mce.Object);
                var value = ExpressionRouter(mce.Arguments[0]);
                _dbParamters[_dbParamters.Count - 1].Value = string.Format("{0}%", _dbParamters[_dbParamters.Count - 1].Value);
                return string.Format("{0} like {1}", name, value);
            }
            else if (mce.Method.Name == "EndsWith")
            {
                var name = ExpressionRouter(mce.Object);
                var value = ExpressionRouter(mce.Arguments[0]);
                _dbParamters[_dbParamters.Count - 1].Value = string.Format("%{0}", _dbParamters[_dbParamters.Count - 1].Value);
                return string.Format("{0} like {1}", name, value);
            }
            else if (typeof(IFormulaProvider).IsAssignableFrom(reflectedType))//自定义函数
            {
                var name = ExpressionRouter(mce.Arguments[0]);
                MethodInfo methodInfo = typeof(IFormulaProvider).GetMethod(mce.Method.Name + "_method");//加载方法
                return methodInfo.Invoke(_dbProvider.FormulaProvider, new object[] { name })?.ToString();//执行
            }
            else
            {
                object result = Expression.Lambda(mce).Compile().DynamicInvoke();
                AddParamter(_dbParamters, ResolveValue(result));
                return "#para" + (_dbParamters.Count + _dbParamterStartIndex) + "#";
            }
        }

        private string NewArrayExpressionProvider(Expression exp)
        {
            NewArrayExpression ae = exp as NewArrayExpression;
            StringBuilder sbTmp = new StringBuilder();
            foreach (Expression ex in ae.Expressions)
            {
                sbTmp.Append(ExpressionRouter(ex));
                sbTmp.Append(",");
            }
            return sbTmp.ToString(0, sbTmp.Length - 1);
        }

        private string NewExpressionProvider(Expression exp)
        {
            NewExpression ne = exp as NewExpression;
            StringBuilder sbTmp = new StringBuilder();
            foreach (var m in ne.Members)
            {
                sbTmp.Append(ResolveName(m.Name));
                sbTmp.Append(",");
            }
            return sbTmp.ToString(0, sbTmp.Length - 1);
        }

        private string ParameterExpressionProvider(Expression exp)
        {
            ParameterExpression pe = exp as ParameterExpression;
            return ResolveName(pe.Type.Name);
        }

        private string UnaryExpressionProvider(Expression exp)
        {
            UnaryExpression ue = exp as UnaryExpression;
            var result = ExpressionRouter(ue.Operand);
            ExpressionType type = exp.NodeType;
            if (type == ExpressionType.Not)
            {
                if (result.Contains(" in "))
                {
                    result = result.Replace(" in ", " not in ");
                }
                if (result.Contains(" like "))
                {
                    result = result.Replace(" like ", " not like ");
                }
            }
            return result;
        }

        #region For Select

        private string NewExpressionProviderForSelect(Expression exp)
        {
            NewExpression ne = exp as NewExpression;
            StringBuilder sbTmp = new StringBuilder();
            foreach (var m in ne.Members)
            {
                sbTmp.Append(ResolveNameForSelect(m.Name));
                sbTmp.Append(",");
            }
            return sbTmp.ToString(0, sbTmp.Length - 1);
        }

        private string ParameterExpressionProviderForSelect(Expression exp)
        {
            ParameterExpression pe = exp as ParameterExpression;
            return ResolveNameForSelect(pe.Type.Name);
        }

        private string ResolveNameForSelect(string name)
        {
            var map = TableMap.GetMap<T>();
            if (map.Columns.Any(x => x.PropertyName == name))
            {
                name = _dbProvider.FormatQuotationForSql(map.Columns.First(x => x.PropertyName == name).Name) + " as " + _dbProvider.FormatQuotationForSql(name);
            }
            else
            {
                name = _dbProvider.FormatQuotationForSql(name);
            }
            return name;
        }

        #endregion For Select

        private string ResolveName(string name)
        {
            var map = TableMap.GetMap<T>();
            if (map.Columns.Any(x => x.PropertyName == name))
            {
                name = map.Columns.First(x => x.PropertyName == name).Name;
            }
            return _dbProvider.FormatQuotationForSql(name);
        }

        private object ResolveValue(object value)
        {
            return value;
        }

        private void AddParamter(List<DbParamter> paramters, object value)
        {
            DbParamter p = new DbParamter
            {
                Name = "para" + (paramters.Count + _dbParamterStartIndex + 1),
                Value = value
            };
            paramters.Add(p);
        }

        public string GetWhereSql(Expression<Func<T, bool>> expression)
        {
            string result = string.Empty;
            if (expression != null)
            {
                Expression exp = expression.Body as Expression;
                result = ExpressionRouter(exp);
            }
            return result;
        }

        public string GetOrderBySql(Expression<Func<T, object>> expression)
        {
            string result = string.Empty;
            if (expression != null)
            {
                Expression exp = expression.Body as Expression;
                List<DbParamter> paramters = new List<DbParamter>();
                result = ExpressionRouter(exp);
            }
            return result;
        }

        public string GetGroupBySql(Expression<Func<T, object>> expression)
        {
            string result = string.Empty;

            if (expression != null)
            {
                Expression exp = expression.Body as Expression;
                List<DbParamter> paramters = new List<DbParamter>();
                result = ExpressionRouter(exp);
            }
            return result;
        }

        public string GetHavingSql(Expression<Func<T, bool>> expression)
        {
            string result = string.Empty;
            if (expression != null)
            {
                Expression exp = expression.Body as Expression;
                result = ExpressionRouter(exp);
            }
            return result;
        }

        public string GetDistinctSql(Expression<Func<T, object>> expression)
        {
            string result = string.Empty;

            if (expression != null)
            {
                Expression exp = expression.Body as Expression;
                List<DbParamter> paramters = new List<DbParamter>();
                result = ExpressionRouter(exp);
            }
            return result;
        }

        public string GetSelectSql(Expression<Func<T, object>> expression)
        {
            string result = string.Empty;

            if (expression != null)
            {
                Expression exp = expression.Body as Expression;
                if (exp is NewExpression)
                {
                    result = NewExpressionProviderForSelect(exp as NewExpression);
                }
                else if (exp is ParameterExpression)
                {
                    result = ParameterExpressionProviderForSelect(exp as ParameterExpression);
                }
            }
            return result;
        }

        public string GetSetSql(Expression<Func<T, object>> expression)
        {
            string result = string.Empty;

            if (expression != null)
            {
                Expression exp = expression.Body as Expression;
                List<DbParamter> paramters = new List<DbParamter>();
                result = ExpressionRouter(exp);
            }
            return result;
        }
    }

    public class DbParamter
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}