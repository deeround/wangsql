using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql.Sqlite.Options
{
    public class SqliteProviderOptions : DbProviderOptions
    {
        public SqliteProviderOptions()
        {
        }

        public SqliteProviderOptions(string name, string connectionString, string connectionType, bool useParameterPrefixInSql, bool useParameterPrefixInParameter, string parameterPrefix, bool useQuotationInSql, bool debug) : base(name, connectionString, connectionType, useParameterPrefixInSql, useParameterPrefixInParameter, parameterPrefix, useQuotationInSql, debug)
        {
        }
    }
}
