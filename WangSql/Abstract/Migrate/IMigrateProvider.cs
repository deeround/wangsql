using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql.Abstract.Migrate
{
    public interface IMigrateProvider : IProvider
    {
        void CreateTable();
    }
}
