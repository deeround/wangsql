using System;

namespace WangSql
{
    [Serializable]
    public class SqlException : Exception
    {
        public SqlException()
        {
        }

        public SqlException(string message)
            : base(message)
        {
        }

        public SqlException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}