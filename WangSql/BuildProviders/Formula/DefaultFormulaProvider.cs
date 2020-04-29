using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql.BuildProviders.Formula
{
    public class DefaultFormulaProvider : IFormulaProvider
    {
        protected static readonly DbOperator _dbOperator = new DbOperator();

        private void ResolveParamter(FormulaParamter[] paramters)
        {
            foreach (var item in paramters)
            {
                if (item.IsConstant)//只有常数才需要处理
                {
                    var v = TypeMap.ResolveParamValue(item.Value);
                    var type = TypeMap.GetSimpleStandardType(v);
                    switch (type)
                    {
                        case SimpleStandardType.None:
                            break;
                        case SimpleStandardType.Numeric:
                            break;
                        case SimpleStandardType.Varchar:
                            {
                                v = $"'{v}'";
                            }
                            break;
                        case SimpleStandardType.Text:
                            {
                                v = $"'{v}'";
                            }
                            break;
                        case SimpleStandardType.Char:
                            {
                                v = $"'{v}'";
                            }
                            break;
                        case SimpleStandardType.Enum:
                            break;
                        case SimpleStandardType.DateTime:
                            {
                                v = TypeMap.ConvertToType(v, typeof(string));
                                v = $"'{v}'";
                            }
                            break;
                        case SimpleStandardType.DateTimeOffset:
                            {
                                v = TypeMap.ConvertToType(v, typeof(string));
                                v = $"'{v}'";
                            }
                            break;
                        case SimpleStandardType.Boolean:
                            break;
                    }
                    item.Value = v;
                }
            }
        }

        #region MyRegion
        public DbOperator Avg(object value)
        {
            return _dbOperator;
        }

        public virtual string Avg_method(FormulaParamter[] paramters)
        {
            ResolveParamter(paramters);
            return $"AVG({paramters[0].Value})";
        }

        public DbOperator Count(object value)
        {
            return _dbOperator;
        }

        public virtual string Count_method(FormulaParamter[] paramters)
        {
            ResolveParamter(paramters);
            return $"COUNT({paramters[0].Value})";
        }

        public DbOperator Max(object value)
        {
            return _dbOperator;
        }

        public virtual string Max_method(FormulaParamter[] paramters)
        {
            ResolveParamter(paramters);
            return $"MAX({paramters[0].Value})";
        }

        public DbOperator Min(object value)
        {
            return _dbOperator;
        }

        public virtual string Min_method(FormulaParamter[] paramters)
        {
            ResolveParamter(paramters);
            return $"MIN({paramters[0].Value})";
        }

        public DbOperator Sum(object value)
        {
            return _dbOperator;
        }

        public virtual string Sum_method(FormulaParamter[] paramters)
        {
            ResolveParamter(paramters);
            return $"SUM({paramters[0].Value})";
        }
        #endregion

        #region MyRegion
        public DbOperator Compare(object value)
        {
            return _dbOperator;
        }

        public virtual string Compare_method(FormulaParamter[] paramters)
        {
            return $"{paramters[0].Value}";
        }

        public bool IsNotNull(object value)
        {
            return true;
        }

        public virtual string IsNotNull_method(FormulaParamter[] paramters)
        {
            return $"{paramters[0].Value} IS NOT NULL";
        }

        public bool IsNull(object value)
        {
            return true;
        }

        public virtual string IsNull_method(FormulaParamter[] paramters)
        {
            return $"{paramters[0].Value} IS NULL";
        }
        #endregion

        #region MyRegion
        public DbOperator Abs(object value)
        {
            return _dbOperator;
        }

        public virtual string Abs_method(FormulaParamter[] paramters)
        {
            return $"ABS({paramters[0].Value})";
        }

        public DbOperator Ceil(object value)
        {
            return _dbOperator;
        }

        public virtual string Ceil_method(FormulaParamter[] paramters)
        {
            return $"CEIL({paramters[0].Value})";
        }

        public DbOperator Floor(object value)
        {
            return _dbOperator;
        }

        public virtual string Floor_method(FormulaParamter[] paramters)
        {
            return $"FLOOR({paramters[0].Value})";
        }

        public DbOperator Mod(object value, decimal x)
        {
            return _dbOperator;
        }

        public virtual string Mod_method(FormulaParamter[] paramters)
        {
            return $"MOD({paramters[0].Value},{paramters[1].Value})";
        }

        public DbOperator Round(object value, int s)
        {
            return _dbOperator;
        }

        public virtual string Round_method(FormulaParamter[] paramters)
        {
            return $"ROUND({paramters[0].Value},{paramters[1].Value})";
        }

        public DbOperator Trunc(object value, int s)
        {
            return _dbOperator;
        }

        public virtual string Trunc_method(FormulaParamter[] paramters)
        {
            return $"TRUNC({paramters[0].Value},{paramters[1].Value})";
        }
        #endregion

        #region MyRegion
        public DbOperator Concat(object value1, object value2)
        {
            return _dbOperator;
        }

        public virtual string Concat_method(FormulaParamter[] paramters)
        {
            return $"CONCAT({paramters[0].Value},{paramters[1].Value})";
        }

        public DbOperator Lower(object value)
        {
            return _dbOperator;
        }

        public virtual string Lower_method(FormulaParamter[] paramters)
        {
            return $"LOWER({paramters[0].Value})";
        }

        public DbOperator Upper(object value)
        {
            return _dbOperator;
        }

        public virtual string Upper_method(FormulaParamter[] paramters)
        {
            return $"UPPER({paramters[0].Value})";
        }

        public DbOperator Length(object value)
        {
            return _dbOperator;
        }

        public virtual string Length_method(FormulaParamter[] paramters)
        {
            return $"LENGTH({paramters[0].Value})";
        }

        public DbOperator Replace(object value, string o, string n)
        {
            return _dbOperator;
        }

        public virtual string Replace_method(FormulaParamter[] paramters)
        {
            return $"Replace({paramters[0].Value},{paramters[1].Value},{paramters[2].Value})";
        }
        #endregion

        #region MyRegion
        public DbOperator ToChar(object value, string f)
        {
            return _dbOperator;
        }

        public virtual string ToChar_method(FormulaParamter[] paramters)
        {
            return $"TO_CHAR({paramters[0].Value},'{paramters[1].Value}')";
        }

        public DbOperator ToNumber(object value, string f)
        {
            return _dbOperator;
        }

        public virtual string ToNumber_method(FormulaParamter[] paramters)
        {
            return $"TO_NUMBER({paramters[0].Value},'{paramters[1].Value}')";
        }

        public DbOperator ToDate(object value, string f)
        {
            return _dbOperator;
        }

        public virtual string ToDate_method(FormulaParamter[] paramters)
        {
            return $"TO_DATE({paramters[0].Value},'{paramters[1].Value}')";
        }
        #endregion

        #region MyRegion
        public DbOperator Nvl(object value1, object value2)
        {
            return _dbOperator;
        }

        public virtual string Nvl_method(FormulaParamter[] paramters)
        {
            return $"NVL({paramters[0].Value},{paramters[1].Value})";
        }
        #endregion

        #region MyRegion
        //public DbOperator Fun(string name)
        //{
        //    return _dbOperator;
        //}

        //public string Fun_method(string name)
        //{
        //    return $"{name.ToUpper()}()";
        //}

        //public DbOperator Fun(string name, object value)
        //{
        //    return _dbOperator;
        //}

        //public string Fun_method(string name, object value)
        //{
        //    return $"{name.ToUpper()}({value})";
        //}

        //public DbOperator Fun(string name, object value1, object value2)
        //{
        //    return _dbOperator;
        //}

        //public string Fun_method(string name, object value1, object value2)
        //{
        //    return $"{name.ToUpper()}({value1},{value2})";
        //}

        //public DbOperator Fun(string name, object value1, object value2, object value3)
        //{
        //    return _dbOperator;
        //}

        //public string Fun_method(string name, object value1, object value2, object value3)
        //{
        //    return $"{name.ToUpper()}({value1},{value2},{value3})";
        //}
        #endregion
    }
}
