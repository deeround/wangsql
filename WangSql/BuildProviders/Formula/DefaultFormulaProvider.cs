using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql.BuildProviders.Formula
{
    public class DefaultFormulaProvider : IFormulaProvider
    {
        protected static readonly DbOperator _dbOperator = new DbOperator();

        #region MyRegion
        public DbOperator Avg(object value)
        {
            return _dbOperator;
        }

        public string Avg_method(object name)
        {
            return $"AVG({name})";
        }

        public DbOperator Count(object value)
        {
            return _dbOperator;
        }

        public string Count_method(object name)
        {
            return $"COUNT({name})";
        }

        public DbOperator Max(object value)
        {
            return _dbOperator;
        }

        public string Max_method(object name)
        {
            return $"MAX({name})";
        }

        public DbOperator Min(object value)
        {
            return _dbOperator;
        }

        public string Min_method(object name)
        {
            return $"MIN({name})";
        }

        public DbOperator Sum(object value)
        {
            return _dbOperator;
        }

        public string Sum_method(object name)
        {
            return $"SUM({name})";
        }
        #endregion

        #region MyRegion
        public DbOperator Compare(object value)
        {
            return _dbOperator;
        }

        public string Compare_method(object name)
        {
            return $"{name}";
        }

        public bool IsNotNull(object value)
        {
            return true;
        }

        public string IsNotNull_method(object name)
        {
            return $"{name} IS NOT NULL";
        }

        public bool IsNull(object value)
        {
            return true;
        }

        public string IsNull_method(object name)
        {
            return $"{name} IS NULL";
        }
        #endregion

        #region MyRegion
        public DbOperator Abs(object value)
        {
            return _dbOperator;
        }

        public string Abs_method(object value)
        {
            return $"ABS({value})";
        }

        public DbOperator Ceil(object value)
        {
            return _dbOperator;
        }

        public string Ceil_method(object value)
        {
            return $"CEIL({value})";
        }

        public DbOperator Floor(object value)
        {
            return _dbOperator;
        }

        public string Floor_method(object value)
        {
            return $"FLOOR({value})";
        }

        public DbOperator Mod(object value, decimal x)
        {
            return _dbOperator;
        }

        public string Mod_method(object value, decimal x)
        {
            return $"MOD({value},{x})";
        }

        public DbOperator Round(object value)
        {
            return _dbOperator;
        }

        public string Round_method(object value)
        {
            return $"ROUND({value})";
        }

        public DbOperator Round(object value, int s)
        {
            return _dbOperator;
        }

        public string Round_method(object value, int s)
        {
            return $"ROUND({value},{s})";
        }

        public DbOperator Trunc(object value, int s)
        {
            return _dbOperator;
        }

        public string Trunc_method(object value, int s)
        {
            return $"TRUNC({value},{s})";
        }
        #endregion

        #region MyRegion
        public DbOperator Concat(object value1, object value2)
        {
            return _dbOperator;
        }

        public string Concat_method(object value, object value2)
        {
            return $"CONCAT({value},{value2})";
        }

        public DbOperator Lower(object value)
        {
            return _dbOperator;
        }

        public string Lower_method(object value)
        {
            return $"LOWER({value})";
        }

        public DbOperator Upper(object value)
        {
            return _dbOperator;
        }

        public string Upper_method(object value)
        {
            return $"UPPER({value})";
        }

        public DbOperator Length(object value)
        {
            return _dbOperator;
        }

        public string Length_method(object value)
        {
            return $"LENGTH({value})";
        }

        public DbOperator Replace(object value, string o, string n)
        {
            return _dbOperator;
        }

        public string Replace_method(object value, string o, string n)
        {
            return $"Replace({value},{o},{n})";
        }
        #endregion

        #region MyRegion
        
        #endregion

        #region MyRegion
        public DbOperator Nvl(object value1, object value2)
        {
            return _dbOperator;
        }

        public string Nvl_method(object value1, object value2)
        {
            return $"NVL({value1},{value2})";
        }
        #endregion
    }
}
