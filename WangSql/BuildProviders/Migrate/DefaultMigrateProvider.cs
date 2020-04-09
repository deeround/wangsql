using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql.BuildProviders.Migrate
{
    public class DefaultMigrateProvider : IMigrateProvider
    {
        public virtual void Run(SqlMapper sqlMapper)
        {
            throw new SqlException("未实现迁移工具");
        }
    }
}
