using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangSql.BuildProviders.Migrate;

namespace WangSql
{
    public static class SqlMapperMigrateExtensions
    {
        public static IMigrateProvider Migrate(this ISqlExe sqlExe)
        {
            return sqlExe.SqlFactory.DbProvider.MigrateProvider.Instance(sqlExe);
        }
        public static IMigrateProvider Migrate(this ISqlMapper sqlMapper)
        {
            return sqlMapper.SqlFactory.DbProvider.MigrateProvider.Instance(sqlMapper);
        }
    }
}