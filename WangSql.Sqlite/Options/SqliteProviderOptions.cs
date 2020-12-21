using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql.Sqlite.Options
{
    public class SqliteProviderOptions : DbProviderOptions
    {
        /// <summary>
        /// 表实体映射
        /// </summary>
        public IList<Type> TableMaps { get; set; }
        /// <summary>
        /// 自动创建表
        /// </summary>
        public bool AutoCreateTable { get; set; }

        public SqliteProviderOptions() : base()
        {
        }

        public SqliteProviderOptions(string name, string connectionString, string connectionType, bool useParameterPrefixInSql, bool useParameterPrefixInParameter, string parameterPrefix, bool useQuotationInSql, bool debug) : base(name, connectionString, connectionType, useParameterPrefixInSql, useParameterPrefixInParameter, parameterPrefix, useQuotationInSql, debug)
        {
        }

        public SqliteProviderOptions(string name, string connectionString, string connectionType, bool useParameterPrefixInSql, bool useParameterPrefixInParameter, string parameterPrefix, bool useQuotationInSql, bool debug, bool autoCreateTable) : base(name, connectionString, connectionType, useParameterPrefixInSql, useParameterPrefixInParameter, parameterPrefix, useQuotationInSql, debug)
        {
            AutoCreateTable = autoCreateTable;
        }
    }
}
