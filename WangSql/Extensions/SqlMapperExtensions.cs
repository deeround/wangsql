using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangSql
{
    public static class SqlMapperExtensions
    {
        public static DefaultQuery<T> Entity<T>(this ISqlExe sqlExe) where T : class
        {
            return new DefaultQuery<T>(sqlExe);
        }

        public static DefaultQuery<T, R> Entity<T, R>(this ISqlExe sqlExe) where T : class where R : class
        {
            return new DefaultQuery<T, R>(sqlExe);
        }
    }
}