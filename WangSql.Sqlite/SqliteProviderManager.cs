using System;
using WangSql.Abstract.Linq;
using WangSql.Abstract.Migrate;
using WangSql.Abstract.Paged;
using WangSql.Sqlite.Migrate;
using WangSql.Sqlite.Options;
using WangSql.Sqlite.Paged;

namespace WangSql.Sqlite
{
    public class SqliteProviderManager
    {
        private const string _name = "sqlite";
        //private const string _connectionString = "Data Source=wangsql.db;";
        private const string _connectionType = "System.Data.SQLite.SQLiteConnection,System.Data.SQLite";
        private const bool _useParameterPrefixInSql = true;
        private const bool _useParameterPrefixInParameter = true;
        private const string _parameterPrefix = "@";
        private const bool _useQuotationInSql = false;
        private const bool _debug = false;
        private const bool _autoCreateTable = false;

        public static void Init(string connectionString)
        {
            Init(_name + "_" + Guid.NewGuid().ToString("N"), connectionString);
        }
        public static void Init(string name, string connectionString)
        {
            Init(name, connectionString, _debug, _autoCreateTable);
        }
        public static void Init(string name, string connectionString, bool debug, bool autoCreateTable)
        {
            SqliteProviderOptions options = new SqliteProviderOptions(name, connectionString, _connectionType, _useParameterPrefixInSql, _useParameterPrefixInParameter, _parameterPrefix, _useQuotationInSql, debug, autoCreateTable);
            Init(options);
        }
        public static void Init(SqliteProviderOptions options)
        {
            DbProviderManager.Set(options);
            //注入覆盖
            var provider = DbProviderManager.Get(options.Name);
            provider.AddService<IPageProvider, SqlitePageProvider>();
            provider.AddService<IMigrateProvider, SqliteMigrateProvider>();
            //表映射配置
            if (options.TableMaps != null && options.TableMaps.Count > 0)
            {
                EntityUtil.SetMaps(options.TableMaps, options.Name);
            }
            //自动创建表
            if (options.AutoCreateTable)
            {
                var sqlMapper = new SqlMapper(options.Name);
                var migrate = sqlMapper.Migrate();
                migrate.Init(sqlMapper);
                migrate.CreateTable();
            }
        }
    }
}
