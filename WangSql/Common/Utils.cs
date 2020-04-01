using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace WangSql
{
    internal class Utils
    {
        internal static string GetHashCode(DbProvider dbProvider)
        {
            string codeStr = dbProvider.ConnectionString
                + dbProvider.ConnectionType
                + dbProvider.Name
                + dbProvider.ParameterPrefix
                + dbProvider.UseParameterPrefixInParameter.ToString()
                + dbProvider.UseParameterPrefixInSql.ToString()
                + dbProvider.UseQuotationInSql.ToString();

            byte[] SHA256Data = Encoding.UTF8.GetBytes(codeStr);
            SHA256Managed Sha256 = new SHA256Managed();
            byte[] by = Sha256.ComputeHash(SHA256Data);
            return BitConverter.ToString(by).Replace("-", "").ToLower();
        }
        internal static string GetHashCode(DbProvider dbProvider, string sql)
        {
            string codeStr = dbProvider.ConnectionString
                + dbProvider.ConnectionType
                + dbProvider.Name
                + dbProvider.ParameterPrefix
                + dbProvider.UseParameterPrefixInParameter.ToString()
                + dbProvider.UseParameterPrefixInSql.ToString()
                + dbProvider.UseQuotationInSql.ToString()
                + sql;

            byte[] SHA256Data = Encoding.UTF8.GetBytes(codeStr);
            SHA256Managed Sha256 = new SHA256Managed();
            byte[] by = Sha256.ComputeHash(SHA256Data);
            return BitConverter.ToString(by).Replace("-", "").ToLower();
        }
    }
}
