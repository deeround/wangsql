using System;
using System.Diagnostics;

namespace WangSql.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            SqliteTest.Test();
            //PgsqlTest.Test();
            //PgsqlTest1.Test();
            //OracleTest.Test();

            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }
    }
}
