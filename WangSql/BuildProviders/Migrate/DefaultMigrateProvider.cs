using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql.BuildProviders.Migrate
{
    public class DefaultMigrateProvider : IMigrateProvider
    {
        protected ISqlExe sqlExe;
        protected ISqlMapper sqlMapper;

        public virtual IMigrateProvider Instance(ISqlExe sqlExe)
        {
            this.sqlExe = sqlExe;
            return this;
        }

        public virtual IMigrateProvider Instance(ISqlMapper sqlMapper)
        {
            this.sqlMapper = sqlMapper;
            this.sqlExe = sqlMapper;
            return this;
        }

        public virtual void Run()
        {
            throw new SqlException("未实现");
        }
    }
}
