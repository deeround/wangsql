using System;

namespace WangSql
{
    [Serializable]
    public class SqlMigrateException : SqlException
    {
        public SqlMigrateException()
        {
        }

        public SqlMigrateException(string message)
            : base(message)
        {
        }

        public SqlMigrateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}