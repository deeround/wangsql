using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql.Abstract.Migrate
{
    public interface IMigrateProvider
    {
        void Init(ISqlMapper sqlMapper);
        void Init(ISqlExe sqlMapper);
    }
}
