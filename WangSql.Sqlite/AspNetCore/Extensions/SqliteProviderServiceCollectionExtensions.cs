using System;
using WangSql.Sqlite;
using WangSql.Sqlite.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SqliteProviderServiceCollectionExtensions
    {
        public static IServiceCollection AddSqliteProvider(this IServiceCollection services, string connectionString)
        {
            SqliteProviderManager.Init(connectionString);
            return services;
        }
        public static IServiceCollection AddSqliteProvider(this IServiceCollection services, string name, string connectionString)
        {
            SqliteProviderManager.Init(name, connectionString);
            return services;
        }
        public static IServiceCollection AddSqliteProvider(this IServiceCollection services, string name, string connectionString, bool debug, bool autoCreateTable)
        {
            SqliteProviderManager.Init(name, connectionString, debug, autoCreateTable);
            return services;
        }
        public static IServiceCollection AddSqliteProvider(this IServiceCollection services, Action<SqliteProviderOptions> optionSetup)
        {
            string _name = "sqlite";
            string _connectionString = "";
            string _connectionType = "System.Data.SQLite.SQLiteConnection,System.Data.SQLite";
            bool _useParameterPrefixInSql = true;
            bool _useParameterPrefixInParameter = true;
            string _parameterPrefix = "@";
            bool _useQuotationInSql = false;
            bool _autoCreateTable = false;
            bool _debug = false;
            SqliteProviderOptions options = new SqliteProviderOptions(_name, _connectionString, _connectionType, _useParameterPrefixInSql, _useParameterPrefixInParameter, _parameterPrefix, _useQuotationInSql, _debug, _autoCreateTable);
            optionSetup?.Invoke(options);
            return services.AddSqliteProvider(options);
        }
        public static IServiceCollection AddSqliteProvider(this IServiceCollection services, SqliteProviderOptions options)
        {
            SqliteProviderManager.Init(options);
            return services;
        }
    }
}