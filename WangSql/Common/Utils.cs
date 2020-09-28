using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace WangSql
{
    internal static class Utils
    {
        private static MD5 md5 = MD5.Create();
        internal static string GetHashCode(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] source = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str));
            for (int i = 0; i < source.Length; i++)
            {
                sb.Append(source[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}