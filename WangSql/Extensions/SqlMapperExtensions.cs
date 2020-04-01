using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangSql
{
    public static class SqlMapperExtensions
    {
        public static DefaultQuery<T> From<T>(this ISqlExe sqlExe)
            where T : class
        {
            return new DefaultQuery<T>(sqlExe);
        }
    }
}