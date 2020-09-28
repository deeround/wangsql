using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql.BuildProviders.Formula
{
    public class DbOperator
    {

        public static bool operator ==(DbOperator value1, object value2)
        {
            return false;
        }

        public static bool operator !=(DbOperator value1, object value2)
        {
            return false;
        }

        public static bool operator >(DbOperator value1, object value2)
        {
            return false;
        }

        public static bool operator <(DbOperator value1, object value2)
        {
            return false;
        }

        public static bool operator >=(DbOperator value1, object value2)
        {
            return false;
        }

        public static bool operator <=(DbOperator value1, object value2)
        {
            return false;
        }

        public override bool Equals(object obj)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
