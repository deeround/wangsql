using System;
using System.Collections.Generic;
using WangSql.Abstract.Linq;
using WangSql.Abstract.Migrate;
using WangSql.Abstract.Paged;
using WangSql.Sqlite.Migrate;
using WangSql.Sqlite.Paged;

namespace WangSql.Sqlite
{
    public class SqliteProviderManager
    {
        public static void Set(DbProviderOptions options, IList<Type> tableMaps = null, bool autoCreateTable = false)
        {
            DbProviderManager.Set(options);
            //注入覆盖
            var provider = DbProviderManager.Get(options.Name);
            provider.AddService<IPageProvider, SqlitePageProvider>();
            provider.AddService<IMigrateProvider, SqliteMigrateProvider>();
            //表映射配置
            if (tableMaps != null && tableMaps.Count > 0)
            {
                EntityUtil.SetMaps(tableMaps, options.Name);
            }
            //自动创建表
            if (autoCreateTable)
            {
                var sqlMapper = new SqlMapper(options.Name);
                var migrate = sqlMapper.Migrate();
                migrate.Init(sqlMapper);
                migrate.CreateTable();
            }
        }
    }
}
