using WangSql.Abstract.Migrate;
using WangSql.Abstract.Paged;
using WangSql.DependencyInjection;
using WangSql.Sqlite.Migrate;
using WangSql.Sqlite.Paged;

namespace WangSql.Sqlite
{
    public class SqliteMapperManager
    {
        private const string _name = "sqlite";
        private const string _connectionString = "Data Source=wangsql.db;";
        private const string _connectionType = "System.Data.SQLite.SQLiteConnection,System.Data.SQLite";
        private const bool _useParameterPrefixInSql = true;
        private const bool _useParameterPrefixInParameter = true;
        private const string _parameterPrefix = "@";
        private const bool _useQuotationInSql = false;
        private const bool _debug = false;

        public static void Init(string connectionString)
        {
            Init(_name, connectionString);
        }
        public static void Init(string name, string connectionString)
        {
            Init(name, connectionString, _connectionType, _useParameterPrefixInSql, _useParameterPrefixInParameter, _parameterPrefix, _useQuotationInSql, _debug);
        }
        public static void Init(string name, string connectionString, string connectionType, bool useParameterPrefixInSql, bool useParameterPrefixInParameter, string parameterPrefix, bool useQuotationInSql, bool debug = false)
        {
            DbProviderManager.Set(name, connectionString, connectionType, useParameterPrefixInSql, useParameterPrefixInParameter, parameterPrefix, useQuotationInSql, debug);

            //注入覆盖
            IocManager.AddService<IPageProvider, PageProvider>(name);
            IocManager.AddService<IMigrateProvider, MigrateProvider>(name);
        }
    }
}
