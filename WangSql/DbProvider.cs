using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace WangSql
{
    public class DbProvider
    {
        /// <summary>
        /// 使用静态变量注意内存溢出
        /// </summary>
        private static readonly ConcurrentDictionary<string, Type> CacheConnectionType = new ConcurrentDictionary<string, Type>();
        private static readonly int CacheConnectionTypeLength = 1000;

        public DbProvider(
            string name,
            string connectionString,
            string connectionType,
            bool useParameterPrefixInSql,
            bool useParameterPrefixInParameter,
            string parameterPrefix,
            bool useQuotationInSql,
            bool debug)
        {
            Name = name;
            ConnectionString = connectionString;
            ConnectionType = connectionType;
            UseParameterPrefixInSql = useParameterPrefixInSql;
            UseParameterPrefixInParameter = useParameterPrefixInParameter;
            ParameterPrefix = parameterPrefix;
            UseQuotationInSql = useQuotationInSql;
            Debug = debug;
            BuildProvider = new DefaultBuildProvider();

            //oracle
            if (connectionType.ToLower().Contains("oracle"))
            {
                BuildProvider = new OracleBuildProvider();
            }
            //oracle
            else if (connectionType.ToLower().Contains("pgsql"))
            {
                BuildProvider = new PgsqlBuildProvider();
            }
        }

        public string ConnectionString { get; }
        public string ConnectionType { get; }
        public string Name { get; }
        public string ParameterPrefix { get; }
        public bool UseParameterPrefixInParameter { get; }
        public bool UseParameterPrefixInSql { get; }
        public bool UseQuotationInSql { get; }
        public bool Debug { get; }
        public IBuildProvider BuildProvider { get; }

        public IDbConnection CreateConnection()
        {
            var type = GetCacheType();
            var conn = (IDbConnection)Activator.CreateInstance(type);
            conn.ConnectionString = ConnectionString;
            return conn;
        }

        public string FormatNameForParameter(string parameterName)
        {
            return UseParameterPrefixInParameter ? ParameterPrefix + parameterName : parameterName;
        }

        public string FormatNameForSql(string parameterName)
        {
            return UseParameterPrefixInSql ? ParameterPrefix + parameterName : parameterName;
        }

        public string FormatQuotationForSql(string parameterName)
        {
            return UseQuotationInSql ? "\"" + parameterName + "\"" : parameterName;
        }

        private Type GetCacheType()
        {
            var code = Utils.GetHashCode(this);
            if (CacheConnectionType.ContainsKey(code))
            {
                return CacheConnectionType[code];
            }

            var type = Type.GetType(ConnectionType);
            if (type == null) throw new SqlException(ConnectionType + "加载失败");
            if (CacheConnectionType.Count > CacheConnectionTypeLength)
            {
                CacheConnectionType.Clear();
            }
            CacheConnectionType[code] = type;
            return type;
        }
    }
}